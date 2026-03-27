using AISEP.DAL.Entities;

namespace AISEP.DAL.Repositories.Deals
{
    public interface IDealRepository
    {
        IQueryable<Deal> GetQuery();
        Task<Deal?> GetByIdAsync(int dealId);
        Task<Deal?> GetByIdWithNftAsync(int dealId);
        Task AddAsync(Deal deal);
        void Update(Deal deal);
    }
}
