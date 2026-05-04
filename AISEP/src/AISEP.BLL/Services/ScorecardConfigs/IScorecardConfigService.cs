using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;

namespace AISEP.BLL.Services.ScorecardConfigs
{
    public interface IScorecardConfigService
    {
        Task<ScorecardWeightConfigResponse> GetDefaultConfigAsync();
        Task<ScorecardWeightConfigResponse> UpdateConfigAsync(int id, UpdateScorecardWeightRequest request);
    }
}
