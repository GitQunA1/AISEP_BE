using AISEP.DAL.Data;
using AISEP.DAL.Entities;
using AISEP.DAL.Enums;
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
                    .ThenInclude(i => i.User)
                .Include(d => d.Project)
                    .ThenInclude(p => p.Startup)
                        .ThenInclude(s => s.User)
                .AsQueryable();
        }

        public async Task<Deal?> GetByIdAsync(int dealId)
        {
            return await GetQuery().FirstOrDefaultAsync(d => d.DealId == dealId);
        }

        public async Task<Deal?> GetByIdWithDetailsAsync(int dealId)
        {
            return await _context.Deals
                .Include(d => d.Project)
                    .ThenInclude(p => p.Startup)
                        .ThenInclude(s => s.User)
                .Include(d => d.Investor)
                    .ThenInclude(i => i.User)
                .FirstOrDefaultAsync(d => d.DealId == dealId);
        }

        public async Task<bool> HasBlockingDealAsync(int investorId, int projectId)
        {
            return await _context.Deals.AnyAsync(d =>
                d.InvestorId == investorId &&
                d.ProjectId == projectId &&
                (d.Status == DealStatus.Pending ||
                 d.Status == DealStatus.Confirmed ||
                 d.Status == DealStatus.Waiting_For_Startup_Signature ||
                 d.Status == DealStatus.Contract_Signed));
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
