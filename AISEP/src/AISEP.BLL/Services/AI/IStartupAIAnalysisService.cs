using AISEP.BLL.DTOs.Responses;

namespace AISEP.BLL.Services.AI
{
    public interface IStartupAIAnalysisService
    {
        Task<StartupAIAnalysisResponse> AnalyzeProjectAsync(int projectId);
        Task<StartupAIAnalysisResponse?> GetAnalysisAsync(int projectId);
        Task<StartupEligibilityResponse> EvaluateEligibilityAsync(int projectId);
        Task<StartupEligibilityResponse?> GetEligibilityEvaluationAsync(int projectId);
    }
}
