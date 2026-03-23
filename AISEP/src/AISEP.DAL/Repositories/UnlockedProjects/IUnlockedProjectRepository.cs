using AISEP.DAL.Entities;

namespace AISEP.DAL.Repositories.UnlockedProjects
{
    public interface IUnlockedProjectRepository
    {
        Task<bool> ExistsAsync(int userId, int projectId);
        Task AddAsync(UnlockedProject unlockedProject);
    }
}
