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
    public class PayoutGroupService : IPayoutGroupService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISieveProcessor _sieveProcessor;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;

        public PayoutGroupService(
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

        public async Task<List<PayoutResponse>> GenerateAsync(GeneratePayoutGroupRequest request)
        {
            var (periodStartUtc, periodEndUtc, fromDateLocal, toDateLocal) = NormalizeDateRangeOrThrow(request.FromDate, request.ToDate);

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

            var batch = await CreateBatchAsync(fromDateLocal, toDateLocal);
            var groupedByWallet = eligibleTransactions.GroupBy(x => x.WalletId);
            var generated = new List<Payout>();
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

                var payout = new Payout
                {
                    PayoutGroupId = batch.PayoutGroupId,
                    WalletId = walletGroup.Key,
                    PeriodFromDate = fromDateLocal,
                    PeriodToDate = toDateLocal,
                    Amount = totalAmount,
                    Status = MonthlyPayoutStatus.Pending,
                    BankName = activeBankAccount.BankName,
                    AccountNumber = activeBankAccount.AccountNumber,
                    AccountHolderName = activeBankAccount.AccountHolderName
                };
                await _unitOfWork.Payouts.AddAsync(payout);
                generated.Add(payout);

                foreach (var transaction in walletGroup)
                {
                    transaction.Payout = payout;
                }
            }

            await _unitOfWork.SaveChangesAsync();
            await RecalculateAsync(batch.PayoutGroupId);

            await NotifyMissingBankAccountsAsync(missingBankAdvisors);

            var ids = generated.Select(x => x.PayoutId).ToList();
            var created = await _unitOfWork.Payouts.GetQuery()
                .Where(x => ids.Contains(x.PayoutId))
                .ToListAsync();

            return created.Select(x => _mapper.Map<PayoutResponse>(x)).ToList();
        }

        public async Task<PagedResult<PayoutGroupResponse>> GetGroupsAsync(SieveModel model)
        {
            var query = _unitOfWork.PayoutGroups.GetQuery();
            return await PaginationHelper.PaginateAsync(
                query,
                model,
                _sieveProcessor,
                x => _mapper.Map<PayoutGroupResponse>(x));
        }

        public async Task<PayoutGroupResponse?> GetGroupByIdAsync(int groupId)
        {
            var group = await _unitOfWork.PayoutGroups.GetByIdAsync(groupId);
            return group is null ? null : _mapper.Map<PayoutGroupResponse>(group);
        }

        public async Task<PagedResult<PayoutResponse>> GetItemsByGroupIdAsync(int groupId, SieveModel model)
        {
            var group = await _unitOfWork.PayoutGroups.GetByIdAsync(groupId);
            if (group is null)
            {
                throw new KeyNotFoundException("Monthly payout group not found.");
            }

            var query = _unitOfWork.Payouts.GetQuery()
                .Where(x => x.PayoutGroupId == groupId);

            return await PaginationHelper.PaginateAsync(
                query,
                model,
                _sieveProcessor,
                x => _mapper.Map<PayoutResponse>(x));
        }

        public async Task RecalculateAsync(int groupId)
        {
            var group = await _unitOfWork.PayoutGroups.GetByIdAsync(groupId);
            if (group is null)
            {
                return;
            }

            group.EstimatedTotalAmount = Math.Round(group.Payouts.Sum(x => x.Amount), 2, MidpointRounding.AwayFromZero);
            group.RejectedAmount = Math.Round(
                group.Payouts
                    .Where(x => x.Status == MonthlyPayoutStatus.Rejected)
                    .Sum(x => x.Amount),
                2,
                MidpointRounding.AwayFromZero);
            group.ActualPayableAmount = Math.Round(group.EstimatedTotalAmount - group.RejectedAmount, 2, MidpointRounding.AwayFromZero);

            var hasPending = group.Payouts.Any(x =>
                x.Status == MonthlyPayoutStatus.Pending || x.Status == MonthlyPayoutStatus.PendingRecheck);
            if (hasPending)
            {
                group.Status = MonthlyPayoutBatchStatus.InProgress;
                group.CompletedAt = null;
            }
            else
            {
                group.Status = MonthlyPayoutBatchStatus.Completed;
                group.CompletedAt ??= DateTime.UtcNow;
            }

            _unitOfWork.PayoutGroups.Update(group);
            await _unitOfWork.SaveChangesAsync();
        }

        private async Task NotifyMissingBankAccountsAsync(List<(int UserId, string Name)> missingBankAdvisors)
        {
            if (missingBankAdvisors.Count == 0)
            {
                return;
            }

            var advisorNoticeTitle = "Thiếu thông tin ngân hàng nhận thanh toán";
            var advisorNoticeMessage = "Hệ thống không thể tạo khoản thanh toán vì bạn chưa cập nhật thông tin ngân hàng. Vui lòng cập nhật để nhận thanh toán.";

            foreach (var advisor in missingBankAdvisors)
            {
                await _notificationService.SendNotificationAsync(
                    advisor.UserId,
                    advisorNoticeTitle,
                    advisorNoticeMessage,
                    NotificationType.System);
            }

            var names = string.Join(", ", missingBankAdvisors.Select(x => x.Name).Distinct());
            var reviewerNoticeTitle = "Có cố vấn chưa có thông tin ngân hàng nhận thanh toán";
            var reviewerNoticeMessage = $"Không thể tạo khoản thanh toán cho các cố vấn sau do chưa cập nhật thông tin ngân hàng: {names}.";

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

        private async Task<PayoutGroup> CreateBatchAsync(DateTime fromDateLocal, DateTime toDateLocal)
        {
            var batch = new PayoutGroup
            {
                FromDate = fromDateLocal,
                ToDate = toDateLocal,
                EstimatedTotalAmount = 0m,
                RejectedAmount = 0m,
                ActualPayableAmount = 0m,
                Status = MonthlyPayoutBatchStatus.InProgress,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.PayoutGroups.AddAsync(batch);
            await _unitOfWork.SaveChangesAsync();
            return batch;
        }


        private static (DateTime PeriodStartUtc, DateTime PeriodEndUtc, DateTime FromDateLocal, DateTime ToDateLocal) NormalizeDateRangeOrThrow(
            DateTime fromDate,
            DateTime toDate)
        {
            var from = fromDate.Date;
            var to = toDate.Date;

            if (from > to)
            {
                throw new InvalidOperationException("FromDate must be less than or equal to ToDate.");
            }

            if ((to - from).TotalDays > 62)
            {
                throw new InvalidOperationException("Date range is too large. Please select a range of 62 days or less.");
            }

            var startLocal = new DateTime(from.Year, from.Month, from.Day, 0, 0, 0, DateTimeKind.Unspecified);
            var endLocalExclusive = new DateTime(to.Year, to.Month, to.Day, 0, 0, 0, DateTimeKind.Unspecified).AddDays(1);

            var vietnamTz = GetVietnamTimeZone();
            var startUtc = TimeZoneInfo.ConvertTimeToUtc(startLocal, vietnamTz);
            var endUtc = TimeZoneInfo.ConvertTimeToUtc(endLocalExclusive, vietnamTz);
            return (startUtc, endUtc, from, to);
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

    }
}







