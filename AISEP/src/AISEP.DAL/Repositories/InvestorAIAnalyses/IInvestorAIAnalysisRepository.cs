using AISEP.DAL.Entities;

namespace AISEP.DAL.Repositories.InvestorAIAnalyses
{
    public interface IInvestorAIAnalysisRepository
    {
        IQueryable<InvestorAIAnalysis> GetQuery();
        Task<InvestorAIAnalysis?> GetByInvestorAndProjectAsync(int investorId, int projectId);
        Task<InvestorAIAnalysis?> GetLatestByProjectAsync(int projectId);
        Task AddAsync(InvestorAIAnalysis analysis);
        void Update(InvestorAIAnalysis analysis);
    }
}
