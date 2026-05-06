using AISEP.BLL.DTOs.Responses;

namespace AISEP.BLL.Services.Admins
{
    public interface IAdminService
    {
        Task<PlatformOverviewResponse> GetPlatformOverviewAsync(DateTime? from, DateTime? to);
        Task<ProjectStatusBreakdownResponse> GetProjectStatusBreakdownAsync();
        Task<InvestmentTrendsResponse> GetInvestmentTrendsAsync(DateTime? from, DateTime? to);
        Task<PlatformRevenueStatisticsResponse> GetPlatformRevenueStatisticsAsync(
            int? month,
            int? year,
            DateTime? from = null,
            DateTime? to = null);
    }
}
