using AISEP.BLL.Helpers;
using AISEP.BLL.Services.ProjectAdvisorAssignments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;

namespace AISEP.API.Controllers
{
    [ApiController]
    [Route("api/project-advisor-assignments")]
    [Authorize]
    public class ProjectAdvisorAssignmentsController : ControllerBase
    {
        private readonly IProjectAdvisorAssignmentService _assignmentService;

        public ProjectAdvisorAssignmentsController(IProjectAdvisorAssignmentService assignmentService)
        {
            _assignmentService = assignmentService;
        }

        [HttpGet("project/{projectId:int}")]
        public async Task<IActionResult> GetAssignedAdvisorsByProject(int projectId)
        {
            try
            {
                var advisors = await _assignmentService.GetAssignedAdvisorsByProjectAsync(projectId);
                if (advisors.Count == 0)
                {
                    return Ok(ApiResponse<object>.SuccessResponse(advisors, "Project has not been assigned to any advisor yet."));
                }

                return Ok(ApiResponse<object>.SuccessResponse(advisors, "Success"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(ex.Message, "Not found", 404));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message, "Bad request", 400));
            }
        }

        [HttpGet("me/projects")]
        [Authorize(Roles = "Advisor,Staff,Admin")]
        public async Task<IActionResult> GetAssignedProjectsForCurrentAdvisor([FromQuery] SieveModel model)
        {
            try
            {
                var result = await _assignmentService.GetAssignedProjectsForCurrentAdvisorAsync(model);
                return Ok(ApiResponse<object>.SuccessResponse(result, "Success"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(ex.Message, "Not found", 404));
            }
        }
    }
}
