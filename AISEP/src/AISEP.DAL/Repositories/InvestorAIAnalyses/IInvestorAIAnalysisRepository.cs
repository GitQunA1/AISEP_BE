using AISEP.DAL.Entities;

namespace AISEP.DAL.Repositories.InvestorAIAnalyses
{
    public interface IInvestorAIAnalysisRepository
    {
        Task<InvestorAIAnalysis?> GetByInvestorAndProjectAsync(int investorId, int projectId);
        Task AddAsync(InvestorAIAnalysis analysis);
        void Update(InvestorAIAnalysis analysis);
    }
}
