using AISEP.DAL.Data;
using AISEP.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace AISEP.DAL.Repositories.SystemCommissionChangeLogs
{
    public class SystemCommissionChangeLogRepository : ISystemCommissionChangeLogRepository
    {
        private readonly ApplicationDbContext _context;

        public SystemCommissionChangeLogRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public IQueryable<SystemCommissionChangeLog> GetQuery()
            => _context.SystemCommissionChangeLogs
                .Include(x => x.ChangedBy)
                .OrderByDescending(x => x.ChangedAt)
                .AsNoTracking();

        public async Task AddAsync(SystemCommissionChangeLog log)
            => await _context.SystemCommissionChangeLogs.AddAsync(log);
    }
}
