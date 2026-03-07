using AISEP.Common;
using AISEP.DTOs.Requests;
using AISEP.Services.Users;
using AISEP.Services.Projects;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;

namespace AISEP.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectsController : ControllerBase
    {
        private readonly IProjectService _projectService;
        private readonly IUserService _currentUserService;

        public ProjectsController(IProjectService projectService, IUserService currentUserService)
        {
            _projectService = projectService;
            _currentUserService = currentUserService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] SieveModel model)
        {
            var result = await _projectService.GetAllProjectsAsync(model);
            return Ok(ApiResponse<object>.SuccessResponse(result, "Success"));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var project = await _projectService.GetProjectByIdAsync(id);
            if (project is null)
                return NotFound(ApiResponse<object>.ErrorResponse("Project not found.", "Not found", 404));
            return Ok(ApiResponse<object>.SuccessResponse(project, "Success"));
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMyProjects([FromQuery] SieveModel model)
        {
            var userId = _currentUserService.GetUserId();
            var result = await _projectService.GetMyProjectsAsync(userId, model);
            return Ok(ApiResponse<object>.SuccessResponse(result, "Success"));
        }

        [HttpGet("drafts")]
        public async Task<IActionResult> GetDraftProjects([FromQuery] SieveModel model)
        {
            var result = await _projectService.GetDraftProjectsAsync(model);
            return Ok(ApiResponse<object>.SuccessResponse(result, "Success"));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProjectRequest dto)
        {
            var userId = _currentUserService.GetUserId();
            var data   = await _projectService.CreateProjectAsync(userId, dto);
            return CreatedAtAction(nameof(GetById), new { id = data.ProjectId },
                ApiResponse<object>.SuccessResponse(data, "Project created successfully", 201));
        }

      
        [HttpPut("{id:int}/approve")]
        public async Task<IActionResult> Approve(int id, [FromBody] ApproveProjectRequest dto)
        {
            await _projectService.ApproveProjectAsync(id, dto);
            return Ok(ApiResponse<object>.SuccessResponse(null, "Project approved successfully."));
        }

        [HttpPut("{id:int}/reject")]
        public async Task<IActionResult> Reject(int id, [FromBody] RejectProjectRequest dto)
        {
            await _projectService.RejectProjectAsync(id, dto);
            return Ok(ApiResponse<object>.SuccessResponse(null, "Project rejected successfully."));
        }
    }
}
