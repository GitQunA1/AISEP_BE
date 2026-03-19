using AISEP.BLL.DTOs.Responses;

namespace AISEP.BLL.Services.AI
{
    public interface IInvestorAIAnalysisService
    {
        Task<InvestorAIAnalysisResponse> AnalyzeProjectForInvestorAsync(int projectId);
        Task<InvestorAIAnalysisResponse?> GetAnalysisAsync(int projectId);
    }
}
