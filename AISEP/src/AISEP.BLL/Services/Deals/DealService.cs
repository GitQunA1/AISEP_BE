using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;
using AISEP.BLL.Exceptions;
using AISEP.BLL.Helpers;
using AISEP.BLL.Services.BackgroundServices;
using AISEP.BLL.Services.Blockchain;
using AISEP.BLL.Services.Notifications;
using AISEP.BLL.Services.Storage;
using AISEP.DAL.Common;
using AISEP.DAL.Entities;
using AISEP.DAL.Enums;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using Sieve.Services;

namespace AISEP.BLL.Services.Deals
{
    public class DealService : IDealService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;
        private readonly IMapper _mapper;
        private readonly IBlockchainOwnershipAssignmentQueue _blockchainOwnershipAssignmentQueue;
        private readonly IBlockchainService _blockchainService;
        private readonly IStorageService _storageService;
        private readonly ISieveProcessor _sieveProcessor;

        public DealService(
            IUnitOfWork unitOfWork,
            INotificationService notificationService,
            IMapper mapper,
            IBlockchainOwnershipAssignmentQueue blockchainOwnershipAssignmentQueue,
            IBlockchainService blockchainService,
            IStorageService storageService,
            ISieveProcessor sieveProcessor)
        {
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
            _mapper = mapper;
            _blockchainOwnershipAssignmentQueue = blockchainOwnershipAssignmentQueue;
            _blockchainService = blockchainService;
            _storageService = storageService;
            _sieveProcessor = sieveProcessor;
        }

        public async Task<DealDto> CreateDealForInvestorAsync(int investorId, CreateDealDto dto)
        {
            if (dto is null)
            {
                throw new ArgumentNullException(nameof(dto));
            }

            _ = await _unitOfWork.Investors.GetByIdAsync(investorId)
                ?? throw new KeyNotFoundException("Investor not found.");

            var project = await _unitOfWork.Projects.GetByIdAsync(dto.ProjectId)
                ?? throw new KeyNotFoundException("Project not found.");

            var hasBlockingDeal = await _unitOfWork.Deals.HasBlockingDealAsync(investorId, dto.ProjectId);
            if (hasBlockingDeal)
            {
                throw new InvalidOperationException("You already have an active deal for this project.");
            }

            var (documentUrl, documentHash) = await UploadEvidenceAsync(dto.EvidenceFile, project.StartupId);

            var deal = _mapper.Map<Deal>(dto);
            deal.InvestorId = investorId;
            deal.ProjectId = dto.ProjectId;
            deal.DocumentUrl = documentUrl;
            deal.InitiatorRole = UserRole.Investor;
            deal.InvestorConfirmed = true;
            deal.StartupConfirmed = false;
            deal.Status = DealStatus.PendingCounterpartyConfirmation;
            deal.DealDate = DateTime.UtcNow;
            deal.IsCompleted = false;
            deal.DocumentHash = documentHash;

            await _unitOfWork.Deals.AddAsync(deal);
            await _unitOfWork.SaveChangesAsync();

            await _notificationService.SendNotificationAsync(
                project.Startup.UserId,
                "Yêu cầu xác nhận giao dịch ký kết",
                $"Nhà đầu tư đã khởi tạo giao dịch ký kết cho dự án '{project.ProjectName}'. Vui lòng xác nhận.",
                NotificationType.Deal,
                deal.DealId,
                "Deal");

            var created = await _unitOfWork.Deals.GetByIdWithDetailsAsync(deal.DealId)
                ?? throw new KeyNotFoundException("Created deal not found.");

            return _mapper.Map<DealDto>(created);
        }

        public async Task<DealDto> CreateDealForStartupAsync(int startupId, CreateDealDto dto)
        {
            if (dto is null)
            {
                throw new ArgumentNullException(nameof(dto));
            }

            var project = await _unitOfWork.Projects.GetByIdAsync(dto.ProjectId)
                ?? throw new KeyNotFoundException("Project not found.");

            if (project.StartupId != startupId)
            {
                throw new ForbiddenAccessException("You do not have permission to create a deal for this project.");
            }

            var connection = await GetLatestAcceptedConnectionAsync(startupId, dto.ProjectId);
            var investor = connection.Investor
                ?? throw new KeyNotFoundException("Investor not found.");

            var hasBlockingDeal = await _unitOfWork.Deals.HasBlockingDealAsync(investor.InvestorId, dto.ProjectId);
            if (hasBlockingDeal)
            {
                throw new InvalidOperationException("You already have an active deal for this project.");
            }

            var (documentUrl, documentHash) = await UploadEvidenceAsync(dto.EvidenceFile, project.StartupId);

            var deal = _mapper.Map<Deal>(dto);
            deal.InvestorId = investor.InvestorId;
            deal.ProjectId = dto.ProjectId;
            deal.DocumentUrl = documentUrl;
            deal.InitiatorRole = UserRole.Startup;
            deal.InvestorConfirmed = false;
            deal.StartupConfirmed = true;
            deal.Status = DealStatus.PendingCounterpartyConfirmation;
            deal.DealDate = DateTime.UtcNow;
            deal.IsCompleted = false;
            deal.DocumentHash = documentHash;

            await _unitOfWork.Deals.AddAsync(deal);
            await _unitOfWork.SaveChangesAsync();

            await _notificationService.SendNotificationAsync(
                investor.UserId,
                "Yêu cầu xác nhận giao dịch ký kết",
                $"Startup đã khởi tạo giao dịch ký kết cho dự án '{project.ProjectName}'. Vui lòng xác nhận.",
                NotificationType.Deal,
                deal.DealId,
                "Deal");

            var created = await _unitOfWork.Deals.GetByIdWithDetailsAsync(deal.DealId)
                ?? throw new KeyNotFoundException("Created deal not found.");

            return _mapper.Map<DealDto>(created);
        }

        public async Task<PagedResult<DealDto>> GetDealsAsync(SieveModel sieveModel)
        {
            sieveModel ??= new SieveModel();

            var query = _unitOfWork.Deals.GetQuery();

            return await PaginationHelper.PaginateAsync(
                query,
                sieveModel,
                _sieveProcessor,
                d => _mapper.Map<DealDto>(d));
        }

        public async Task<PagedResult<DealDto>> GetInvestorDealsAsync(int investorId, SieveModel sieveModel)
        {
            sieveModel ??= new SieveModel();

            var query = _unitOfWork.Deals.GetQuery()
                .Where(d => d.InvestorId == investorId);

            return await PaginationHelper.PaginateAsync(
                query,
                sieveModel,
                _sieveProcessor,
                d => _mapper.Map<DealDto>(d));
        }

        public async Task<PagedResult<DealDto>> GetStartupDealsAsync(int startupId, SieveModel sieveModel)
        {
            sieveModel ??= new SieveModel();

            var query = _unitOfWork.Deals.GetQuery()
                .Where(d => d.Project.StartupId == startupId);

            return await PaginationHelper.PaginateAsync(
                query,
                sieveModel,
                _sieveProcessor,
                d => _mapper.Map<DealDto>(d));
        }

        public async Task<DealDto> VerifyDealForInvestorAsync(int investorId, int dealId, VerifyDealRequestDto request)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (request.IsConfirmed is null)
            {
                throw new InvalidOperationException("IsConfirmed is required.");
            }

            var deal = await _unitOfWork.Deals.GetByIdWithDetailsAsync(dealId)
                ?? throw new KeyNotFoundException("Deal not found.");

            EnsureInvestorOwnsDeal(deal, investorId);
            EnsureCounterpartyRole(deal, UserRole.Investor);
            EnsureDealStatus(deal, DealStatus.PendingCounterpartyConfirmation, "Only pending deals can be verified.");

            return await HandleCounterpartyVerificationAsync(deal, UserRole.Investor, request.IsConfirmed.Value, request.Reason);
        }

        public async Task<DealDto> VerifyDealForStartupAsync(int startupId, int dealId, VerifyDealRequestDto request)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (request.IsConfirmed is null)
            {
                throw new InvalidOperationException("IsConfirmed is required.");
            }

            var deal = await _unitOfWork.Deals.GetByIdWithDetailsAsync(dealId)
                ?? throw new KeyNotFoundException("Deal not found.");

            EnsureStartupOwnsDeal(deal, startupId);
            EnsureCounterpartyRole(deal, UserRole.Startup);
            EnsureDealStatus(deal, DealStatus.PendingCounterpartyConfirmation, "Only pending deals can be verified.");

            return await HandleCounterpartyVerificationAsync(deal, UserRole.Startup, request.IsConfirmed.Value, request.Reason);
        }

        public async Task<DealDto> StaffReviewDealAsync(int dealId, StaffReviewDealRequestDto request)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (request.IsApproved is null)
            {
                throw new InvalidOperationException("IsApproved is required.");
            }

            var deal = await _unitOfWork.Deals.GetByIdWithDetailsAsync(dealId)
                ?? throw new KeyNotFoundException("Deal not found.");

            EnsureDealStatus(deal, DealStatus.PendingStaffApproval, "Only deals pending staff approval can be reviewed.");

            if (!request.IsApproved.Value)
            {
                deal.Status = DealStatus.RequireReupload;
                ResetConfirmationForInitiator(deal);

                _unitOfWork.Deals.Update(deal);
                await _unitOfWork.SaveChangesAsync();

                var reasonText = string.IsNullOrWhiteSpace(request.Reason)
                    ? "Vui lòng tải lại minh chứng để tiếp tục."
                    : $"Lý do: {request.Reason.Trim()}";

                await _notificationService.SendNotificationAsync(
                    GetInitiatorUserId(deal),
                    "Yêu cầu tải lại minh chứng",
                    $"Giao dịch cần tải lại minh chứng. {reasonText}",
                    NotificationType.Deal,
                    deal.DealId,
                    "Deal");

                return _mapper.Map<DealDto>(deal);
            }

            if (string.IsNullOrWhiteSpace(deal.DocumentUrl))
            {
                throw new InvalidOperationException("Deal evidence file is missing.");
            }

            deal.Status = DealStatus.ProcessingBlockchain;
            _unitOfWork.Deals.Update(deal);
            await _unitOfWork.SaveChangesAsync();

            await _blockchainOwnershipAssignmentQueue.QueueAsync(new DocumentOwnerAssignmentWorkItem(deal.DealId));

            await NotifyProcessingBlockchainAsync(deal);

            return _mapper.Map<DealDto>(deal);
        }

        public async Task<DealDto> ReuploadDealEvidenceForInvestorAsync(int investorId, int dealId, ReuploadDealEvidenceDto request)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var deal = await _unitOfWork.Deals.GetByIdWithDetailsAsync(dealId)
                ?? throw new KeyNotFoundException("Deal not found.");

            EnsureInvestorOwnsDeal(deal, investorId);
            EnsureInitiatorRole(deal, UserRole.Investor);
            EnsureDealAllowsReupload(deal);

            var (documentUrl, documentHash) = await UploadEvidenceAsync(request.EvidenceFile, deal.Project.StartupId);

            deal.DocumentUrl = documentUrl;
            deal.DocumentHash = documentHash;
            deal.BlockchainTxHash = null;
            deal.BlockchainVerifiedAt = null;
            deal.BlockchainErrorMessage = null;
            deal.Status = DealStatus.PendingCounterpartyConfirmation;
            ResetConfirmationForInitiator(deal);

            _unitOfWork.Deals.Update(deal);
            await _unitOfWork.SaveChangesAsync();

            await NotifyCounterpartyToVerifyAsync(deal);

            return _mapper.Map<DealDto>(deal);
        }

        public async Task<DealDto> ReuploadDealEvidenceForStartupAsync(int startupId, int dealId, ReuploadDealEvidenceDto request)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var deal = await _unitOfWork.Deals.GetByIdWithDetailsAsync(dealId)
                ?? throw new KeyNotFoundException("Deal not found.");

            EnsureStartupOwnsDeal(deal, startupId);
            EnsureInitiatorRole(deal, UserRole.Startup);
            EnsureDealAllowsReupload(deal);

            var (documentUrl, documentHash) = await UploadEvidenceAsync(request.EvidenceFile, deal.Project.StartupId);

            deal.DocumentUrl = documentUrl;
            deal.DocumentHash = documentHash;
            deal.BlockchainTxHash = null;
            deal.BlockchainVerifiedAt = null;
            deal.BlockchainErrorMessage = null;
            deal.Status = DealStatus.PendingCounterpartyConfirmation;
            ResetConfirmationForInitiator(deal);

            _unitOfWork.Deals.Update(deal);
            await _unitOfWork.SaveChangesAsync();

            await NotifyCounterpartyToVerifyAsync(deal);

            return _mapper.Map<DealDto>(deal);
        }

        private async Task<DealDto> HandleCounterpartyVerificationAsync(
            Deal deal,
            UserRole actorRole,
            bool isConfirmed,
            string? reason)
        {
            if (isConfirmed)
            {
                ApplyCounterpartyConfirmation(deal, actorRole);
                deal.Status = DealStatus.PendingStaffApproval;

                _unitOfWork.Deals.Update(deal);
                await _unitOfWork.SaveChangesAsync();

                await _notificationService.SendNotificationAsync(
                    GetInitiatorUserId(deal),
                    "Đối tác đã xác nhận giao dịch",
                    "Đối tác đã xác nhận giao dịch. Đang chờ nhân viên duyệt.",
                    NotificationType.Deal,
                    deal.DealId,
                    "Deal");

                return _mapper.Map<DealDto>(deal);
            }

            var rejectionReason = string.IsNullOrWhiteSpace(reason)
                ? "Đối tác đã từ chối giao dịch."
                : $"Đối tác đã từ chối giao dịch. Lý do: {reason.Trim()}";

            if (actorRole == UserRole.Investor)
            {
                deal.InvestorConfirmed = false;
            }
            else if (actorRole == UserRole.Startup)
            {
                deal.StartupConfirmed = false;
            }

            deal.Status = DealStatus.Canceled;

            _unitOfWork.Deals.Update(deal);
            await _unitOfWork.SaveChangesAsync();

            await _notificationService.SendNotificationAsync(
                GetInitiatorUserId(deal),
                "Giao dịch bị từ chối",
                rejectionReason,
                NotificationType.Deal,
                deal.DealId,
                "Deal");

            return _mapper.Map<DealDto>(deal);
        }

        private async Task NotifyProcessingBlockchainAsync(Deal deal)
        {
            const string message = "Giao dịch đã được duyệt và đang ghi nhận lên blockchain.";

            await _notificationService.SendNotificationAsync(
                deal.Investor.UserId,
                "Đang xử lý blockchain",
                message,
                NotificationType.Deal,
                deal.DealId,
                "Deal");

            await _notificationService.SendNotificationAsync(
                deal.Project.Startup.UserId,
                "Đang xử lý blockchain",
                message,
                NotificationType.Deal,
                deal.DealId,
                "Deal");
        }

        private async Task NotifyCounterpartyToVerifyAsync(Deal deal)
        {
            var counterpartyUserId = GetCounterpartyUserId(deal);
            var initiatorLabel = deal.InitiatorRole == UserRole.Investor ? "Nhà đầu tư" : "Startup";

            await _notificationService.SendNotificationAsync(
                counterpartyUserId,
                "Yêu cầu xác nhận giao dịch ký kết",
                $"{initiatorLabel} đã cập nhật minh chứng. Vui lòng xác nhận giao dịch.",
                NotificationType.Deal,
                deal.DealId,
                "Deal");
        }

        private async Task<(string DocumentUrl, string DocumentHash)> UploadEvidenceAsync(IFormFile file, int startupId)
        {
            if (file is null)
            {
                throw new InvalidOperationException("EvidenceFile is required.");
            }

            var documentHash = await _blockchainService.ComputeFileHashAsync(file);
            var documentUrl = await _storageService.UploadFileAsync(file, "deal-evidences");

            await _blockchainService.RegisterDocumentAsync(documentHash, startupId);

            return (documentUrl, documentHash);
        }

        private async Task<ConnectionRequest> GetLatestAcceptedConnectionAsync(int startupId, int projectId)
        {
            var connection = await _unitOfWork.ConnectionRequests.GetByStartupQuery(startupId)
                .Where(cr => cr.ProjectId == projectId && cr.Status == ConnectionRequestStatus.Accepted)
                .OrderByDescending(cr => cr.ResponseDate ?? DateTime.MinValue)
                .FirstOrDefaultAsync();

            return connection ?? throw new InvalidOperationException("No accepted connection found for this project.");
        }

        private static void EnsureCounterpartyRole(Deal deal, UserRole actorRole)
        {
            if (deal.InitiatorRole == actorRole)
            {
                throw new ForbiddenAccessException("You do not have permission to verify this deal.");
            }
        }

        private static void EnsureInitiatorRole(Deal deal, UserRole actorRole)
        {
            if (deal.InitiatorRole != actorRole)
            {
                throw new ForbiddenAccessException("You do not have permission to update this deal.");
            }
        }

        private static void EnsureDealStatus(Deal deal, DealStatus expectedStatus, string message)
        {
            if (deal.Status != expectedStatus)
            {
                throw new InvalidOperationException(message);
            }
        }

        private static void EnsureDealAllowsReupload(Deal deal)
        {
            if (deal.Status != DealStatus.RequireReupload && deal.Status != DealStatus.PendingCounterpartyConfirmation)
            {
                throw new InvalidOperationException("Deal is not allowed to reupload evidence in current status.");
            }
        }

        private static void ResetConfirmationForInitiator(Deal deal)
        {
            if (deal.InitiatorRole == UserRole.Investor)
            {
                deal.InvestorConfirmed = true;
                deal.StartupConfirmed = false;
            }
            else if (deal.InitiatorRole == UserRole.Startup)
            {
                deal.InvestorConfirmed = false;
                deal.StartupConfirmed = true;
            }
        }

        private static void ApplyCounterpartyConfirmation(Deal deal, UserRole actorRole)
        {
            if (actorRole == UserRole.Investor)
            {
                deal.InvestorConfirmed = true;
            }
            else if (actorRole == UserRole.Startup)
            {
                deal.StartupConfirmed = true;
            }
        }

        private static int GetInitiatorUserId(Deal deal)
        {
            return deal.InitiatorRole == UserRole.Investor
                ? deal.Investor.UserId
                : deal.Project.Startup.UserId;
        }

        private static int GetCounterpartyUserId(Deal deal)
        {
            return deal.InitiatorRole == UserRole.Investor
                ? deal.Project.Startup.UserId
                : deal.Investor.UserId;
        }

        private static void EnsureInvestorOwnsDeal(Deal deal, int investorId)
        {
            if (deal.InvestorId != investorId)
            {
                throw new ForbiddenAccessException("You do not have permission to access this deal.");
            }
        }

        private static void EnsureStartupOwnsDeal(Deal deal, int startupId)
        {
            if (deal.Project.StartupId != startupId)
            {
                throw new ForbiddenAccessException("You do not have permission to access this deal.");
            }
        }

        public async Task<DealDto> GetDealByIdAsync(int dealId)
        {
            var deal = await _unitOfWork.Deals.GetByIdWithDetailsAsync(dealId)
                ?? throw new KeyNotFoundException("Deal not found.");

            return _mapper.Map<DealDto>(deal);
        }

        public async Task<DealBlockchainVerificationResponse> GetDealOnChainVerificationAsync(int dealId)
        {
            var deal = await _unitOfWork.Deals.GetByIdWithDetailsAsync(dealId)
                ?? throw new KeyNotFoundException("Deal not found.");

            if (deal.Status == DealStatus.ProcessingBlockchain)
            {
                return new DealBlockchainVerificationResponse
                {
                    DealId = deal.DealId,
                    DocumentHash = deal.DocumentHash ?? string.Empty,
                    InvestorWallet = deal.Investor.WalletAddress?.Trim() ?? string.Empty,
                    StartupId = deal.Project.StartupId,
                    IsVerified = false,
                    Message = "Giao dịch đang được ghi nhận lên blockchain. Vui lòng chờ hoàn tất."
                };
            }

            if (deal.Status == DealStatus.BlockchainFailed)
            {
                var errorMessage = string.IsNullOrWhiteSpace(deal.BlockchainErrorMessage)
                    ? "Ghi nhận blockchain thất bại. Vui lòng thử lại sau."
                    : deal.BlockchainErrorMessage.Trim();

                return new DealBlockchainVerificationResponse
                {
                    DealId = deal.DealId,
                    DocumentHash = deal.DocumentHash ?? string.Empty,
                    InvestorWallet = deal.Investor.WalletAddress?.Trim() ?? string.Empty,
                    StartupId = deal.Project.StartupId,
                    IsVerified = false,
                    Message = errorMessage
                };
            }

            if (string.IsNullOrWhiteSpace(deal.DocumentUrl))
            {
                throw new InvalidOperationException("Deal evidence file is missing.");
            }

            var investorWallet = deal.Investor.WalletAddress?.Trim();
            if (string.IsNullOrWhiteSpace(investorWallet))
            {
                throw new InvalidOperationException("Investor wallet is missing.");
            }

            var fileHash = await _blockchainService.ComputeFileHashFromUrlAsync(deal.DocumentUrl);
            var (startupId, timestamp, owners) = await _blockchainService.VerifyDocumentAsync(fileHash);

            var ownerFound = owners?.Any(o => string.Equals(o?.Trim(), investorWallet, StringComparison.OrdinalIgnoreCase)) ?? false;

            var timestampText = timestamp > 0
                ? DateTimeOffset.FromUnixTimeSeconds(timestamp).UtcDateTime.ToString("yyyy-MM-dd HH:mm:ss UTC")
                : string.Empty;

            return new DealBlockchainVerificationResponse
            {
                DealId = deal.DealId,
                Message = ownerFound
                    ? "Giao dịch đã được xác minh trên blockchain."
                    : "Hệ thống không tìm thấy ghi nhận trên blockchain cho giao dịch này.",
                DocumentHash = fileHash,
                InvestorWallet = investorWallet,
                StartupId = startupId,
                TimestampOnBlockchain = timestampText,
                Owners = owners ?? Array.Empty<string>(),
                IsVerified = ownerFound
            };
        }
}

}

