using AISEP.DAL.Entities;

namespace AISEP.DAL.Repositories.WalletTransactions
{
    public interface IWalletTransactionRepository
    {
        Task AddAsync(WalletTransaction walletTransaction);
    }
}
