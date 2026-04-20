using AISEP.BLL.DTOs.Responses;

namespace AISEP.BLL.Services.AdminMonitoring
{
    public interface IAdminMonitoringService
    {
        Task<AdminStatusResponse> GetStatusAsync();
    }
}
