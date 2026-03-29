using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;
using AISEP.BLL.Helpers;
using Sieve.Models;

namespace AISEP.BLL.Services.AdvisorAvailabilities
{
    public interface IAdvisorAvailabilityService
    {
        Task<PagedResult<AdvisorAvailabilityResponse>> GetByAdvisorIdAsync(int advisorId, SieveModel model);
        Task<PagedResult<AdvisorAvailabilityResponse>> GetMyAvailabilitiesAsync(SieveModel model);
        Task<List<AdvisorAvailabilityResponse>> CreateMyAvailabilityAsync(CreateAdvisorAvailabilityRequest request);
        Task<AdvisorAvailabilityResponse> UpdateMyAvailabilityAsync(int availabilityId, UpdateAdvisorAvailabilityRequest request);
        Task<bool> DeleteMyAvailabilityAsync(int availabilityId);
    }
}
