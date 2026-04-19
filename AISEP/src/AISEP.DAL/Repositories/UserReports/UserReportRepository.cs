using AISEP.DAL.Data;
using AISEP.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace AISEP.DAL.Repositories.UserReports
{
    public class UserReportRepository : IUserReportRepository
    {
        private readonly ApplicationDbContext _context;

        public UserReportRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<UserReport?> GetByIdAsync(int id)
        {
            return await _context.UserReports
                .Include(r => r.Booking!)
                    .ThenInclude(b => b.Advisor)
                        .ThenInclude(a => a.User)
                .Include(r => r.Booking!)
                    .ThenInclude(b => b.Customer)
                .Include(r => r.Reporter)
                .Include(r => r.ResolvedBy)
                .FirstOrDefaultAsync(r => r.UserReportId == id);
        }

        public async Task AddAsync(UserReport report)
        {
            await _context.UserReports.AddAsync(report);
        }

        public void Update(UserReport report)
        {
            _context.UserReports.Update(report);
        }

        public IQueryable<UserReport> GetAll()
        {
            return _context.UserReports
                .Include(r => r.Booking!)
                    .ThenInclude(b => b.Advisor)
                        .ThenInclude(a => a.User)
                .Include(r => r.Booking!)
                    .ThenInclude(b => b.Customer)
                .Include(r => r.Reporter)
                .Include(r => r.ResolvedBy)
                .AsQueryable();
        }
    }
}
