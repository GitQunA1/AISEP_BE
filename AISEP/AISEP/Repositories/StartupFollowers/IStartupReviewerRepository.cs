using AISEP.Models.Entities;

namespace AISEP.Repositories.StartupFollowers
{
    public interface IStartupFollowerRepository
    {
        Task AddAsync(StartupFollower startupFollower);
        Task RemoveAsync(Guid userId, Guid startupId);
        Task<StartupFollower?> GetByIdAsync(Guid userId, Guid startupId);
        Task<bool> IsFollowingAsync(Guid userId, Guid startupId);
        IQueryable<StartupFollower> GetFollowerQuery();
    }
}
