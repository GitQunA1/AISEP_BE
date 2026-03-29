using AISEP.DAL.Entities;

namespace AISEP.DAL.Repositories.ProjectAdvisorAssignments
{
    public interface IProjectAdvisorAssignmentRepository
    {
        Task<ProjectAdvisorAssignment?> GetByProjectIdAsync(int projectId);
        Task AddAsync(ProjectAdvisorAssignment assignment);
        void Update(ProjectAdvisorAssignment assignment);
    }
}
