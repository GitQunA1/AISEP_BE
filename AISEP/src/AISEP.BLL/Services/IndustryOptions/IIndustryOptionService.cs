using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;
using Sieve.Models;

namespace AISEP.BLL.Services.IndustryOptions
{
    public interface IIndustryOptionService
    {
        Task<PagedResult<IndustryOptionResponse>> GetAllAsync(SieveModel model);
        Task<IndustryOptionResponse> CreateAsync(CreateIndustryOptionRequest request);
        Task<IndustryOptionResponse> SetActiveAsync(int id, bool isActive);
    }
}
