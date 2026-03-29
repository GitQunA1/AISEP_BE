using AISEP.BLL.DTOs.Responses;
using AISEP.BLL.Helpers;
using Sieve.Models;

namespace AISEP.BLL.Services.ProjectFollowers
{
    public interface IProjectFollowerService
    {
        Task<bool> FollowProjectAsync(int projectId);
        Task<bool> UnfollowProjectAsync(int projectId);
        Task<bool> IsFollowingAsync(int projectId);
        Task<PagedResult<FollowedProjectResponse>> GetMyFollowedProjectsAsync(SieveModel model);
    }
}
