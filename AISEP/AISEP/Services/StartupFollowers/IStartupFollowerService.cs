using AISEP.DTOs;
using Sieve.Models;

namespace AISEP.Services.StartupFollowers
{
    public interface IStartupFollowerService
    {
        Task<bool> FollowStartupAsync(Guid startupId);
        Task<bool> UnfollowStartupAsync(Guid startupId);
        Task<bool> IsFollowingAsync(Guid startupId);
        //Task<StartupFollowerResponseDto?> GetFollowerByIdAsync(Guid userId, Guid startupId);
        Task<PagedResultDto<FollowedStartupDto>> GetMyFollowedStartupsAsync(SieveModel model);
        //Task<PagedResultDto<StartupFollowerResponseDto>> GetStartupFollowersAsync(Guid startupId, SieveModel model);
    }
}
