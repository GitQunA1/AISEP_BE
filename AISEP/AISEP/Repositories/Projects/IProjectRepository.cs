using AISEP.Models.Entities;

namespace AISEP.Repositories.Projects
{
    public interface IProjectRepository
    {
        IQueryable<Project> GetAllQuery();
        IQueryable<Project> GetByStartupIdQuery(int startupId);
        Task<Project?> GetByIdAsync(int id);
        Task AddAsync(Project project);
        void Update(Project project);
    }
}
