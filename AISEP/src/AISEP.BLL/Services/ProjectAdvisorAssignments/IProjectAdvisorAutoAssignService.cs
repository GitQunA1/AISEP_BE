using AISEP.DAL.Entities;

namespace AISEP.BLL.Services.ProjectAdvisorAssignments
{
    public interface IProjectAdvisorAutoAssignService
    {
        Task<bool> TryAssignAdvisorAsync(Project project, CancellationToken cancellationToken = default);
        Task<int> AutoAssignUnassignedApprovedProjectsAsync(CancellationToken cancellationToken = default);
    }
}
