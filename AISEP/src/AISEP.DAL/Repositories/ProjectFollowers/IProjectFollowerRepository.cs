using AISEP.DAL.Entities;

namespace AISEP.DAL.Repositories.ProjectFollowers
{
    public interface IProjectFollowerRepository
    {
        Task AddAsync(ProjectFollower projectFollower);
        Task RemoveAsync(int userId, int projectId);
        Task<ProjectFollower?> GetByIdAsync(int userId, int projectId);
        Task<bool> IsFollowingAsync(int userId, int projectId);
        IQueryable<ProjectFollower> GetFollowerQuery();
    }
}
