using AISEP.BLL.Helpers;
using AISEP.BLL.Services.StartupFollowers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;

namespace AISEP.API.Controllers
{
    [ApiController]
    [Route("api/startups")]
    [Authorize]
    public class StartupFollowerController : ControllerBase
    {
        private readonly IStartupFollowerService _followerService;

        public StartupFollowerController(IStartupFollowerService followerService)
        {
            _followerService = followerService;
        }

        [HttpPost("{startupId:int}/follow")]
        public async Task<IActionResult> FollowStartup(int startupId)
        {
            var result = await _followerService.FollowStartupAsync(startupId);
            if (!result)
                return Conflict(ApiResponse<object>.ErrorResponse("You already follow this startup.", "Conflict", 409));

            return Ok(ApiResponse<object>.SuccessResponse(null!, "Followed successfully"));
        }

        [HttpDelete("{startupId:int}/follow")]
        public async Task<IActionResult> UnfollowStartup(int startupId)
        {
            var result = await _followerService.UnfollowStartupAsync(startupId);
            if (!result)
                return NotFound(ApiResponse<object>.ErrorResponse("You are not following this startup.", "Not found", 404));

            return Ok(ApiResponse<object>.SuccessResponse(null!, "Unfollowed successfully"));
        }

        [HttpGet("my-followed")]
        public async Task<IActionResult> GetMyFollowedStartups([FromQuery] SieveModel model)
        {
            try
            {
                var result = await _followerService.GetMyFollowedStartupsAsync(model);
                return Ok(ApiResponse<object>.SuccessResponse(result, "Success"));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<object>.ErrorResponse(ex.Message, ex.Message, 401));
            }
        }
    }
}
