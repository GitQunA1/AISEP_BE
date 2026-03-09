using AISEP.DTOs.Responses;

namespace AISEP.Services.AI
{
    public interface IStartupAIAnalysisService
    {
        Task<StartupAIAnalysisResponse> AnalyzeProjectAsync(int projectId);
        Task<StartupAIAnalysisResponse?> GetAnalysisAsync(int projectId);
    }
}
