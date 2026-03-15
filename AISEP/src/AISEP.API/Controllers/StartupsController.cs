using AISEP.BLL.Helpers;
using AISEP.BLL.DTOs.Requests;
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
            var startup = await _startupService.GetStartupByIdAsync(id);
            if (startup is null)
                return NotFound(ApiResponse<object>.ErrorResponse("Startup not found.", "Not found", 404));
            return Ok(ApiResponse<object>.SuccessResponse(startup, "Success"));
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
            
            var data = await _startupService.CreateStartupAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = data.Id }, ApiResponse<object>.SuccessResponse(data, "Startup created successfully", 201));
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Startup")]
        public async Task<IActionResult> Update(int id,[FromForm] UpdateStartupRequest dto)
        {
            var data = await _startupService.UpdateStartupAsync(id,dto);
            return Ok(ApiResponse<object>.SuccessResponse(data, "Startup updated successfully."));
        }

        [HttpPatch("{startupId:int}/approve")]
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> ApproveStartup(int startupId)
        {
            await _startupService.ApproveStartupAsync(startupId);
            return Ok(ApiResponse<object>.SuccessResponse(null!, "Startup approved successfully."));
        }

        [HttpPatch("{startupId:int}/reject")]
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> RejectStartup(int startupId, [FromBody] RejectStartupRequest dto)
        {
            await _startupService.RejectStartupAsync(startupId, dto);
            return Ok(ApiResponse<object>.SuccessResponse(null!, "Startup rejected successfully."));
        }
    }
}
