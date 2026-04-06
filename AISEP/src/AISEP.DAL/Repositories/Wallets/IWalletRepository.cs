using AISEP.DAL.Entities;

namespace AISEP.DAL.Repositories.Wallets
{
    public interface IWalletRepository
    {
        Task<Wallet?> GetByAdvisorIdAsync(int advisorId);
        Task AddAsync(Wallet wallet);
        void Update(Wallet wallet);
    }
}
