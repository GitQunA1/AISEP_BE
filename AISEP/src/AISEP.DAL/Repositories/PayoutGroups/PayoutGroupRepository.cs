using AISEP.DAL.Data;
using AISEP.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace AISEP.DAL.Repositories.PayoutGroups
{
    public class PayoutGroupRepository : IPayoutGroupRepository
    {
        private readonly ApplicationDbContext _context;

        public PayoutGroupRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PayoutGroup?> GetByIdAsync(int id)
            => await _context.PayoutGroups
                .Include(x => x.Payouts)
                .FirstOrDefaultAsync(x => x.PayoutGroupId == id);

        public IQueryable<PayoutGroup> GetQuery()
            => _context.PayoutGroups
                .Include(x => x.Payouts)
                .OrderByDescending(x => x.ToDate)
                .ThenByDescending(x => x.FromDate)
                .ThenByDescending(x => x.PayoutGroupId)
                .AsNoTracking();

        public async Task AddAsync(PayoutGroup batch)
            => await _context.PayoutGroups.AddAsync(batch);

        public void Update(PayoutGroup batch)
            => _context.PayoutGroups.Update(batch);
    }
}





