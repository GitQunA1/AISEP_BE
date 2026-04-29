using AISEP.BLL.Services.Blockchain;
using AISEP.BLL.Services.Notifications;
using AISEP.DAL.Common;
using AISEP.DAL.Entities;
using AISEP.DAL.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AISEP.BLL.Services.BackgroundServices
{
    public class BlockchainOwnershipAssignmentBackgroundService : BackgroundService
    {
        private readonly IBlockchainOwnershipAssignmentQueue _queue;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<BlockchainOwnershipAssignmentBackgroundService> _logger;

        public BlockchainOwnershipAssignmentBackgroundService(
            IBlockchainOwnershipAssignmentQueue queue,
            IServiceScopeFactory scopeFactory,
            ILogger<BlockchainOwnershipAssignmentBackgroundService> logger)
        {
            _queue = queue;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("BlockchainOwnershipAssignmentBackgroundService started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                DocumentOwnerAssignmentWorkItem workItem;
                try
                {
                    workItem = await _queue.DequeueAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }

                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                    var blockchainService = scope.ServiceProvider.GetRequiredService<IBlockchainService>();
                    var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                    var deal = await unitOfWork.Deals.GetByIdWithDetailsAsync(workItem.DealId);
                    if (deal is null)
                    {
                        _logger.LogWarning("Deal not found for blockchain processing. DealId {DealId}", workItem.DealId);
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(deal.DocumentUrl))
                    {
                        _logger.LogWarning("Deal evidence file is missing. DealId {DealId}", deal.DealId);
                        await MarkBlockchainFailedAsync(unitOfWork, notificationService, deal, "Deal evidence file is missing.");
                        continue;
                    }

                    var investorWallet = deal.Investor.WalletAddress?.Trim();
                    if (string.IsNullOrWhiteSpace(investorWallet))
                    {
                        _logger.LogWarning("Investor wallet is missing. DealId {DealId}", deal.DealId);
                        await MarkBlockchainFailedAsync(unitOfWork, notificationService, deal, "Investor wallet is missing.");
                        continue;
                    }

                    var attempt = 0;
                    const int maxRetries = 2;
                    while (true)
                    {
                        try
                        {
                            _logger.LogInformation(
                                "Computing file hash for DealId {DealId}. Url: {DocumentUrl}",
                                deal.DealId,
                                deal.DocumentUrl);

                            var documentHash = string.IsNullOrWhiteSpace(deal.DocumentHash)
                                ? await blockchainService.ComputeFileHashFromUrlAsync(deal.DocumentUrl)
                                : deal.DocumentHash;

                            deal.DocumentHash = documentHash;

                            var (startupId, timestamp, _) = await blockchainService.VerifyDocumentAsync(documentHash);
                            if (timestamp == 0)
                            {
                                _logger.LogInformation(
                                    "Registering document hash on blockchain for DealId {DealId}. Hash: {DocumentHash}",
                                    deal.DealId,
                                    documentHash);

                                await blockchainService.RegisterDocumentAsync(documentHash, deal.Project.StartupId);
                            }

                            _logger.LogInformation(
                                "Assigning blockchain owner for DealId {DealId}. Hash: {DocumentHash}",
                                deal.DealId,
                                documentHash);

                            var txHash = await blockchainService.AssignDocumentOwnerAsync(documentHash, investorWallet);

                            deal.Status = DealStatus.Completed;
                            deal.IsCompleted = true;
                            deal.CompletionDate = DateTime.UtcNow;
                            deal.BlockchainTxHash = txHash;
                            deal.BlockchainVerifiedAt = DateTime.UtcNow;
                            deal.BlockchainErrorMessage = null;

                            unitOfWork.Deals.Update(deal);
                            await unitOfWork.SaveChangesAsync();

                            await NotifyBlockchainSuccessAsync(notificationService, deal, txHash);

                            _logger.LogInformation(
                                "Assigned document owner on blockchain for DealId {DealId}. TxHash: {TxHash}",
                                deal.DealId,
                                txHash);
                            break;
                        }
                        catch (Exception ex) when (attempt < maxRetries)
                        {
                            attempt++;
                            _logger.LogWarning(
                                ex,
                                "Blockchain assignment failed for DealId {DealId}. Retrying {Attempt}/{MaxRetries}.",
                                deal.DealId,
                                attempt,
                                maxRetries);

                            var delay = TimeSpan.FromSeconds(5 * attempt);
                            await Task.Delay(delay, stoppingToken);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(
                                ex,
                                "Blockchain assignment failed for DealId {DealId} after retries.",
                                deal.DealId);

                            await MarkBlockchainFailedAsync(unitOfWork, notificationService, deal, ex.Message);
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Failed to process blockchain assignment for DealId {DealId}.",
                        workItem.DealId);
                }
            }

            _logger.LogInformation("BlockchainOwnershipAssignmentBackgroundService stopped.");
        }

        private static async Task NotifyBlockchainSuccessAsync(
            INotificationService notificationService,
            Deal deal,
            string txHash)
        {
            var message = $"Giao dịch #{deal.DealId} đã được ghi nhận trên blockchain. Mã giao dịch: {txHash}.";

            await notificationService.SendNotificationAsync(
                deal.Investor.UserId,
                "Giao dịch đã hoàn tất",
                message,
                NotificationType.Deal,
                deal.DealId,
                "Deal");

            await notificationService.SendNotificationAsync(
                deal.Project.Startup.UserId,
                "Giao dịch đã hoàn tất",
                message,
                NotificationType.Deal,
                deal.DealId,
                "Deal");
        }

        private static async Task MarkBlockchainFailedAsync(
            IUnitOfWork unitOfWork,
            INotificationService notificationService,
            Deal deal,
            string errorMessage)
        {
            deal.Status = DealStatus.BlockchainFailed;
            deal.IsCompleted = false;
            deal.CompletionDate = null;
            deal.BlockchainErrorMessage = errorMessage;

            unitOfWork.Deals.Update(deal);
            await unitOfWork.SaveChangesAsync();

            const string message = "Hệ thống chưa thể ghi nhận giao dịch lên blockchain. Vui lòng liên hệ hỗ trợ để xử lý.";

            await notificationService.SendNotificationAsync(
                deal.Investor.UserId,
                "Giao dịch cần xử lý",
                message,
                NotificationType.Deal,
                deal.DealId,
                "Deal");

            await notificationService.SendNotificationAsync(
                deal.Project.Startup.UserId,
                "Giao dịch cần xử lý",
                message,
                NotificationType.Deal,
                deal.DealId,
                "Deal");
        }
    }
}
