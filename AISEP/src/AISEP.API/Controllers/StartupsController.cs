using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.Exceptions;
using AISEP.BLL.Helpers;
using AISEP.BLL.Services.Startups;
using AISEP.BLL.Services.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;

namespace AISEP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    
    public class StartupsController : ControllerBase
    {
        private readonly IStartupService _startupService;
        private readonly IUserService _currentUserService;

        public StartupsController(IStartupService startupService, IUserService currentUserService)
        {
            _startupService = startupService;
            _currentUserService = currentUserService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] SieveModel model)
        {
            var result = await _startupService.GetAllStartupsAsync(model);
            return Ok(ApiResponse<object>.SuccessResponse(result, "Success"));
        }

        [HttpGet("{id:int}")]
        [Authorize]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var startup = await _startupService.GetStartupByIdAsync(id);
                return Ok(ApiResponse<object>.SuccessResponse(startup, "Success"));
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

        [HttpGet("me")]
        [Authorize(Roles = "Startup")]
        public async Task<IActionResult> GetMyProfile()
        {
            try
            {
                var startup = await _startupService.GetMyProfileAsync();
                return Ok(ApiResponse<object>.SuccessResponse(startup, "Success"));
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

        [HttpGet("search")]
        [Authorize]
        public async Task<IActionResult> Search([FromQuery] SieveModel model, [FromQuery] string? industry = null, [FromQuery] string? stage = null)
        {
            var result = await _startupService.SearchStartupsAsync(model, industry, stage);
            return Ok(ApiResponse<object>.SuccessResponse(result, "Success"));
        }

        [HttpGet("by-status")]
        [Authorize]
        public async Task<IActionResult> GetByStatus([FromQuery] SieveModel model, [FromQuery] string? status = null)
        {
            var result = await _startupService.GetStartupsByStatusAsync(model, status);
            return Ok(ApiResponse<object>.SuccessResponse(result, "Success"));
        }

        [HttpPost]
        [Authorize(Roles = "Startup")]

        public async Task<IActionResult> Create([FromForm] CreateStartupRequest dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Invalid input data.", "Bad Request", 400));
            }
            try
            {
                var data = await _startupService.CreateStartupAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = data.Id },
                    ApiResponse<object>.SuccessResponse(data, "Startup created successfully", 201));
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
        public async Task<IActionResult> Update(int id,[FromForm] UpdateStartupRequest dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Invalid input data.", "Bad Request", 400));
            }
            try
            {
                var data = await _startupService.UpdateStartupAsync(id, dto);
                return Ok(ApiResponse<object>.SuccessResponse(data, "Startup updated successfully."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(ex.Message, "Not Found", 404));
            }
            catch(ForbiddenAccessException ex)
            {
                return StatusCode(403,ApiResponse<object>.ErrorResponse(ex.Message, "Forbidden", 403));
            }
        }

        [HttpPatch("{id:int}/approve")]
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> Approve(int id)
        {
            try
            {
                await _startupService.ApproveStartupAsync(id);
                return Ok(ApiResponse<object>.SuccessResponse(null, "Startup approved successfully."));
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
        public async Task<IActionResult> Reject(int id, [FromBody] RejectStartupRequest dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Invalid input data.", "Bad Request", 400));
            }
            try
            {
                await _startupService.RejectStartupAsync(id, dto);
                return Ok(ApiResponse<object>.SuccessResponse(null, "Startup rejected successfully."));
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
