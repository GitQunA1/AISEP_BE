using AISEP.DAL.Data;
using AISEP.DAL.Entities;
using AISEP.DAL.Enums;
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

        public IQueryable<Investor> GetStartupMatchingInvestorsQuery(IEnumerable<int> industryOptionIds, IEnumerable<int> stageOptionIds)
        {
            var industryIds = industryOptionIds
                .Where(id => id > 0)
                .Distinct()
                .ToList();

            var stageIds = stageOptionIds
                .Where(id => id > 0)
                .Distinct()
                .ToList();

            return _context.Investors
                .Include(i => i.User)
                .Include(i => i.PreferredStageOption)
                .Include(i => i.InvestorIndustries)
                    .ThenInclude(ii => ii.IndustryOption)
                .Where(i => i.ApprovalStatus == ApprovalStatus.Approved)
                .OrderByDescending(i => i.InvestorIndustries.Any(ii => industryIds.Contains(ii.IndustryOptionId))
                    && i.PreferredStageOptionId.HasValue
                    && stageIds.Contains(i.PreferredStageOptionId.Value))
                .ThenByDescending(i => i.InvestorIndustries.Any(ii => industryIds.Contains(ii.IndustryOptionId)))
                .ThenByDescending(i => i.PreferredStageOptionId.HasValue && stageIds.Contains(i.PreferredStageOptionId.Value))
                .ThenBy(i => i.InvestorId)
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
