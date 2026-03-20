using AISEP.DAL.Data;
using AISEP.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace AISEP.DAL.Repositories.InvestorAIAnalyses
{
    public class InvestorAIAnalysisRepository : IInvestorAIAnalysisRepository
    {
        private readonly ApplicationDbContext _context;

        public InvestorAIAnalysisRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<InvestorAIAnalysis?> GetByInvestorAndProjectAsync(int investorId, int projectId)
        {
            return await _context.InvestorAIAnalyses
                .FirstOrDefaultAsync(x => x.InvestorId == investorId && x.ProjectId == projectId);
        }

        public async Task AddAsync(InvestorAIAnalysis analysis)
        {
            await _context.InvestorAIAnalyses.AddAsync(analysis);
        }

        public void Update(InvestorAIAnalysis analysis)
        {
            _context.InvestorAIAnalyses.Update(analysis);
        }
    }
}
