using AISEP.Common;
using AISEP.DTOs;
using AISEP.Services.CurrentUser;
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
        private readonly ICurrentUserService _currentUserService;

        public ProjectsController(IProjectService projectService, ICurrentUserService currentUserService)
        {
            _projectService = projectService;
            _currentUserService = currentUserService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] SieveModel model)
        {
            var result = await _projectService.GetAllProjectsAsync(model);
            return Ok(ApiResponse.Success(result));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var project = await _projectService.GetProjectByIdAsync(id);
            if (project is null)
                return NotFound(ApiResponse.Fail("Project not found."));
            return Ok(ApiResponse.Success(project));
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMyProjects([FromQuery] SieveModel model)
        {
            var userId = _currentUserService.GetUserId();
            var result = await _projectService.GetMyProjectsAsync(userId, model);
            return Ok(ApiResponse.Success(result));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProjectDto dto)
        {
            var userId = _currentUserService.GetUserId();
            var data   = await _projectService.CreateProjectAsync(userId, dto);
            return CreatedAtAction(nameof(GetById), new { id = data.ProjectId }, ApiResponse.Success(data));
        }

        [HttpPut("{id:int}/submit")]
        public async Task<IActionResult> Submit(int id)
        {
            var userId = _currentUserService.GetUserId();
            await _projectService.SubmitProjectAsync(id, userId);
            return Ok(ApiResponse.Success("Project submitted for review successfully."));
        }

        [HttpPut("{id:int}/approve")]
        public async Task<IActionResult> Approve(int id, [FromBody] ReviewProjectDto dto)
        {
            await _projectService.ApproveProjectAsync(id, dto);
            return Ok(ApiResponse.Success("Project approved successfully."));
        }

        [HttpPut("{id:int}/reject")]
        public async Task<IActionResult> Reject(int id, [FromBody] RejectProjectDto dto)
        {
            await _projectService.RejectProjectAsync(id, dto);
            return Ok(ApiResponse.Success("Project rejected successfully."));
        }
    }
}
