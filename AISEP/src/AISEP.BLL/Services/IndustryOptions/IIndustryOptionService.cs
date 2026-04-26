using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;
using Sieve.Models;

namespace AISEP.BLL.Services.IndustryOptions
{
    public interface IIndustryOptionService
    {
        Task<PagedResult<IndustryOptionResponse>> GetAllAsync(SieveModel model, bool includeInactive = false);
        Task<IndustryOptionResponse> CreateAsync(CreateIndustryOptionRequest request);
        Task<IndustryOptionResponse> UpdateAsync(int id, UpdateIndustryOptionRequest request);
    }
}
