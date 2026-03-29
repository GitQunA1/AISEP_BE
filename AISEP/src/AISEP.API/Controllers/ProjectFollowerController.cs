using AISEP.BLL.Helpers;
using AISEP.BLL.Services.ProjectFollowers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;

namespace AISEP.API.Controllers
{
    [ApiController]
    [Route("api/projects")]
    [Authorize(Roles = "Startup,Investor")]
    public class ProjectFollowerController : ControllerBase
    {
        private readonly IProjectFollowerService _followerService;

        public ProjectFollowerController(IProjectFollowerService followerService)
        {
            _followerService = followerService;
        }

        [HttpPost("{projectId:int}/follow")]
        public async Task<IActionResult> FollowProject(int projectId)
        {
            var result = await _followerService.FollowProjectAsync(projectId);
            if (!result)
            {
                return Conflict(ApiResponse<object>.ErrorResponse("You already follow this project.", "Conflict", 409));
            }

            return Ok(ApiResponse<object>.SuccessResponse(null!, "Followed successfully"));
        }

        [HttpDelete("{projectId:int}/follow")]
        public async Task<IActionResult> UnfollowProject(int projectId)
        {
            var result = await _followerService.UnfollowProjectAsync(projectId);
            if (!result)
            {
                return NotFound(ApiResponse<object>.ErrorResponse("You are not following this project.", "Not found", 404));
            }

            return Ok(ApiResponse<object>.SuccessResponse(null!, "Unfollowed successfully"));
        }

        [HttpGet("my-followed")]
        public async Task<IActionResult> GetMyFollowedProjects([FromQuery] SieveModel model)
        {
            var result = await _followerService.GetMyFollowedProjectsAsync(model);
            return Ok(ApiResponse<object>.SuccessResponse(result, "Success"));
        }
    }
}
