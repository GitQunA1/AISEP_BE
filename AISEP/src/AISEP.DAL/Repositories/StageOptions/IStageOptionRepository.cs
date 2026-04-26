using AISEP.DAL.Entities;

namespace AISEP.DAL.Repositories.StageOptions
{
    public interface IStageOptionRepository
    {
        IQueryable<StageOption> GetAllQuery();
        IQueryable<StageOption> GetActiveQuery();
        Task<StageOption?> GetByIdAsync(int id);
        Task<List<StageOption>> GetByIdsAsync(IEnumerable<int> ids);
        Task AddAsync(StageOption option);
        void Update(StageOption option);
    }
}
