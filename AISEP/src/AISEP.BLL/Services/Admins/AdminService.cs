using AISEP.BLL.DTOs.Responses;
using AISEP.DAL.Common;
using AISEP.DAL.Enums;
using Microsoft.EntityFrameworkCore;

namespace AISEP.BLL.Services.Admins
{
    public class AdminService : IAdminService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AdminService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PlatformOverviewResponse> GetPlatformOverviewAsync(DateTime? from, DateTime? to)
        {
            var startupQuery = _unitOfWork.Startups.GetStartupQuery()
                .Where(s => s.ApprovalStatus == ApprovalStatus.Approved
                            && s.User.Status == UserStatus.Active);

            var investorQuery = _unitOfWork.Investors.GetAllQuery()
                .Where(i => i.ApprovalStatus == ApprovalStatus.Approved
                            && i.User.Status == UserStatus.Active);

            var projectQuery = _unitOfWork.Projects.GetAllQuery();

            var bookingQuery = _unitOfWork.Bookings.GetBookingQuery()
                .Where(b => b.Status == BookingStatus.Completed);

            if (from.HasValue)
            {
                startupQuery = startupQuery.Where(s => s.CreatedAt >= from.Value);
                investorQuery = investorQuery.Where(i => i.User.CreatedAt >= from.Value);
                projectQuery = projectQuery.Where(p => p.CreatedAt >= from.Value);
                bookingQuery = bookingQuery.Where(b => b.CreatedAt >= from.Value);
            }

            if (to.HasValue)
            {
                startupQuery = startupQuery.Where(s => s.CreatedAt <= to.Value);
                investorQuery = investorQuery.Where(i => i.User.CreatedAt <= to.Value);
                projectQuery = projectQuery.Where(p => p.CreatedAt <= to.Value);
                bookingQuery = bookingQuery.Where(b => b.CreatedAt <= to.Value);
            }

            var activeStartupCount = await startupQuery.CountAsync();
            var activeInvestorCount = await investorQuery.CountAsync();
            var projectCount = await projectQuery.CountAsync();
            var completedBookingCount = await bookingQuery.CountAsync();

            return new PlatformOverviewResponse
            {
                ActiveStartupCount = activeStartupCount,
                ActiveInvestorCount = activeInvestorCount,
                ProjectCount = projectCount,
                CompletedBookingCount = completedBookingCount
            };
        }

        public async Task<ProjectStatusBreakdownResponse> GetProjectStatusBreakdownAsync()
        {
            var grouped = await _unitOfWork.Projects.GetAllQuery()
                .GroupBy(p => p.Status)
                .Select(g => new
                {
                    Status = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            var lookup = grouped.ToDictionary(x => x.Status, x => x.Count);

            return new ProjectStatusBreakdownResponse
            {
                DraftCount = lookup.GetValueOrDefault(ProjectStatus.Draft),
                PendingCount = lookup.GetValueOrDefault(ProjectStatus.Pending),
                PublishedCount = lookup.GetValueOrDefault(ProjectStatus.Approved),
                RejectedCount = lookup.GetValueOrDefault(ProjectStatus.Rejected)
            };
        }

        public async Task<InvestmentTrendsResponse> GetInvestmentTrendsAsync(DateTime? from, DateTime? to)
        {
            var fromDate = from?.Date;
            var toExclusive = to?.Date.AddDays(1);

            if (fromDate.HasValue && toExclusive.HasValue && fromDate.Value >= toExclusive.Value)
            {
                throw new InvalidOperationException("From date must be earlier than or equal to to date.");
            }

            var dealQuery = _unitOfWork.Deals.GetQuery()
                .Where(d => d.Status == DealStatus.Completed && d.InvestedAmount > 0m);

            if (fromDate.HasValue)
            {
                dealQuery = dealQuery.Where(d => d.DealDate >= fromDate.Value);
            }

            if (toExclusive.HasValue)
            {
                dealQuery = dealQuery.Where(d => d.DealDate < toExclusive.Value);
            }

            var monthlyAmountGroups = await dealQuery
                .GroupBy(d => new { d.DealDate.Year, d.DealDate.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    Amount = g.Sum(x => x.InvestedAmount)
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToListAsync();

            var monthlyAmountLookup = monthlyAmountGroups.ToDictionary(x => (x.Year, x.Month), x => x.Amount);
            var monthlyAmounts = BuildMonthlyInvestmentAmounts(monthlyAmountLookup, fromDate, to);

            var typeGroups = await dealQuery
                .GroupBy(d => d.Type)
                .Select(g => new
                {
                    Type = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            var totalDealCount = typeGroups.Sum(x => x.Count);
            var equityDealCount = typeGroups
                .Where(x => x.Type == InvestmentType.Equity)
                .Sum(x => x.Count);
            var customTermsDealCount = typeGroups
                .Where(x => x.Type == InvestmentType.CustomTerms)
                .Sum(x => x.Count);

            var typeBreakdown = new InvestmentTypeBreakdownResponse
            {
                EquityPercent = totalDealCount > 0 ? Math.Round(equityDealCount * 100m / totalDealCount, 2) : 0m,
                CustomTermsPercent = totalDealCount > 0 ? Math.Round(customTermsDealCount * 100m / totalDealCount, 2) : 0m
            };

            var topProjects = await dealQuery
                .GroupBy(d => new { d.ProjectId, d.Project.ProjectName })
                .Select(g => new InvestmentTopProjectResponse
                {
                    ProjectId = g.Key.ProjectId,
                    ProjectName = g.Key.ProjectName,
                    TotalInvestedAmount = g.Sum(x => x.InvestedAmount)
                })
                .OrderByDescending(x => x.TotalInvestedAmount)
                .Take(5)
                .ToListAsync();

            return new InvestmentTrendsResponse
            {
                MonthlyAmounts = monthlyAmounts,
                TypeBreakdown = typeBreakdown,
                TopProjects = topProjects
            };
        }

        private static List<MonthlyInvestmentAmountResponse> BuildMonthlyInvestmentAmounts(
            IReadOnlyDictionary<(int Year, int Month), decimal> monthlyAmountLookup,
            DateTime? fromDate,
            DateTime? to)
        {
            if (monthlyAmountLookup.Count == 0)
            {
                if (!fromDate.HasValue || !to.HasValue)
                {
                    return [];
                }

                return BuildEmptyMonthlyInvestmentAmounts(
                    new DateTime(fromDate.Value.Year, fromDate.Value.Month, 1),
                    new DateTime(to.Value.Year, to.Value.Month, 1));
            }

            var groupPeriods = monthlyAmountLookup.Keys
                .Select(k => new DateTime(k.Year, k.Month, 1))
                .OrderBy(d => d)
                .ToList();

            var startPeriod = fromDate.HasValue
                ? new DateTime(fromDate.Value.Year, fromDate.Value.Month, 1)
                : groupPeriods.First();

            var endPeriod = to.HasValue
                ? new DateTime(to.Value.Year, to.Value.Month, 1)
                : groupPeriods.Last();

            var monthlyAmounts = new List<MonthlyInvestmentAmountResponse>();
            for (var period = startPeriod; period <= endPeriod; period = period.AddMonths(1))
            {
                monthlyAmounts.Add(new MonthlyInvestmentAmountResponse
                {
                    Year = period.Year,
                    Month = period.Month,
                    Amount = monthlyAmountLookup.GetValueOrDefault((period.Year, period.Month))
                });
            }

            return monthlyAmounts;
        }

        private static List<MonthlyInvestmentAmountResponse> BuildEmptyMonthlyInvestmentAmounts(
            DateTime startPeriod,
            DateTime endPeriod)
        {
            var monthlyAmounts = new List<MonthlyInvestmentAmountResponse>();
            for (var period = startPeriod; period <= endPeriod; period = period.AddMonths(1))
            {
                monthlyAmounts.Add(new MonthlyInvestmentAmountResponse
                {
                    Year = period.Year,
                    Month = period.Month,
                    Amount = 0m
                });
            }

            return monthlyAmounts;
        }

        public async Task<PlatformRevenueStatisticsResponse> GetPlatformRevenueStatisticsAsync(
            int? month,
            int? year,
            DateTime? from = null,
            DateTime? to = null)
        {
            var now = DateTime.UtcNow;
            var targetYear = year ?? now.Year;
            var targetMonth = month ?? now.Month;
            var fromDate = from?.Date;
            var toExclusive = to?.Date.AddDays(1);

            if (targetMonth < 1 || targetMonth > 12)
            {
                throw new InvalidOperationException("Month must be between 1 and 12.");
            }

            if (fromDate.HasValue && toExclusive.HasValue && fromDate.Value >= toExclusive.Value)
            {
                throw new InvalidOperationException("From date must be earlier than or equal to to date.");
            }

            var subscriptionQuery = _unitOfWork.Subscriptions.GetQuery()
                .Include(s => s.Package)
                .Where(s => s.Package.Price > 0m);

            var monthRevenue = await subscriptionQuery
                .Where(s => s.StartDate.Year == targetYear && s.StartDate.Month == targetMonth)
                .SumAsync(s => s.Package.Price);

            var yearQuery = subscriptionQuery
                .Where(s => s.StartDate.Year == targetYear);

            var yearRevenue = await yearQuery.SumAsync(s => s.Package.Price);

            var periodQuery = subscriptionQuery;
            if (fromDate.HasValue)
            {
                periodQuery = periodQuery.Where(s => s.StartDate >= fromDate.Value);
            }

            if (toExclusive.HasValue)
            {
                periodQuery = periodQuery.Where(s => s.StartDate < toExclusive.Value);
            }

            if (!fromDate.HasValue && !toExclusive.HasValue)
            {
                periodQuery = yearQuery;
            }

            var periodRevenue = await periodQuery.SumAsync(s => s.Package.Price);

            var roleGroups = await periodQuery
                .GroupBy(s => s.Package.TargetRole)
                .Select(g => new
                {
                    Role = g.Key,
                    Revenue = g.Sum(s => s.Package.Price)
                })
                .ToListAsync();

            var startupRevenue = roleGroups
                .Where(x => x.Role == UserRole.Startup)
                .Sum(x => x.Revenue);
            var investorRevenue = roleGroups
                .Where(x => x.Role == UserRole.Investor)
                .Sum(x => x.Revenue);

            var bestsellerPackage = await periodQuery
                .GroupBy(s => new { s.PackageId, s.Package.PackageName, s.Package.TargetRole })
                .Select(g => new BestsellerPackageResponse
                {
                    PackageId = g.Key.PackageId,
                    PackageName = g.Key.PackageName,
                    TargetRole = g.Key.TargetRole.ToString(),
                    PurchaseCount = g.Count(),
                    TotalRevenue = g.Sum(s => s.Package.Price)
                })
                .OrderByDescending(x => x.PurchaseCount)
                .ThenByDescending(x => x.TotalRevenue)
                .FirstOrDefaultAsync();

            return new PlatformRevenueStatisticsResponse
            {
                Month = targetMonth,
                Year = targetYear,
                FromDate = fromDate,
                ToDate = to?.Date,
                PeriodRevenue = periodRevenue,
                MonthRevenue = monthRevenue,
                YearRevenue = yearRevenue,
                RoleBreakdown = new PlatformRevenueRoleBreakdownResponse
                {
                    StartupRevenue = startupRevenue,
                    InvestorRevenue = investorRevenue,
                    StartupPercent = periodRevenue > 0m ? Math.Round(startupRevenue / periodRevenue * 100m, 2) : 0m,
                    InvestorPercent = periodRevenue > 0m ? Math.Round(investorRevenue / periodRevenue * 100m, 2) : 0m
                },
                BestsellerPackage = bestsellerPackage
            };
        }
    }
}
