using AISEP.DAL.Data;
using AISEP.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace AISEP.DAL.Repositories.Payouts
{
    public class PayoutRepository : IPayoutRepository
    {
        private readonly ApplicationDbContext _context;

        public PayoutRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Payout?> GetByIdAsync(int payoutId)
            => await _context.Payouts
                .Include(x => x.Wallet).ThenInclude(w => w.Advisor).ThenInclude(a => a.User)
                .Include(x => x.PaidBy)
                .Include(x => x.RejectedBy)
                .Include(x => x.PayoutGroup)
                .FirstOrDefaultAsync(x => x.PayoutId == payoutId);

        public IQueryable<Payout> GetQuery()
            => _context.Payouts
                .Include(x => x.Wallet).ThenInclude(w => w.Advisor).ThenInclude(a => a.User)
                .Include(x => x.PaidBy)
                .Include(x => x.RejectedBy)
                .Include(x => x.PayoutGroup)
                .OrderByDescending(x => x.PeriodToDate)
                .ThenByDescending(x => x.PeriodFromDate)
                .ThenByDescending(x => x.PayoutId)
                .AsNoTracking();

        public async Task AddAsync(Payout payout)
            => await _context.Payouts.AddAsync(payout);

        public void Update(Payout payout)
            => _context.Payouts.Update(payout);
    }
}






