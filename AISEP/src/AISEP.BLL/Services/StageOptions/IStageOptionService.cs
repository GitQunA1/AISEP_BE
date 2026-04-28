using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;
using AISEP.BLL.Helpers;
using Sieve.Models;

namespace AISEP.BLL.Services.StageOptions
{
    public interface IStageOptionService
    {
        Task<PagedResult<StageOptionResponse>> GetAllAsync(SieveModel model);
        Task<StageOptionResponse> CreateAsync(CreateStageOptionRequest request);
        Task<StageOptionResponse> SetActiveAsync(int id, bool isActive);
    }
}
