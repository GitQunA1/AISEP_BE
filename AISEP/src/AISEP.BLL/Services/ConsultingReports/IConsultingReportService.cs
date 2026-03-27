using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;

namespace AISEP.BLL.Services.ConsultingReports
{
    public interface IConsultingReportService
    {
        Task<ConsultingReportResponse> CreateAsync(CreateConsultingReportRequest request);
        Task<ConsultingReportResponse?> GetByIdAsync(int id);
        Task<ConsultingReportResponse?> GetByBookingIdAsync(int bookingId);
    }
}
