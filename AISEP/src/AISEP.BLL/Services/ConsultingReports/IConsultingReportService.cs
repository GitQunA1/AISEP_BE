using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;
using Sieve.Models;

namespace AISEP.BLL.Services.ConsultingReports
{
    public interface IConsultingReportService
    {
        Task<ConsultingReportResponse> CreateAsync(CreateConsultingReportRequest request);
        Task<PagedResult<ConsultingReportResponse>> GetAllAsync(SieveModel model);
        Task<ConsultingReportResponse?> GetByIdAsync(int id);
        Task<ConsultingReportResponse?> GetByBookingIdAsync(int bookingId);
        Task<ConsultingReportResponse> ApproveAsync(int reportId);
        Task<ConsultingReportResponse> RequestRevisionAsync(int reportId, string reason);
        Task<ConsultingReportResponse> AcceptComplaintByStaffAsync(int reportId);
        Task<ConsultingReportResponse> RejectComplaintByStaffAsync(int reportId);
        Task<int> ProcessReportDeadlinesAsync();
    }
}
