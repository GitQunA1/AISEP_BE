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

namespace AISEP.BLL.Services.MonthlyPayouts
{
    public class MonthlyPayoutBatchService : IMonthlyPayoutBatchService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISieveProcessor _sieveProcessor;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;

        public MonthlyPayoutBatchService(
            IUnitOfWork unitOfWork,
            ISieveProcessor sieveProcessor,
            IMapper mapper,
            INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _sieveProcessor = sieveProcessor;
            _mapper = mapper;
            _notificationService = notificationService;
        }

        public async Task<List<MonthlyPayoutResponse>> GenerateAsync(GenerateMonthlyPayoutRequest request)
        {
            ValidatePeriod(request.Year, request.Month);

            var nowLocal = GetVietnamNow();
            var (latestClosedYear, latestClosedMonth) = GetLatestClosedCycle(nowLocal);
            if (request.Year != latestClosedYear || request.Month != latestClosedMonth)
            {
                throw new InvalidOperationException(
                    $"Monthly payout can only be generated for the latest closed cycle {latestClosedMonth:D2}/{latestClosedYear}.");
            }

            var (periodStartUtc, periodEndUtc) = GetCycleWindowUtc(request.Year, request.Month);

            var existingBatch = await _unitOfWork.MonthlyPayoutBatches.GetByPeriodAsync(request.Year, request.Month);
            if (existingBatch is not null)
            {
                throw new InvalidOperationException(
                    $"Monthly payout batch for {request.Month:D2}/{request.Year} already exists. Generation is allowed only once per month.");
            }

            var depositQuery = _unitOfWork.WalletTransactions
                .GetCompletedDepositsWithoutPayoutQuery(periodStartUtc, periodEndUtc);

            if (request.AdvisorId.HasValue)
            {
                depositQuery = depositQuery.Where(x => x.Wallet.AdvisorId == request.AdvisorId.Value);
            }

            var eligibleTransactions = await depositQuery.ToListAsync();
            if (eligibleTransactions.Count == 0)
            {
                return [];
            }

            var batch = await GetOrCreateBatchAsync(request.Year, request.Month);
            var groupedByWallet = eligibleTransactions.GroupBy(x => x.WalletId);
            var generated = new List<MonthlyPayout>();
            var missingBankAdvisors = new List<(int UserId, string Name)>();
            var missingBankAdvisorIds = new HashSet<int>();

            foreach (var walletGroup in groupedByWallet)
            {
                var first = walletGroup.First();
                var advisorId = first.Wallet.AdvisorId;
                var totalAmount = Math.Round(walletGroup.Sum(x => x.Amount), 2, MidpointRounding.AwayFromZero);
                if (totalAmount <= 0)
                {
                    continue;
                }

                var activeBankAccount = await _unitOfWork.AdvisorBankAccounts.GetActiveByAdvisorIdAsync(advisorId);
                if (activeBankAccount is null)
                {
                    if (missingBankAdvisorIds.Add(advisorId))
                    {
                        var advisorProfile = await _unitOfWork.Advisors.GetByIdAsync(advisorId);
                        if (advisorProfile is not null)
                        {
                            var advisorName = advisorProfile.User?.UserName ?? "Advisor";
                            missingBankAdvisors.Add((advisorProfile.UserId, advisorName));
                        }
                    }

                    if (request.AdvisorId.HasValue)
                    {
                        await NotifyMissingBankAccountsAsync(missingBankAdvisors);
                        throw new InvalidOperationException("Advisor has no active bank account. Please update bank information first.");
                    }
                    continue;
                }

                var existing = await _unitOfWork.MonthlyPayouts.GetByAdvisorAndPeriodAsync(advisorId, request.Year, request.Month);
                if (existing is null)
                {
                    existing = new MonthlyPayout
                    {
                        MonthlyPayoutBatchId = batch.MonthlyPayoutBatchId,
                        WalletId = walletGroup.Key,
                        AdvisorId = advisorId,
                        Year = request.Year,
                        Month = request.Month,
                        Amount = totalAmount,
                        Status = MonthlyPayoutStatus.Pending,
                        BankName = activeBankAccount.BankName,
                        AccountNumber = activeBankAccount.AccountNumber,
                        AccountHolderName = activeBankAccount.AccountHolderName
                    };
                    await _unitOfWork.MonthlyPayouts.AddAsync(existing);
                    generated.Add(existing);
                }
                else
                {
                    if (existing.Status != MonthlyPayoutStatus.Pending)
                    {
                        throw new InvalidOperationException(
                            "Monthly payout is already finalized and cannot be regenerated.");
                    }

                    existing.Amount = Math.Round(existing.Amount + totalAmount, 2, MidpointRounding.AwayFromZero);
                    existing.MonthlyPayoutBatchId = batch.MonthlyPayoutBatchId;
                    existing.BankName = activeBankAccount.BankName;
                    existing.AccountNumber = activeBankAccount.AccountNumber;
                    existing.AccountHolderName = activeBankAccount.AccountHolderName;
                    _unitOfWork.MonthlyPayouts.Update(existing);
                    if (existing.MonthlyPayoutId == 0 || generated.All(x => x.MonthlyPayoutId != existing.MonthlyPayoutId))
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
            await RecalculateAsync(batch.MonthlyPayoutBatchId);

            await NotifyMissingBankAccountsAsync(missingBankAdvisors);

            var ids = generated.Select(x => x.MonthlyPayoutId).ToList();
            var created = await _unitOfWork.MonthlyPayouts.GetQuery()
                .Where(x => ids.Contains(x.MonthlyPayoutId))
                .ToListAsync();

            return created.Select(x => _mapper.Map<MonthlyPayoutResponse>(x)).ToList();
        }

        public async Task<PagedResult<MonthlyPayoutBatchResponse>> GetBatchesAsync(SieveModel model)
        {
            var query = _unitOfWork.MonthlyPayoutBatches.GetQuery();
            return await PaginationHelper.PaginateAsync(
                query,
                model,
                _sieveProcessor,
                x => _mapper.Map<MonthlyPayoutBatchResponse>(x));
        }

        public async Task<MonthlyPayoutBatchResponse?> GetBatchByIdAsync(int batchId)
        {
            var batch = await _unitOfWork.MonthlyPayoutBatches.GetByIdAsync(batchId);
            return batch is null ? null : _mapper.Map<MonthlyPayoutBatchResponse>(batch);
        }

        public async Task<PagedResult<MonthlyPayoutResponse>> GetItemsByBatchIdAsync(int batchId, SieveModel model)
        {
            var batch = await _unitOfWork.MonthlyPayoutBatches.GetByIdAsync(batchId);
            if (batch is null)
            {
                throw new KeyNotFoundException("Monthly payout batch not found.");
            }

            var query = _unitOfWork.MonthlyPayouts.GetQuery()
                .Where(x => x.MonthlyPayoutBatchId == batchId);

            return await PaginationHelper.PaginateAsync(
                query,
                model,
                _sieveProcessor,
                x => _mapper.Map<MonthlyPayoutResponse>(x));
        }

        public async Task RecalculateAsync(int batchId)
        {
            var batch = await _unitOfWork.MonthlyPayoutBatches.GetByIdAsync(batchId);
            if (batch is null)
            {
                return;
            }

            batch.EstimatedTotalAmount = Math.Round(batch.MonthlyPayouts.Sum(x => x.Amount), 2, MidpointRounding.AwayFromZero);
            batch.RejectedAmount = Math.Round(
                batch.MonthlyPayouts
                    .Where(x => x.Status == MonthlyPayoutStatus.Rejected)
                    .Sum(x => x.Amount),
                2,
                MidpointRounding.AwayFromZero);
            batch.ActualPayableAmount = Math.Round(batch.EstimatedTotalAmount - batch.RejectedAmount, 2, MidpointRounding.AwayFromZero);

            var hasPending = batch.MonthlyPayouts.Any(x => x.Status == MonthlyPayoutStatus.Pending);
            if (hasPending)
            {
                batch.Status = MonthlyPayoutBatchStatus.InProgress;
                batch.CompletedAt = null;
            }
            else
            {
                batch.Status = MonthlyPayoutBatchStatus.Completed;
                batch.CompletedAt ??= DateTime.UtcNow;
            }

            _unitOfWork.MonthlyPayoutBatches.Update(batch);
            await _unitOfWork.SaveChangesAsync();
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

        private async Task NotifyMissingBankAccountsAsync(List<(int UserId, string Name)> missingBankAdvisors)
        {
            if (missingBankAdvisors.Count == 0)
            {
                return;
            }

            var advisorNoticeTitle = "Thieu thong tin ngan hang nhan payout";
            var advisorNoticeMessage = "He thong khong the tao payout vi ban chua cap nhat thong tin ngan hang. Vui long cap nhat de nhan thanh toan.";

            foreach (var advisor in missingBankAdvisors)
            {
                await _notificationService.SendNotificationAsync(
                    advisor.UserId,
                    advisorNoticeTitle,
                    advisorNoticeMessage,
                    NotificationType.System);
            }

            var names = string.Join(", ", missingBankAdvisors.Select(x => x.Name).Distinct());
            var reviewerNoticeTitle = "Co advisor chua co thong tin ngan hang payout";
            var reviewerNoticeMessage = $"Khong the tao payout cho cac advisor sau do chua cap nhat thong tin ngan hang: {names}.";

            var reviewerIds = await _unitOfWork.Users.GetAllQuery()
                .Where(u => u.Role == UserRole.Staff || u.Role == UserRole.Admin)
                .Select(u => u.Id)
                .ToListAsync();

            foreach (var reviewerId in reviewerIds)
            {
                await _notificationService.SendNotificationAsync(
                    reviewerId,
                    reviewerNoticeTitle,
                    reviewerNoticeMessage,
                    NotificationType.System);
            }
        }

        private async Task<MonthlyPayoutBatch> GetOrCreateBatchAsync(int year, int month)
        {
            var batch = await _unitOfWork.MonthlyPayoutBatches.GetByPeriodAsync(year, month);
            if (batch is not null)
            {
                return batch;
            }

            batch = new MonthlyPayoutBatch
            {
                Year = year,
                Month = month,
                EstimatedTotalAmount = 0m,
                RejectedAmount = 0m,
                ActualPayableAmount = 0m,
                Status = MonthlyPayoutBatchStatus.InProgress,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.MonthlyPayoutBatches.AddAsync(batch);
            await _unitOfWork.SaveChangesAsync();
            return batch;
        }

        private static (int Year, int Month) GetLatestClosedCycle(DateTime nowLocal)
        {
            var previousMonth = new DateTime(nowLocal.Year, nowLocal.Month, 1).AddMonths(-1);
            return (previousMonth.Year, previousMonth.Month);
        }

        private static (DateTime PeriodStartUtc, DateTime PeriodEndUtc) GetCycleWindowUtc(int cycleYear, int cycleMonth)
        {
            var startLocal = new DateTime(cycleYear, cycleMonth, 1, 0, 0, 0, DateTimeKind.Unspecified);
            var endLocal = startLocal.AddMonths(1);

            var vietnamTz = GetVietnamTimeZone();
            var startUtc = TimeZoneInfo.ConvertTimeToUtc(startLocal, vietnamTz);
            var endUtc = TimeZoneInfo.ConvertTimeToUtc(endLocal, vietnamTz);
            return (startUtc, endUtc);
        }

        private static TimeZoneInfo GetVietnamTimeZone()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            }
            catch
            {
                return TimeZoneInfo.CreateCustomTimeZone("UTC+7", TimeSpan.FromHours(7), "UTC+7", "UTC+7");
            }
        }

        private static DateTime GetVietnamNow()
        {
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, GetVietnamTimeZone());
        }
    }
}
