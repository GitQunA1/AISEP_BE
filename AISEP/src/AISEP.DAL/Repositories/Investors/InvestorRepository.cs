using AISEP.DAL.Data;
using AISEP.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace AISEP.DAL.Repositories.Investors
{
    public class InvestorRepository : IInvestorRepository
    {
        private readonly ApplicationDbContext _context;

        public InvestorRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public IQueryable<Investor> GetAllQuery()
        {
            return _context.Investors
                .Include(i => i.User)
                .Include(i => i.PreferredStageOption)
                .Include(i => i.InvestorIndustries)
                    .ThenInclude(ii => ii.IndustryOption)
                .OrderBy(i => i.InvestorId)
                .AsQueryable();
        }

        public async Task<Investor?> GetByIdAsync(int investorId)
        {
            return await _context.Investors
                .Include(i => i.User)
                .Include(i => i.PreferredStageOption)
                .Include(i => i.InvestorIndustries)
                    .ThenInclude(ii => ii.IndustryOption)
                .FirstOrDefaultAsync(i => i.InvestorId == investorId);
        }

        public async Task<Investor?> GetByUserIdAsync(int userId)
        {
            return await _context.Investors
                .Include(i => i.User)
                .Include(i => i.PreferredStageOption)
                .Include(i => i.InvestorIndustries)
                    .ThenInclude(ii => ii.IndustryOption)
                .FirstOrDefaultAsync(i => i.UserId == userId);
        }

        public async Task AddAsync(Investor investor)
        {
            await _context.Investors.AddAsync(investor);
        }

        public void Update(Investor investor)
        {
            _context.Investors.Update(investor);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
