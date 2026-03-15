using AISEP.BLL.Helpers;
using AISEP.DAL.Common;
using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;
using Sieve.Models;

namespace AISEP.BLL.Services.Projects
{
    public interface IProjectService
    {
        Task<PagedResult<ProjectResponse>> GetAllProjectsAsync(SieveModel model);
        Task<ProjectResponse?> GetProjectByIdAsync(int id);
        Task<PagedResult<ProjectResponse>> GetMyProjectsAsync(int userId, SieveModel model);
        Task<PagedResult<ProjectResponse>> GetDraftProjectsAsync(SieveModel model);

        Task<ProjectResponse> CreateProjectAsync(CreateProjectRequest dto);
        Task<ProjectResponse> UpdateProjectAsync(int projectId, UpdateProjectRequest dto);

        Task ApproveProjectAsync(int projectId);
        Task RejectProjectAsync(int projectId, RejectProjectRequest dto);
    }
}
