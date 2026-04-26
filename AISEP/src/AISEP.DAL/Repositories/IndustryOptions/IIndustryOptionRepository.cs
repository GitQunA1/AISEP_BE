using AISEP.DAL.Entities;

namespace AISEP.DAL.Repositories.IndustryOptions
{
    public interface IIndustryOptionRepository
    {
        IQueryable<IndustryOption> GetAllQuery();
        IQueryable<IndustryOption> GetActiveQuery();
        Task<IndustryOption?> GetByIdAsync(int id);
        Task<List<IndustryOption>> GetByIdsAsync(IEnumerable<int> ids);
        Task AddAsync(IndustryOption option);
        void Update(IndustryOption option);
    }
}
