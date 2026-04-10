using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;
using AISEP.BLL.Helpers;
using AISEP.DAL.Common;
using AISEP.DAL.Entities;
using AISEP.DAL.Enums;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using Sieve.Services;

namespace AISEP.BLL.Services.Wallets
{
    public class MonthlyPayoutService : IMonthlyPayoutService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISieveProcessor _sieveProcessor;
        private readonly IMapper _mapper;

        public MonthlyPayoutService(IUnitOfWork unitOfWork, ISieveProcessor sieveProcessor, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _sieveProcessor = sieveProcessor;
            _mapper = mapper;
        }

        public async Task<List<MonthlyPayoutResponse>> GenerateAsync(GenerateMonthlyPayoutRequest request)
        {
            ValidatePeriod(request.Year, request.Month);

            var periodStartUtc = new DateTime(request.Year, request.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var periodEndUtc = periodStartUtc.AddMonths(1);

            var depositQuery = _unitOfWork.WalletTransactions
                .GetCompletedDepositsWithoutPayoutQuery(periodStartUtc, periodEndUtc);

            if (request.AdvisorId.HasValue)
            {
                depositQuery = depositQuery.Where(x => x.Wallet.AdvisorId == request.AdvisorId.Value);
            }

            var eligibleTransactions = await depositQuery.ToListAsync();
            if (eligibleTransactions.Count == 0)
            {
                var hasExistingPayout = await _unitOfWork.MonthlyPayouts.ExistsByPeriodAsync(request.Year, request.Month);
                if (hasExistingPayout)
                {
                    throw new InvalidOperationException(
                        $"Payout tháng {request.Month:D2}/{request.Year} đã được đồng bộ trước đó. Không có giao dịch mới để tạo thêm.");
                }

                return [];
            }

            var groupedByWallet = eligibleTransactions.GroupBy(x => x.WalletId);
            var generated = new List<MonthlyPayout>();

            foreach (var walletGroup in groupedByWallet)
            {
                var first = walletGroup.First();
                var advisorId = first.Wallet.AdvisorId;
                var totalAmount = Math.Round(walletGroup.Sum(x => x.Amount), 2, MidpointRounding.AwayFromZero);
                if (totalAmount <= 0)
                {
                    continue;
                }

                var existing = await _unitOfWork.MonthlyPayouts.GetByAdvisorAndPeriodAsync(advisorId, request.Year, request.Month);
                if (existing is null)
                {
                    existing = new MonthlyPayout
                    {
                        WalletId = walletGroup.Key,
                        AdvisorId = advisorId,
                        Year = request.Year,
                        Month = request.Month,
                        Amount = totalAmount,
                        Status = MonthlyPayoutStatus.Pending
                    };
                    await _unitOfWork.MonthlyPayouts.AddAsync(existing);
                    generated.Add(existing);
                }
                else
                {
                    if (existing.Status == MonthlyPayoutStatus.Paid)
                    {
                        throw new InvalidOperationException(
                            $"Monthly payout {request.Month}/{request.Year} for advisor #{advisorId} is already paid.");
                    }

                    existing.Amount = Math.Round(existing.Amount + totalAmount, 2, MidpointRounding.AwayFromZero);
                    _unitOfWork.MonthlyPayouts.Update(existing);
                    if (generated.All(x => x.MonthlyPayoutId != existing.MonthlyPayoutId))
                    {
                        generated.Add(existing);
                    }
                }

                foreach (var transaction in walletGroup)
                {
                    transaction.MonthlyPayout = existing;
                }
            }

            await _unitOfWork.SaveChangesAsync();

            var ids = generated.Select(x => x.MonthlyPayoutId).ToList();
            var created = await _unitOfWork.MonthlyPayouts.GetQuery()
                .Where(x => ids.Contains(x.MonthlyPayoutId))
                .ToListAsync();

            return created.Select(x => _mapper.Map<MonthlyPayoutResponse>(x)).ToList();
        }

        public async Task<MonthlyPayoutResponse> MarkPaidAsync(int monthlyPayoutId, int staffUserId, MarkMonthlyPayoutPaidRequest request)
        {
            var payout = await _unitOfWork.MonthlyPayouts.GetByIdAsync(monthlyPayoutId)
                ?? throw new KeyNotFoundException("Monthly payout not found.");

            if (payout.Status == MonthlyPayoutStatus.Paid)
            {
                return _mapper.Map<MonthlyPayoutResponse>(payout);
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

            payout.Status = MonthlyPayoutStatus.Paid;
            payout.PaidAt = DateTime.UtcNow;
            payout.PaidById = staffUserId;
            payout.Note = string.IsNullOrWhiteSpace(request.Note) ? payout.Note : request.Note.Trim();

            _unitOfWork.MonthlyPayouts.Update(payout);
            await _unitOfWork.SaveChangesAsync();

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

        private static void ValidatePeriod(int year, int month)
        {
            if (year < 2000 || year > 2100)
            {
                throw new InvalidOperationException("Year must be in range 2000-2100.");
            }

            if (month < 1 || month > 12)
            {
                throw new InvalidOperationException("Month must be in range 1-12.");
            }
        }
    }
}
