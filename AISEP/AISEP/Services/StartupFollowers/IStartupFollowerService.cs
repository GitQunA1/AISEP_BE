using AISEP.Common;
using AISEP.DTOs.Responses;
using Sieve.Models;

namespace AISEP.Services.StartupFollowers
{
    public interface IStartupFollowerService
    {
        Task<bool> FollowStartupAsync(int startupId);
        Task<bool> UnfollowStartupAsync(int startupId);
        Task<bool> IsFollowingAsync(int startupId);
        Task<PagedResult<FollowedStartupResponse>> GetMyFollowedStartupsAsync(SieveModel model);
    }
}
