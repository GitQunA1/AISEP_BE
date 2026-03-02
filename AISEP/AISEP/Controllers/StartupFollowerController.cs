using AISEP.Common;
using AISEP.DTOs;
using AISEP.Services.StartupFollowers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;

namespace AISEP.Controllers
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

        /// <summary>
        /// Follow một startup
        /// </summary>
        [HttpPost("{startupId:int}/follow")]
        public async Task<IActionResult> FollowStartup(int startupId)
        {
            var result = await _followerService.FollowStartupAsync(startupId);
            if (!result)
                return Conflict(ApiResponse.Fail("You already follow this startup."));

            return Ok(ApiResponse.Success());
        }

        [HttpDelete("{startupId:int}/follow")]
        public async Task<IActionResult> UnfollowStartup(int startupId)
        {
            var result = await _followerService.UnfollowStartupAsync(startupId);
            if (!result)
                return NotFound(ApiResponse.Fail("You are not following this startup."));

            return Ok(ApiResponse.Success());
        }

        
        //[HttpGet("{startupId:guid}/is-following")]
        //public async Task<IActionResult> IsFollowing(Guid startupId)
        //{
        //    try
        //    {
        //        var isFollowing = await _followerService.IsFollowingAsync(startupId);
        //        return Ok(new { isFollowing });
        //    }
        //    catch (UnauthorizedAccessException ex)
        //    {
        //        return Unauthorized(new { message = ex.Message });
        //    }
        //}

       
        //[HttpGet("{startupId:guid}/followers/{userId:guid}")]
        //[AllowAnonymous]
        //public async Task<IActionResult> GetFollowerById(Guid startupId, Guid userId)
        //{
        //    var follower = await _followerService.GetFollowerByIdAsync(userId, startupId);
        //    if (follower == null)
        //        return NotFound(new { message = "Follower not found" });

        //    return Ok(follower);
        //}

        
        [HttpGet("my-followed")]
        public async Task<IActionResult> GetMyFollowedStartups([FromQuery] SieveModel model)
        {
            try
            {
                var result = await _followerService.GetMyFollowedStartupsAsync(model);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        
        //[HttpGet("{startupId:guid}/followers")]
        //[AllowAnonymous]
        //public async Task<IActionResult> GetStartupFollowers(Guid startupId, [FromQuery] SieveModel model)
        //{
        //    var result = await _followerService.GetStartupFollowersAsync(startupId, model);
        //    return Ok(result);
        //}
    }
}
