using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;
using Sieve.Models;

namespace AISEP.BLL.Services.UserReports
{
    public interface IUserReportService
    {
        Task<UserReportResponse> CreateAsync(CreateUserReportRequest request);
        Task<UserReportResponse> ResolveAsValidAsync(int reportId);
        Task<UserReportResponse> ResolveAsFalseAsync(int reportId);
        Task<PagedResult<UserReportResponse>> GetUserReports(SieveModel sieveModel);
        Task<PagedResult<UserReportResponse>> GetMyReportsAsReporterAsync(SieveModel sieveModel);
        Task<PagedResult<UserReportResponse>> GetMyReportsAsReportedUserAsync(SieveModel sieveModel);
    }
}
