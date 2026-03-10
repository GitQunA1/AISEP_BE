using AISEP.DAL.Data;
using AISEP.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace AISEP.DAL.Repositories.StartupAIAnalyses
{
    public class StartupAIAnalysisRepository : IStartupAIAnalysisRepository
    {
        private readonly ApplicationDbContext _context;

        public StartupAIAnalysisRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<StartupAIAnalysis?> GetByProjectIdAsync(int projectId)
        {
            return await _context.StartupAIAnalyses
                .FirstOrDefaultAsync(a => a.ProjectId == projectId);
        }

        public async Task AddAsync(StartupAIAnalysis analysis)
        {
            await _context.StartupAIAnalyses.AddAsync(analysis);
        }

        public void Update(StartupAIAnalysis analysis)
        {
            _context.StartupAIAnalyses.Update(analysis);
        }
    }
}
