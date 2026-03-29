using AISEP.DAL.Entities;

namespace AISEP.DAL.Repositories.StartupFollowers
{
    public interface IStartupFollowerRepository
    {
        Task AddAsync(StartupFollower startupFollower);
        Task RemoveAsync(int userId, int startupId);
        Task<StartupFollower?> GetByIdAsync(int userId, int startupId);
        Task<bool> IsFollowingAsync(int userId, int startupId);
        IQueryable<StartupFollower> GetFollowerQuery();
    }
}
