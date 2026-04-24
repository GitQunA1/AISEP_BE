using AISEP.BLL.Services.Blockchain;
using AISEP.BLL.Services.Notifications;
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
                    var blockchainService = scope.ServiceProvider.GetRequiredService<IBlockchainService>();
                    var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                    var txHash = await blockchainService.AssignDocumentOwnerAsync(
                        workItem.DocumentHash,
                        workItem.InvestorWallet);

                    await notificationService.SendNotificationAsync(
                        workItem.InvestorUserId,
                        "Đã chuyển giao quyền sở hữu tài liệu",
                        $"Thỏa thuận #{workItem.DealId} đã được ghi nhận chuyển giao quyền sở hữu tài liệu trên chuỗi khối cho ví {workItem.InvestorWallet}. Mã giao dịch: {txHash}",
                        NotificationType.Deal,
                        workItem.DealId,
                        "Deal");

                    _logger.LogInformation(
                        "Assigned document owner on blockchain for DealId {DealId}, ProjectId {ProjectId}. TxHash: {TxHash}",
                        workItem.DealId,
                        workItem.ProjectId,
                        txHash);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Failed to assign document owner on blockchain for DealId {DealId}, ProjectId {ProjectId}.",
                        workItem.DealId,
                        workItem.ProjectId);
                }
            }

            _logger.LogInformation("BlockchainOwnershipAssignmentBackgroundService stopped.");
        }
    }
}
