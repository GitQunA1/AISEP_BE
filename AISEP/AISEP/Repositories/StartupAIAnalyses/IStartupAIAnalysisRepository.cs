using AISEP.Models.Entities;

namespace AISEP.Repositories.StartupAIAnalyses
{
    public interface IStartupAIAnalysisRepository
    {
        Task<StartupAIAnalysis?> GetByProjectIdAsync(int projectId);
        Task AddAsync(StartupAIAnalysis analysis);
        void Update(StartupAIAnalysis analysis);
    }
}
