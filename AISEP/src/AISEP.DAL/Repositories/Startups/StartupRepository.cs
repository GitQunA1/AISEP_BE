using AISEP.DAL.Data;
using AISEP.DAL.Entities;
using AISEP.DAL.Enums;
using Microsoft.EntityFrameworkCore;

namespace AISEP.DAL.Repositories.Startups
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
                .Include(s => s.User)
                .Include(s => s.Followers)
                .Where(s =>
                    s.ApprovalStatus == ApprovalStatus.Approved &&
                    (string.IsNullOrWhiteSpace(industry) || (s.Industry != null && s.Industry.ToString()!.ToLower().Contains(industry.ToLower()))) &&
                    (string.IsNullOrWhiteSpace(searchTerm) || (s.CompanyName != null && s.CompanyName.ToLower().Contains(searchTerm.ToLower())))
                )
                .OrderBy(s => s.StartupId)
                .AsQueryable();
        }

        public async Task<Startup?> GetByIdAsync(int id)
        {
            return await _context.Startups
                .Include(s => s.User)
                .Include(s => s.Followers)
                .FirstOrDefaultAsync(s => s.StartupId == id);
        }

        public async Task<Startup?> GetByUserIdAsync(int userId)
        {
            return await _context.Startups
                .Include(s => s.User)
                .Include(s => s.Followers)
                .FirstOrDefaultAsync(s => s.UserId == userId);
        }

        public IQueryable<Startup> GetStartupQuery()
        {
            return _context.Startups
                .Include(s => s.User)
                .Include(s => s.Followers)
                .OrderBy(s => s.StartupId)
                .AsQueryable();
        }

        public IQueryable<Startup> GetPendingStartupsQuery()
        {
            return _context.Startups
                .Include(s => s.User)
                .Include(s => s.Followers)
                .Where(s => s.ApprovalStatus == ApprovalStatus.Pending)
                .OrderBy(s => s.StartupId)
                .AsQueryable();
        }

        public IQueryable<Startup> GetByStatusQuery(ApprovalStatus? status = null)
        {
            var query = _context.Startups
                .Include(s => s.User)
                .Include(s => s.Followers)
                .OrderBy(s => s.StartupId)
                .AsQueryable();

            if (status.HasValue)
                query = query.Where(s => s.ApprovalStatus == status.Value);

            return query;
        }

        public async Task AddAsync(Startup startup)
        {
            await _context.Startups.AddAsync(startup);
        }

        public void Update(Startup startup)
        {
            _context.Startups.Update(startup);
        }
    }
}
