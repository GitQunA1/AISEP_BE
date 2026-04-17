using AISEP.DAL.Entities;

namespace AISEP.DAL.Repositories.Payouts
{
    public interface IPayoutRepository
    {
        Task<Payout?> GetByIdAsync(int payoutId);
        IQueryable<Payout> GetQuery();
        Task AddAsync(Payout payout);
        void Update(Payout payout);
    }
}



