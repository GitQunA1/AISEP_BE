using AISEP.DTOs;
using Sieve.Models;

namespace AISEP.Services.Projects
{
    public interface IProjectService
    {
        Task<PagedResultDto<ProjectResponseDto>> GetAllProjectsAsync(SieveModel model);
        Task<ProjectResponseDto?> GetProjectByIdAsync(int id);
        Task<PagedResultDto<ProjectResponseDto>> GetMyProjectsAsync(int userId, SieveModel model);

        Task<ProjectResponseDto> CreateProjectAsync(int userId, CreateProjectDto dto);
        Task SubmitProjectAsync(int projectId, int userId);

        Task ApproveProjectAsync(int projectId, ReviewProjectDto dto);
        Task RejectProjectAsync(int projectId, RejectProjectDto dto);
    }
}
