using AISEP.BLL.Common;
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
        Task<AdvisorResponse?> CreateAsync(int userId, CreateAdvisorRequest dto);
        Task<AdvisorResponse?> UpdateAsync(int userId, UpdateAdvisorRequest dto);
        Task<bool> DeleteAsync(int advisorId);
    }
}
