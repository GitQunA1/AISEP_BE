using AISEP.DAL.Entities;

namespace AISEP.DAL.Repositories.PayoutGroups
{
    public interface IPayoutGroupRepository
    {
        Task<PayoutGroup?> GetByIdAsync(int id);
        IQueryable<PayoutGroup> GetQuery();
        Task AddAsync(PayoutGroup batch);
        void Update(PayoutGroup batch);
    }
}


