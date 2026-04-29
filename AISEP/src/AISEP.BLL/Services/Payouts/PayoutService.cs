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

namespace AISEP.BLL.Services.Payouts
{
    public class PayoutService : IPayoutService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISieveProcessor _sieveProcessor;
        private readonly IMapper _mapper;
        private readonly IPayoutGroupService _payoutGroupService;
        private readonly INotificationService _notificationService;

        public PayoutService(
            IUnitOfWork unitOfWork,
            ISieveProcessor sieveProcessor,
            IMapper mapper,
            IPayoutGroupService payoutGroupService,
            INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _sieveProcessor = sieveProcessor;
            _mapper = mapper;
            _payoutGroupService = payoutGroupService;
            _notificationService = notificationService;
        }

        public async Task<PayoutResponse> MarkPaidAsync(int payoutId, int staffUserId, MarkPayoutPaidRequest request)
        {
            var payout = await _unitOfWork.Payouts.GetByIdAsync(payoutId)
                ?? throw new KeyNotFoundException("Monthly payout not found.");

            if (payout.Status == MonthlyPayoutStatus.Paid)
            {
                return _mapper.Map<PayoutResponse>(payout);
            }

            if (payout.Status == MonthlyPayoutStatus.Rejected)
            {
                throw new InvalidOperationException("Rejected payout cannot be approved as paid.");
            }
            if (payout.Wallet.Balance < payout.Amount)
            {
                throw new InvalidOperationException("Wallet balance is not enough to mark this payout as paid.");
            }

            payout.Wallet.Balance = Math.Round(payout.Wallet.Balance - payout.Amount, 2, MidpointRounding.AwayFromZero);
            _unitOfWork.Wallets.Update(payout.Wallet);

            await _unitOfWork.WalletTransactions.AddAsync(new WalletTransaction
            {
                WalletId = payout.WalletId,
                Amount = payout.Amount,
                Type = WalletTransactionType.Payout,
                Status = WalletTransactionStatus.Completed,
                CreatedAt = DateTime.UtcNow,
                PayoutId = payout.PayoutId
            });

            var now = DateTime.UtcNow;
            await _unitOfWork.Transactions.AddAsync(new Transaction
            {
                UserId = payout.Wallet.Advisor.UserId,
                Amount = payout.Amount,
                Type = TransactionType.Withdraw,
                Status = TransactionStatus.Completed,
                CreatedAt = now,
                ReferenceType = ReferenceType.Payout.ToString(),
                ReferenceId = payout.PayoutId,
                PaymentContent = string.IsNullOrWhiteSpace(request.Note)
                    ? $"Payout to advisor for payout #{payout.PayoutId}"
                    : request.Note.Trim(),
                CompletedAt = now
            });

            payout.Status = MonthlyPayoutStatus.Paid;
            payout.PaidAt = now;
            payout.PaidById = staffUserId;
            payout.RejectedAt = null;
            payout.RejectedById = null;
            payout.RejectReason = null;
            payout.Note = string.IsNullOrWhiteSpace(request.Note) ? payout.Note : request.Note.Trim();

            _unitOfWork.Payouts.Update(payout);
            await _unitOfWork.SaveChangesAsync();

            if (payout.PayoutGroupId.HasValue)
            {
                await _payoutGroupService.RecalculateAsync(payout.PayoutGroupId.Value);
            }

            await NotifyAdvisorPaidAsync(payout);

            return _mapper.Map<PayoutResponse>(payout);
        }

        public async Task<PayoutResponse> RequestRetryAsync(int payoutId, int advisorUserId, RequestPayoutRetryRequest request)
        {
            var payout = await _unitOfWork.Payouts.GetByIdAsync(payoutId)
                ?? throw new KeyNotFoundException("Monthly payout not found.");

            if (payout.Wallet.Advisor.UserId != advisorUserId)
            {
                throw new UnauthorizedAccessException("You do not have permission to request retry for this payout.");
            }
            if (payout.Wallet.Advisor.ApprovalStatus != ApprovalStatus.Approved)
            {
                throw new InvalidOperationException("Your advisor profile must be approved before requesting payout retry.");
            }

            if (payout.Status != MonthlyPayoutStatus.Rejected)
            {
                throw new InvalidOperationException("Only rejected payouts can request retry.");
            }

            var resolutionNote = request.ResolutionNote.Trim();

            payout.Status = MonthlyPayoutStatus.PendingRecheck;
            payout.RetryRequestedAt = DateTime.UtcNow;
            payout.RetryRequestNote = resolutionNote;

            _unitOfWork.Payouts.Update(payout);
            await _unitOfWork.SaveChangesAsync();

            await NotifyStaffRetryRequestedAsync(payout);
            await NotifyAdvisorRetrySubmittedAsync(payout);

            if (payout.PayoutGroupId.HasValue)
            {
                await _payoutGroupService.RecalculateAsync(payout.PayoutGroupId.Value);
            }

            return _mapper.Map<PayoutResponse>(payout);
        }

        public async Task<PayoutResponse> RejectAsync(int payoutId, int staffUserId, RejectPayoutRequest request)
        {
            var payout = await _unitOfWork.Payouts.GetByIdAsync(payoutId)
                ?? throw new KeyNotFoundException("Monthly payout not found.");

            if (payout.Status == MonthlyPayoutStatus.Paid)
            {
                throw new InvalidOperationException("Paid payout cannot be rejected.");
            }

            var reason = request.Reason.Trim();

            payout.Status = MonthlyPayoutStatus.Rejected;
            payout.RejectedAt = DateTime.UtcNow;
            payout.RejectedById = staffUserId;
            payout.RejectReason = reason;
            payout.Note = string.IsNullOrWhiteSpace(request.Note) ? payout.Note : request.Note.Trim();

            _unitOfWork.Payouts.Update(payout);
            await _unitOfWork.SaveChangesAsync();

            if (payout.PayoutGroupId.HasValue)
            {
                await _payoutGroupService.RecalculateAsync(payout.PayoutGroupId.Value);
            }

            await NotifyAdvisorRejectedAsync(payout);

            return _mapper.Map<PayoutResponse>(payout);
        }

        public async Task<PagedResult<PayoutResponse>> GetAllAsync(SieveModel model)
        {
            var query = _unitOfWork.Payouts.GetQuery();
            return await PaginationHelper.PaginateAsync(
                query,
                model,
                _sieveProcessor,
                x => _mapper.Map<PayoutResponse>(x));
        }

        public async Task<PagedResult<PayoutResponse>> GetMineAsync(int advisorUserId, SieveModel model)
        {
            var advisor = await _unitOfWork.Advisors.GetByUserIdAsync(advisorUserId)
                ?? throw new KeyNotFoundException("Advisor profile not found.");

            var query = _unitOfWork.Payouts.GetQuery()
                .Where(x => x.Wallet.AdvisorId == advisor.AdvisorId);

            return await PaginationHelper.PaginateAsync(
                query,
                model,
                _sieveProcessor,
                x => _mapper.Map<PayoutResponse>(x));
        }

        private async Task NotifyAdvisorPaidAsync(Payout payout)
        {
            await _notificationService.SendNotificationAsync(
                payout.Wallet.Advisor.UserId,
                "Khoản thanh toán đã được xử lý",
                $"Khoản thanh toán với số tiền {payout.Amount:0.##} đã được chuyển thành công.",
                NotificationType.System,
                payout.PayoutId,
                "Payout");
        }

        private async Task NotifyAdvisorRejectedAsync(Payout payout)
        {
            var reason = string.IsNullOrWhiteSpace(payout.RejectReason)
                ? "Vui lòng kiểm tra lại thông tin thanh toán."
                : payout.RejectReason;

            await _notificationService.SendNotificationAsync(
                payout.Wallet.Advisor.UserId,
                "Khoản thanh toán bị từ chối",
                $"Khoản thanh toán đã bị từ chối. Lý do: {reason}",
                NotificationType.System,
                payout.PayoutId,
                "Payout");
        }

        private async Task NotifyAdvisorRetrySubmittedAsync(Payout payout)
        {
            await _notificationService.SendNotificationAsync(
                payout.Wallet.Advisor.UserId,
                "Yêu cầu gửi lại thanh toán đã được gửi",
                "Khoản thanh toán đã được chuyển sang để kiểm tra lại và đang chờ nhân viên xem xét.",
                NotificationType.System,
                payout.PayoutId,
                "Payout");
        }

        private async Task NotifyStaffRetryRequestedAsync(Payout payout)
        {
            var staffIds = await _unitOfWork.Users.GetAllQuery()
                .Where(u => u.Role == UserRole.Staff || u.Role == UserRole.Admin)
                .Select(u => u.Id)
                .ToListAsync();

            if (staffIds.Count == 0)
            {
                return;
            }

            var advisorName = payout.Wallet.Advisor.User?.UserName ?? "Cố vấn";
            var title = "Có yêu cầu gửi lại thanh toán";
            var message = $"{advisorName} đã gửi yêu cầu gửi lại thanh toán và đang chờ nhân viên xem xét.";

            foreach (var staffId in staffIds)
            {
                await _notificationService.SendNotificationAsync(
                    staffId,
                    title,
                    message,
                    NotificationType.System,
                    payout.PayoutId,
                    "Payout");
            }
        }
    }
}









