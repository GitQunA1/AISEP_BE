using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;

namespace AISEP.BLL.Services.UserReports
{
    public interface IUserReportService
    {
        Task<UserReportResponse> CreateAsync(CreateUserReportRequest request);
        Task<UserReportResponse> ResolveAsValidAsync(int reportId);
        Task<UserReportResponse> ResolveAsFalseAsync(int reportId);
    }
}
