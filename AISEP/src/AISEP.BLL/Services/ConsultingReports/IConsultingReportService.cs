using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;

namespace AISEP.BLL.Services.ConsultingReports
{
    public interface IConsultingReportService
    {
        Task<ConsultingReportResponse> CreateAsync(CreateConsultingReportRequest request);
        Task<ConsultingReportResponse?> GetByIdAsync(int id);
        Task<ConsultingReportResponse?> GetByBookingIdAsync(int bookingId);
        Task<ConsultingReportResponse> ApproveAsync(int reportId);
        Task<ConsultingReportResponse> RequestRevisionAsync(int reportId, string reason);
        Task<int> ProcessReportDeadlinesAsync();
    }
}
