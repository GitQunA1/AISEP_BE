using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;
using Sieve.Models;

namespace AISEP.BLL.Services.SystemCommissions
{
    public interface ISystemCommissionService
    {
        Task<SystemCommissionCurrentResponse> GetCurrentAsync();
        Task<SystemCommissionCurrentResponse> UpdateCurrentAsync(UpdateSystemCommissionRequest request);
        Task<PagedResult<SystemCommissionChangeLogResponse>> GetHistoryAsync(SieveModel model);
    }
}
