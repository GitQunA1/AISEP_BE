using AISEP.Common;
using AISEP.DTOs.Requests;
using AISEP.DTOs.Responses;
using Sieve.Models;

namespace AISEP.Services.Projects
{
    public interface IProjectService
    {
        Task<PagedResult<ProjectResponse>> GetAllProjectsAsync(SieveModel model);
        Task<ProjectResponse?> GetProjectByIdAsync(int id);
        Task<PagedResult<ProjectResponse>> GetMyProjectsAsync(int userId, SieveModel model);

        Task<ProjectResponse> CreateProjectAsync(int userId, CreateProjectRequest dto);
        Task SubmitProjectAsync(int projectId, int userId);

        Task ApproveProjectAsync(int projectId, ApproveProjectRequest dto);
        Task RejectProjectAsync(int projectId, RejectProjectRequest dto);
    }
}
