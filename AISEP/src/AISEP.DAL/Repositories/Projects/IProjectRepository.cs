using AISEP.DAL.Entities;
using AISEP.DAL.Enums;

namespace AISEP.DAL.Repositories.Projects
{
    public interface IProjectRepository
    {
        IQueryable<Project> GetAllQuery();
        IQueryable<Project> GetByStartupIdQuery(int startupId);
        IQueryable<Project> GetByStatusQuery(ProjectStatus status);
        Task<Project?> GetByIdAsync(int id);
        Task AddAsync(Project project);
        void Update(Project project);
    }
}
