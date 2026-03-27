using AISEP.DAL.Data;
using AISEP.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace AISEP.DAL.Repositories.Deals
{
    public class DealRepository : IDealRepository
    {
        private readonly ApplicationDbContext _context;

        public DealRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public IQueryable<Deal> GetQuery()
        {
            return _context.Deals
                .Include(d => d.Investor)
                .Include(d => d.Project)
                    .ThenInclude(p => p.Startup)
                .Include(d => d.NFTRecord)
                .AsQueryable();
        }

        public async Task<Deal?> GetByIdAsync(int dealId)
        {
            return await GetQuery().FirstOrDefaultAsync(d => d.DealId == dealId);
        }

        public async Task<Deal?> GetByIdWithNftAsync(int dealId)
        {
            return await _context.Deals
                .Include(d => d.Project)
                    .ThenInclude(p => p.Startup)
                .Include(d => d.Investor)
                .Include(d => d.NFTRecord)
                .FirstOrDefaultAsync(d => d.DealId == dealId);
        }

        public async Task AddAsync(Deal deal)
        {
            await _context.Deals.AddAsync(deal);
        }

        public void Update(Deal deal)
        {
            _context.Deals.Update(deal);
        }
    }
}
