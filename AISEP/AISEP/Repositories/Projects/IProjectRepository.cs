using AISEP.Models.Entities;
using AISEP.Models.Enums;

namespace AISEP.Repositories.Projects
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
