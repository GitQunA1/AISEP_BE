using AISEP.BLL.DTOs.Responses;
using AISEP.BLL.Helpers;
using Sieve.Models;

namespace AISEP.BLL.Services.ProjectAdvisorAssignments
{
    public interface IProjectAdvisorAssignmentService
    {
        Task<PagedResult<ProjectAssignedAdvisorResponse>> GetAssignedProjectsForCurrentAdvisorAsync(SieveModel model);
    }
}
