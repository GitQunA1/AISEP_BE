using AISEP.DAL.Data;
using AISEP.DAL.Entities;
using AISEP.DAL.Enums;
using Microsoft.EntityFrameworkCore;

namespace AISEP.DAL.Repositories.WalletTransactions
{
    public class WalletTransactionRepository : IWalletTransactionRepository
    {
        private readonly ApplicationDbContext _context;

        public WalletTransactionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(WalletTransaction walletTransaction)
            => await _context.WalletTransactions.AddAsync(walletTransaction);

        public IQueryable<WalletTransaction> GetByWalletIdQuery(int walletId)
            => _context.WalletTransactions
                .Where(x => x.WalletId == walletId)
                .OrderByDescending(x => x.CreatedAt)
                .AsNoTracking();

        public IQueryable<WalletTransaction> GetCompletedDepositsWithoutPayoutQuery(DateTime periodStartUtc, DateTime periodEndUtc)
            => _context.WalletTransactions
                .Include(x => x.Wallet)
                .Where(x =>
                    x.Type == WalletTransactionType.Deposit
                    && x.Status == WalletTransactionStatus.Completed
                    && x.MonthlyPayoutId == null
                    && x.CreatedAt >= periodStartUtc
                    && x.CreatedAt < periodEndUtc);
    }
}
