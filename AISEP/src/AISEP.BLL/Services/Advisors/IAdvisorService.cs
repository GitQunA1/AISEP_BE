using AISEP.BLL.Helpers;
using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;
using Sieve.Models;

namespace AISEP.BLL.Services.Advisors
{
    public interface IAdvisorService
    {
        Task<PagedResult<AdvisorResponse>> GetAllAsync(SieveModel model);
        Task<AdvisorResponse?> GetByIdAsync(int advisorId);
        Task<AdvisorResponse?> GetMyProfileAsync(int userId);
        Task<AdvisorResponse?> CreateAsync( CreateAdvisorRequest dto);
        Task<AdvisorResponse?> UpdateAsync(int id, UpdateAdvisorRequest dto);
        Task ApproveAdvisorAsync(int advisorId);
        Task RejectAdvisorAsync(int advisorId, string rejectionReason);
        Task<bool> DeleteAsync(int advisorId);
    }
}
