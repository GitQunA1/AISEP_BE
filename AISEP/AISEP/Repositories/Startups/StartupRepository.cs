using AISEP.Data;
using AISEP.Models.Entities;
using AISEP.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace AISEP.Repositories.Startups
{
    public class StartupRepository : IStartupRepository
    {
        private readonly ApplicationDbContext _context;

        public StartupRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public IQueryable<Startup> SearchStartupsQuery(string? industry = null, DevelopmentStage? stage = null, string? searchTerm = null)
        {
            return _context.Startups
                .Include(s => s.Followers)
                .Include(s => s.User)
                .Where(s =>
                    (string.IsNullOrWhiteSpace(industry) || (s.Industry != null && s.Industry.ToLower().Contains(industry.ToLower())))
                    && (!stage.HasValue || s.DevelopmentStage == stage.Value)
                    && (string.IsNullOrWhiteSpace(searchTerm) || 
                        (s.CompanyName != null && s.CompanyName.ToLower().Contains(searchTerm.ToLower())) ||
                        (s.ProblemStatement != null && s.ProblemStatement.ToLower().Contains(searchTerm.ToLower())) ||
                        (s.SolutionDescription != null && s.SolutionDescription.ToLower().Contains(searchTerm.ToLower()))
                    )
                )
                .AsQueryable();
        }

        public async Task<Startup?> GetByIdAsync(int id)
        {
            return await _context.Startups
                .Include(s => s.Followers)
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.StartupId == id);
        }

        public IQueryable<Startup> GetStartupQuery()
        {
            return _context.Startups
                .Include(s => s.Followers)
                .Include(s => s.User)
                .AsQueryable();
        }
    }
}
