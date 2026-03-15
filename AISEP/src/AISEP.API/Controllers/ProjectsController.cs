using AISEP.BLL.Helpers;
using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.Services.Projects;
using AISEP.BLL.Services.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;

namespace AISEP.API.Controllers
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
        [Authorize]
        public async Task<IActionResult> GetById(int id)
        {
            var project = await _projectService.GetProjectByIdAsync(id);
            if (project is null)
                return NotFound(ApiResponse<object>.ErrorResponse("Project not found.", "Not found", 404));
            return Ok(ApiResponse<object>.SuccessResponse(project, "Success"));
        }

        [HttpGet("my")]
        [Authorize(Roles = "Startup")]
        public async Task<IActionResult> GetMyProjects([FromQuery] SieveModel model)
        {
            var userId = _currentUserService.GetUserId();
            var result = await _projectService.GetMyProjectsAsync(userId, model);
            return Ok(ApiResponse<object>.SuccessResponse(result, "Success"));
        }

        [HttpGet("drafts")]
        [Authorize]
        public async Task<IActionResult> GetDraftProjects([FromQuery] SieveModel model)
        {
            var result = await _projectService.GetDraftProjectsAsync(model);
            return Ok(ApiResponse<object>.SuccessResponse(result, "Success"));
        }

        [HttpPost]
        [Authorize(Roles ="Startup")]
        public async Task<IActionResult> Create([FromForm] CreateProjectRequest dto)
        {
            //var userId = _currentUserService.GetUserId();
            var data   = await _projectService.CreateProjectAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = data.ProjectId },
                ApiResponse<object>.SuccessResponse(data, "Project created successfully", 201));
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Startup")]
        public async Task<IActionResult> Update(int id, [FromForm] UpdateProjectRequest dto)
        {
            var data = await _projectService.UpdateProjectAsync(id, dto);
            return Ok(ApiResponse<object>.SuccessResponse(data, "Project updated successfully."));
        }


        [HttpPatch("{id:int}/approve")]
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> Approve(int id)
        {
            await _projectService.ApproveProjectAsync(id);
            return Ok(ApiResponse<object>.SuccessResponse(null, "Project approved successfully."));
        }

        [HttpPatch("{id:int}/reject")]
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> Reject(int id, [FromBody] RejectProjectRequest dto)
        {
            await _projectService.RejectProjectAsync(id, dto);
            return Ok(ApiResponse<object>.SuccessResponse(null, "Project rejected successfully."));
        }
    }
}
