using AISEP.DAL.Entities;

namespace AISEP.DAL.Repositories.WalletTransactions
{
    public interface IWalletTransactionRepository
    {
        Task AddAsync(WalletTransaction walletTransaction);
        IQueryable<WalletTransaction> GetByWalletIdQuery(int walletId);
        IQueryable<WalletTransaction> GetCompletedDepositsWithoutPayoutQuery(DateTime periodStartUtc, DateTime periodEndUtc);
    }
}
