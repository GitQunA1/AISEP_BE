using AISEP.BLL.DTOs.Responses;
using AISEP.BLL.Helpers;
using Sieve.Models;

namespace AISEP.BLL.Services.ProjectAdvisorAssignments
{
    public interface IProjectAdvisorAssignmentService
    {
        Task<List<ProjectAssignedAdvisorResponse>> GetAssignedAdvisorsByProjectAsync(int projectId);
        Task<PagedResult<ProjectAssignedAdvisorResponse>> GetAssignedProjectsForCurrentAdvisorAsync(SieveModel model);
    }
}
