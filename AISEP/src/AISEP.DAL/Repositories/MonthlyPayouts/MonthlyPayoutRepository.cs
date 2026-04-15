using AISEP.DAL.Data;
using AISEP.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace AISEP.DAL.Repositories.MonthlyPayouts
{
    public class MonthlyPayoutRepository : IMonthlyPayoutRepository
    {
        private readonly ApplicationDbContext _context;

        public MonthlyPayoutRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<MonthlyPayout?> GetByIdAsync(int monthlyPayoutId)
            => await _context.MonthlyPayouts
                .Include(x => x.Wallet).ThenInclude(w => w.Advisor).ThenInclude(a => a.User)
                .Include(x => x.ApprovedBy)
                .Include(x => x.PaidBy)
                .Include(x => x.RejectedBy)
                .Include(x => x.RetryRequestedBy)
                .Include(x => x.RetryReviewedBy)
                .Include(x => x.MonthlyPayoutBatch)
                .FirstOrDefaultAsync(x => x.MonthlyPayoutId == monthlyPayoutId);

        public async Task<MonthlyPayout?> GetByAdvisorAndPeriodAsync(int advisorId, int year, int month)
            => await _context.MonthlyPayouts
                .FirstOrDefaultAsync(x => x.Wallet.AdvisorId == advisorId && x.Year == year && x.Month == month);

        public async Task<bool> ExistsByPeriodAsync(int year, int month)
            => await _context.MonthlyPayouts
                .AnyAsync(x => x.Year == year && x.Month == month);

        public IQueryable<MonthlyPayout> GetQuery()
            => _context.MonthlyPayouts
                .Include(x => x.Wallet).ThenInclude(w => w.Advisor).ThenInclude(a => a.User)
                .Include(x => x.ApprovedBy)
                .Include(x => x.PaidBy)
                .Include(x => x.RejectedBy)
                .Include(x => x.RetryRequestedBy)
                .Include(x => x.RetryReviewedBy)
                .Include(x => x.MonthlyPayoutBatch)
                .OrderByDescending(x => x.Year)
                .ThenByDescending(x => x.Month)
                .ThenByDescending(x => x.MonthlyPayoutId)
                .AsNoTracking();

        public async Task AddAsync(MonthlyPayout monthlyPayout)
            => await _context.MonthlyPayouts.AddAsync(monthlyPayout);

        public void Update(MonthlyPayout monthlyPayout)
            => _context.MonthlyPayouts.Update(monthlyPayout);
    }
}
