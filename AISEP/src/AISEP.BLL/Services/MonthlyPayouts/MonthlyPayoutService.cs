using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;
using AISEP.BLL.Helpers;
using AISEP.DAL.Common;
using AISEP.DAL.Entities;
using AISEP.DAL.Enums;
using AutoMapper;
using Sieve.Models;
using Sieve.Services;

namespace AISEP.BLL.Services.MonthlyPayouts
{
    public class MonthlyPayoutService : IMonthlyPayoutService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISieveProcessor _sieveProcessor;
        private readonly IMapper _mapper;
        private readonly IMonthlyPayoutBatchService _monthlyPayoutBatchService;

        public MonthlyPayoutService(
            IUnitOfWork unitOfWork,
            ISieveProcessor sieveProcessor,
            IMapper mapper,
            IMonthlyPayoutBatchService monthlyPayoutBatchService)
        {
            _unitOfWork = unitOfWork;
            _sieveProcessor = sieveProcessor;
            _mapper = mapper;
            _monthlyPayoutBatchService = monthlyPayoutBatchService;
        }

        public async Task<MonthlyPayoutResponse> MarkPaidAsync(int monthlyPayoutId, int staffUserId, MarkMonthlyPayoutPaidRequest request)
        {
            var payout = await _unitOfWork.MonthlyPayouts.GetByIdAsync(monthlyPayoutId)
                ?? throw new KeyNotFoundException("Monthly payout not found.");

            if (payout.Status == MonthlyPayoutStatus.Paid)
            {
                return _mapper.Map<MonthlyPayoutResponse>(payout);
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
                MonthlyPayoutId = payout.MonthlyPayoutId
            });

            var now = DateTime.UtcNow;
            payout.Status = MonthlyPayoutStatus.Paid;
            payout.ApprovedAt = now;
            payout.ApprovedById = staffUserId;
            payout.PaidAt = now;
            payout.PaidById = staffUserId;
            payout.RejectedAt = null;
            payout.RejectedById = null;
            payout.RejectReason = null;
            payout.Note = string.IsNullOrWhiteSpace(request.Note) ? payout.Note : request.Note.Trim();

            _unitOfWork.MonthlyPayouts.Update(payout);
            await _unitOfWork.SaveChangesAsync();

            if (payout.MonthlyPayoutBatchId.HasValue)
            {
                await _monthlyPayoutBatchService.RecalculateAsync(payout.MonthlyPayoutBatchId.Value);
            }

            return _mapper.Map<MonthlyPayoutResponse>(payout);
        }

        public async Task<MonthlyPayoutResponse> RejectAsync(int monthlyPayoutId, int staffUserId, RejectMonthlyPayoutRequest request)
        {
            var payout = await _unitOfWork.MonthlyPayouts.GetByIdAsync(monthlyPayoutId)
                ?? throw new KeyNotFoundException("Monthly payout not found.");

            if (payout.Status == MonthlyPayoutStatus.Paid)
            {
                throw new InvalidOperationException("Paid payout cannot be rejected.");
            }

            var reason = string.IsNullOrWhiteSpace(request.Reason) ? null : request.Reason.Trim();
            if (string.IsNullOrWhiteSpace(reason))
            {
                throw new InvalidOperationException("Reject reason is required.");
            }

            payout.Status = MonthlyPayoutStatus.Rejected;
            payout.ApprovedAt = null;
            payout.ApprovedById = null;
            payout.RejectedAt = DateTime.UtcNow;
            payout.RejectedById = staffUserId;
            payout.RejectReason = reason;
            payout.Note = string.IsNullOrWhiteSpace(request.Note) ? payout.Note : request.Note.Trim();

            _unitOfWork.MonthlyPayouts.Update(payout);
            await _unitOfWork.SaveChangesAsync();

            if (payout.MonthlyPayoutBatchId.HasValue)
            {
                await _monthlyPayoutBatchService.RecalculateAsync(payout.MonthlyPayoutBatchId.Value);
            }

            return _mapper.Map<MonthlyPayoutResponse>(payout);
        }

        public async Task<PagedResult<MonthlyPayoutResponse>> GetAllAsync(SieveModel model)
        {
            var query = _unitOfWork.MonthlyPayouts.GetQuery();
            return await PaginationHelper.PaginateAsync(
                query,
                model,
                _sieveProcessor,
                x => _mapper.Map<MonthlyPayoutResponse>(x));
        }

        public async Task<PagedResult<MonthlyPayoutResponse>> GetMineAsync(int advisorUserId, SieveModel model)
        {
            var advisor = await _unitOfWork.Advisors.GetByUserIdAsync(advisorUserId)
                ?? throw new KeyNotFoundException("Advisor profile not found.");

            var query = _unitOfWork.MonthlyPayouts.GetQuery()
                .Where(x => x.AdvisorId == advisor.AdvisorId);

            return await PaginationHelper.PaginateAsync(
                query,
                model,
                _sieveProcessor,
                x => _mapper.Map<MonthlyPayoutResponse>(x));
        }
    }
}
