using AISEP.DAL.Entities;

namespace AISEP.DAL.Repositories.Deals
{
    public interface IDealRepository
    {
        IQueryable<Deal> GetQuery();
        Task<Deal?> GetByIdAsync(int dealId);
        Task<Deal?> GetByIdWithNftAsync(int dealId);
        Task<bool> HasBlockingDealAsync(int investorId, int projectId);
        Task AddAsync(Deal deal);
        void Update(Deal deal);
    }
}
