using AISEP.BLL.Common;
using AISEP.DAL.Common;
using AISEP.BLL.DTOs.Responses;
using Sieve.Models;

namespace AISEP.BLL.Services.StartupFollowers
{
    public interface IStartupFollowerService
    {
        Task<bool> FollowStartupAsync(int startupId);
        Task<bool> UnfollowStartupAsync(int startupId);
        Task<bool> IsFollowingAsync(int startupId);
        Task<PagedResult<FollowedStartupResponse>> GetMyFollowedStartupsAsync(SieveModel model);
    }
}
