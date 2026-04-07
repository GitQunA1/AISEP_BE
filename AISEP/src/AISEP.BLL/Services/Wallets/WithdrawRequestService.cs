using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;
using AISEP.BLL.Helpers;
using AISEP.BLL.Services.Notifications;
using AISEP.DAL.Common;
using AISEP.DAL.Entities;
using AISEP.DAL.Enums;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using Sieve.Services;

namespace AISEP.BLL.Services.Wallets
{
    public class WithdrawRequestService : IWithdrawRequestService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ISieveProcessor _sieveProcessor;
        private readonly INotificationService _notificationService;

        public WithdrawRequestService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ISieveProcessor sieveProcessor,
            INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _sieveProcessor = sieveProcessor;
            _notificationService = notificationService;
        }

        public async Task<WithdrawRequestResponse> CreateWithdrawRequestAsync(int userId, CreateWithdrawRequestDto dto)
        {
            var advisor = await _unitOfWork.Advisors.GetByUserIdAsync(userId)
                ?? throw new KeyNotFoundException("Advisor profile not found.");

            var wallet = await _unitOfWork.Wallets.GetByAdvisorIdAsync(advisor.AdvisorId)
                ?? throw new InvalidOperationException("Advisor wallet not found.");

            if (!wallet.IsActive)
                throw new InvalidOperationException("Advisor wallet is not active.");

            if (dto.Amount <= 0)
                throw new InvalidOperationException("Withdraw amount must be greater than 0.");

            var pendingAmount = await _unitOfWork.WithdrawRequests.GetPendingTotalByWalletIdAsync(wallet.WalletId);
            var availableForWithdraw = wallet.Balance - pendingAmount;
            if (availableForWithdraw < dto.Amount)
                throw new InvalidOperationException("Insufficient available balance for withdrawal request.");

            var request = new WithdrawRequest
            {
                WalletId = wallet.WalletId,
                Amount = Math.Round(dto.Amount, 2, MidpointRounding.AwayFromZero),
                BankName = dto.BankName?.Trim(),
                BankAccount = dto.BankAccount?.Trim(),
                Status = WithdrawRequestStatus.Pending,
                RequestedAt = DateTime.UtcNow
            };

            await _unitOfWork.WithdrawRequests.AddAsync(request);
            await _unitOfWork.SaveChangesAsync();

            await _unitOfWork.WalletTransactions.AddAsync(new WalletTransaction
            {
                WalletId = wallet.WalletId,
                WithdrawRequestId = request.WithdrawRequestId,
                Amount = request.Amount,
                Type = WalletTransactionType.Withdrawal,
                Status = WalletTransactionStatus.Pending,
                CreatedAt = DateTime.UtcNow
            });
            await _unitOfWork.SaveChangesAsync();

            await _notificationService.SendNotificationAsync(
                userId,
                "Withdrawal request submitted",
                $"Your withdrawal request for {request.Amount:0.##} has been submitted and is pending review.",
                NotificationType.General,
                request.WithdrawRequestId,
                "WithdrawRequest");
            await NotifyStaffAndAdminsAsync(
                "New withdrawal request",
                $"A new withdrawal request #{request.WithdrawRequestId} is waiting for approval.");

            var created = await _unitOfWork.WithdrawRequests.GetByIdAsync(request.WithdrawRequestId)
                ?? throw new InvalidOperationException("Failed to load created withdraw request.");
            return _mapper.Map<WithdrawRequestResponse>(created);
        }

        public async Task<PagedResult<WithdrawRequestResponse>> GetMyWithdrawRequestsAsync(int userId, SieveModel model)
        {
            var advisor = await _unitOfWork.Advisors.GetByUserIdAsync(userId)
                ?? throw new KeyNotFoundException("Advisor profile not found.");

            var query = _unitOfWork.WithdrawRequests.GetQuery()
                .Where(x => x.Wallet.AdvisorId == advisor.AdvisorId);

            return await PaginationHelper.PaginateAsync(query, model, _sieveProcessor, x => _mapper.Map<WithdrawRequestResponse>(x));
        }

        public async Task<PagedResult<WithdrawRequestResponse>> GetAllWithdrawRequestsAsync(SieveModel model)
        {
            var query = _unitOfWork.WithdrawRequests.GetQuery();
            return await PaginationHelper.PaginateAsync(query, model, _sieveProcessor, x => _mapper.Map<WithdrawRequestResponse>(x));
        }

        public async Task<WithdrawRequestResponse> ApproveWithdrawRequestAsync(int withdrawRequestId, int reviewerId, string? proofImageUrl)
        {
            var request = await _unitOfWork.WithdrawRequests.GetByIdAsync(withdrawRequestId)
                ?? throw new KeyNotFoundException("Withdraw request not found.");

            if (request.Status != WithdrawRequestStatus.Pending)
                throw new InvalidOperationException("Only pending withdraw requests can be approved.");

            var wallet = request.Wallet;
            if (!wallet.IsActive)
                throw new InvalidOperationException("Wallet is not active.");

            var pendingAmount = await _unitOfWork.WithdrawRequests.GetPendingTotalByWalletIdAsync(wallet.WalletId);
            var availableForWithdraw = wallet.Balance - (pendingAmount - request.Amount);
            if (availableForWithdraw < request.Amount)
                throw new InvalidOperationException("Insufficient wallet balance at approval time.");

            wallet.Balance -= request.Amount;
            _unitOfWork.Wallets.Update(wallet);

            request.Status = WithdrawRequestStatus.Approved;
            request.ApprovedAt = DateTime.UtcNow;
            request.ApprovedById = reviewerId;
            request.RejectedAt = null;
            request.RejectedById = null;
            request.RejectionReason = null;
            request.ProofImageUrl = string.IsNullOrWhiteSpace(proofImageUrl) ? null : proofImageUrl.Trim();
            _unitOfWork.WithdrawRequests.Update(request);

            var transaction = await _unitOfWork.WalletTransactions.GetWithdrawalByWithdrawRequestIdAsync(withdrawRequestId);
            if (transaction is null)
            {
                await _unitOfWork.WalletTransactions.AddAsync(new WalletTransaction
                {
                    WalletId = wallet.WalletId,
                    WithdrawRequestId = request.WithdrawRequestId,
                    Amount = request.Amount,
                    Type = WalletTransactionType.Withdrawal,
                    Status = WalletTransactionStatus.Completed,
                    CreatedAt = DateTime.UtcNow
                });
            }
            else
            {
                transaction.Status = WalletTransactionStatus.Completed;
            }

            await _unitOfWork.SaveChangesAsync();
            await _notificationService.SendNotificationAsync(
                request.Wallet.Advisor.UserId,
                "Withdrawal request approved",
                $"Your withdrawal request #{request.WithdrawRequestId} has been approved.",
                NotificationType.General,
                request.WithdrawRequestId,
                "WithdrawRequest");
            return _mapper.Map<WithdrawRequestResponse>(request);
        }

        public async Task<WithdrawRequestResponse> RejectWithdrawRequestAsync(int withdrawRequestId, int reviewerId, string? reason)
        {
            var request = await _unitOfWork.WithdrawRequests.GetByIdAsync(withdrawRequestId)
                ?? throw new KeyNotFoundException("Withdraw request not found.");

            if (request.Status != WithdrawRequestStatus.Pending)
                throw new InvalidOperationException("Only pending withdraw requests can be rejected.");

            request.Status = WithdrawRequestStatus.Rejected;
            request.RejectedAt = DateTime.UtcNow;
            request.RejectedById = reviewerId;
            request.RejectionReason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
            request.ApprovedAt = null;
            request.ApprovedById = null;
            _unitOfWork.WithdrawRequests.Update(request);

            var transaction = await _unitOfWork.WalletTransactions.GetWithdrawalByWithdrawRequestIdAsync(withdrawRequestId);
            if (transaction is not null)
            {
                transaction.Status = WalletTransactionStatus.Rejected;
            }

            await _unitOfWork.SaveChangesAsync();
            await _notificationService.SendNotificationAsync(
                request.Wallet.Advisor.UserId,
                "Withdrawal request rejected",
                $"Your withdrawal request #{request.WithdrawRequestId} was rejected. Reason: {request.RejectionReason ?? "N/A"}",
                NotificationType.General,
                request.WithdrawRequestId,
                "WithdrawRequest");
            return _mapper.Map<WithdrawRequestResponse>(request);
        }

        private async Task NotifyStaffAndAdminsAsync(string title, string message)
        {
            var reviewerIds = await _unitOfWork.Users.GetAllQuery()
                .Where(u => u.Role == UserRole.Staff || u.Role == UserRole.Admin)
                .Select(u => u.Id)
                .ToListAsync();

            foreach (var reviewerId in reviewerIds)
            {
                await _notificationService.SendNotificationAsync(
                    reviewerId,
                    title,
                    message,
                    NotificationType.System);
            }
        }
    }
}
