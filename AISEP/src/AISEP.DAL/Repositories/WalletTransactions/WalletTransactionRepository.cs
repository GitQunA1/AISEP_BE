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

        public async Task<bool> ExistsWithdrawalByWithdrawRequestIdAsync(int withdrawRequestId)
            => await _context.WalletTransactions.AnyAsync(x =>
                x.WithdrawRequestId == withdrawRequestId
                && x.Type == WalletTransactionType.Withdrawal);
    }
}
