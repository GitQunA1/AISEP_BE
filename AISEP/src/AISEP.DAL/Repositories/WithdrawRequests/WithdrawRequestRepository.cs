using AISEP.DAL.Data;
using AISEP.DAL.Entities;
using AISEP.DAL.Enums;
using Microsoft.EntityFrameworkCore;

namespace AISEP.DAL.Repositories.WithdrawRequests
{
    public class WithdrawRequestRepository : IWithdrawRequestRepository
    {
        private readonly ApplicationDbContext _context;

        public WithdrawRequestRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(WithdrawRequest withdrawRequest)
            => await _context.WithdrawRequests.AddAsync(withdrawRequest);

        public void Update(WithdrawRequest withdrawRequest)
            => _context.WithdrawRequests.Update(withdrawRequest);

        public async Task<WithdrawRequest?> GetByIdAsync(int id)
            => await _context.WithdrawRequests
                .Include(x => x.Wallet)
                    .ThenInclude(w => w.Advisor)
                        .ThenInclude(a => a.User)
                .FirstOrDefaultAsync(x => x.WithdrawRequestId == id);

        public IQueryable<WithdrawRequest> GetQuery()
            => _context.WithdrawRequests
                .Include(x => x.Wallet)
                    .ThenInclude(w => w.Advisor)
                        .ThenInclude(a => a.User)
                .OrderByDescending(x => x.RequestedAt)
                .AsNoTracking();

        public async Task<decimal> GetPendingTotalByWalletIdAsync(int walletId)
            => await _context.WithdrawRequests
                .Where(x => x.WalletId == walletId && x.Status == WithdrawRequestStatus.Pending)
                .SumAsync(x => (decimal?)x.Amount) ?? 0m;
    }
}
