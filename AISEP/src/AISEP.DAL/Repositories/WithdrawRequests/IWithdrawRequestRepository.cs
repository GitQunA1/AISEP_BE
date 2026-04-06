using AISEP.DAL.Entities;

namespace AISEP.DAL.Repositories.WithdrawRequests
{
    public interface IWithdrawRequestRepository
    {
        Task AddAsync(WithdrawRequest withdrawRequest);
        void Update(WithdrawRequest withdrawRequest);
        Task<WithdrawRequest?> GetByIdAsync(int id);
        IQueryable<WithdrawRequest> GetQuery();
        Task<decimal> GetPendingTotalByWalletIdAsync(int walletId);
    }
}
