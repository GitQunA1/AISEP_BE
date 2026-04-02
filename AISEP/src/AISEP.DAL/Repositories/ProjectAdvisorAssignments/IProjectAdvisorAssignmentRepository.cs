using AISEP.DAL.Entities;

namespace AISEP.DAL.Repositories.ProjectAdvisorAssignments
{
    public interface IProjectAdvisorAssignmentRepository
    {
        Task<ProjectAdvisorAssignment?> GetByProjectIdAsync(int projectId);
        IQueryable<ProjectAdvisorAssignment> GetAllQuery();
        IQueryable<ProjectAdvisorAssignment> GetByAdvisorIdQuery(int advisorId);
        Task AddAsync(ProjectAdvisorAssignment assignment);
        void Update(ProjectAdvisorAssignment assignment);
    }
}
