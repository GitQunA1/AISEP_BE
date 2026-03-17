using AISEP.DAL.Data;
using AISEP.DAL.Entities;

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
    }
}
