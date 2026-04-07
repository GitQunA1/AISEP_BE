using AISEP.DAL.Entities;

namespace AISEP.DAL.Repositories.Wallets
{
    public interface IWalletRepository
    {
        Task<Wallet?> GetByAdvisorIdAsync(int advisorId);
        IQueryable<Wallet> GetAllQuery();
        Task AddAsync(Wallet wallet);
        void Update(Wallet wallet);
    }
}
