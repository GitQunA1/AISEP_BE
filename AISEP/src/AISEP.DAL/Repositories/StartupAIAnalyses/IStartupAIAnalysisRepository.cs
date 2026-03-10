using AISEP.DAL.Entities;

namespace AISEP.DAL.Repositories.StartupAIAnalyses
{
    public interface IStartupAIAnalysisRepository
    {
        Task<StartupAIAnalysis?> GetByProjectIdAsync(int projectId);
        Task AddAsync(StartupAIAnalysis analysis);
        void Update(StartupAIAnalysis analysis);
    }
}
