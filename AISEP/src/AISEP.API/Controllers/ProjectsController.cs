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
            try
            {
                var project = await _projectService.GetProjectByIdAsync(id);
                return Ok(ApiResponse<object>.SuccessResponse(project, "Success"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(ex.Message, "Not found", 404));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message, "Internal Server Error", 500));
            }
        }

        [HttpGet("my")]
        [Authorize(Roles = "Startup")]
        public async Task<IActionResult> GetMyProjects([FromQuery] SieveModel model)
        {
            try
            {
                var result = await _projectService.GetMyProjectsAsync(model);
                return Ok(ApiResponse<object>.SuccessResponse(result, "Success"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(ex.Message, "Not found", 404));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message, "Internal Server Error", 500));
            }
        }

        //[HttpGet("drafts")]
        //[Authorize]
        //public async Task<IActionResult> GetDraftProjects([FromQuery] SieveModel model)
        //{
        //    var result = await _projectService.GetDraftProjectsAsync(model);
        //    return Ok(ApiResponse<object>.SuccessResponse(result, "Success"));
        //}

        [HttpPost]
        [Authorize(Roles = "Startup")]
        public async Task<IActionResult> Create([FromForm] CreateProjectRequest dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Invalid input data.", "Bad Request", 400));
            }
            try { 
            var data = await _projectService.CreateProjectAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = data.ProjectId },
                ApiResponse<object>.SuccessResponse(data, "Project created successfully", 201));
                }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(ex.Message, "Not Found", 404));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ApiResponse<object>.ErrorResponse(ex.Message, "Conflict", 409));
            }
           
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message, "Internal Server Error", 500));
            }


        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Startup")]
        public async Task<IActionResult> Update(int id, [FromForm] UpdateProjectRequest dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Invalid input data.", "Bad Request", 400));
            }
            try
            {
                var data = await _projectService.UpdateProjectAsync(id, dto);
            return Ok(ApiResponse<object>.SuccessResponse(data, "Project updated successfully."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(ex.Message, "Not Found", 404));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ApiResponse<object>.ErrorResponse(ex.Message, "Conflict", 409));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message, "Internal Server Error", 500));
            }
        }


        [HttpPatch("{id:int}/approve")]
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> Approve(int id)
        {
           
            try
            {
                await _projectService.ApproveProjectAsync(id);
                return Ok(ApiResponse<object>.SuccessResponse(null, "Project approved successfully."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(ex.Message, "Not Found", 404));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ApiResponse<object>.ErrorResponse(ex.Message, "Conflict", 409));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message, "Internal Server Error", 500));
            }
        }

        [HttpPatch("{id:int}/reject")]
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> Reject(int id, [FromBody] RejectProjectRequest dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Invalid input data.", "Bad Request", 400));
            }
            try
            {
                await _projectService.RejectProjectAsync(id, dto);
                return Ok(ApiResponse<object>.SuccessResponse(null, "Project rejected successfully."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(ex.Message, "Not Found", 404));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ApiResponse<object>.ErrorResponse(ex.Message, "Conflict", 409));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message, "Internal Server Error", 500));
            }
        }
    }
}
