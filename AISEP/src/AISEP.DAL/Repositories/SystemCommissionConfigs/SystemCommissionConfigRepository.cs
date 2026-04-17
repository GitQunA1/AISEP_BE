using AISEP.DAL.Data;
using AISEP.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace AISEP.DAL.Repositories.SystemCommissionConfigs
{
    public class SystemCommissionConfigRepository : ISystemCommissionConfigRepository
    {
        private readonly ApplicationDbContext _context;

        public SystemCommissionConfigRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<SystemCommissionConfig?> GetCurrentAsync(DateTime asOfUtc)
            => await _context.SystemCommissionConfigs
                .Where(x => x.EffectiveFrom <= asOfUtc
                            && (x.EffectiveTo == null || x.EffectiveTo > asOfUtc))
                .OrderByDescending(x => x.EffectiveFrom)
                .FirstOrDefaultAsync();

        public async Task<SystemCommissionConfig?> GetActiveAsync()
            => await _context.SystemCommissionConfigs
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.EffectiveFrom)
                .FirstOrDefaultAsync();

        public IQueryable<SystemCommissionConfig> GetQuery()
            => _context.SystemCommissionConfigs
                .Include(x => x.CreatedBy)
                .OrderByDescending(x => x.EffectiveFrom)
                .AsNoTracking();

        public async Task AddAsync(SystemCommissionConfig config)
            => await _context.SystemCommissionConfigs.AddAsync(config);

        public void Update(SystemCommissionConfig config)
            => _context.SystemCommissionConfigs.Update(config);
    }
}
