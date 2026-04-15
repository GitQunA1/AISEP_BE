using AISEP.BLL.DTOs.Responses;
using AISEP.BLL.Helpers;
using Sieve.Models;

namespace AISEP.BLL.Services.AI
{
    public interface IInvestorAIAnalysisService
    {
        Task<InvestorAIAnalysisResponse> AnalyzeProjectForInvestorAsync(int projectId);
        Task<InvestorAIAnalysisResponse?> GetAnalysisAsync(int projectId);
        Task<PagedResult<InvestorAIAnalysisResponse>> GetAllAnalysesAsync(SieveModel model);
    }
}
