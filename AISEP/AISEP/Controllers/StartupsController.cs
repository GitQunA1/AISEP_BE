using AISEP.Common;
using AISEP.DTOs.Requests;
using AISEP.Services.Users;
using AISEP.Services.Startups;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;

namespace AISEP.Controllers
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
        public async Task<IActionResult> GetById(int id)
        {
            var startup = await _startupService.GetStartupByIdAsync(id);
            if (startup is null)
                return NotFound(ApiResponse<object>.ErrorResponse("Startup not found.", "Not found", 404));
            return Ok(ApiResponse<object>.SuccessResponse(startup, "Success"));
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] SieveModel model, [FromQuery] string? industry = null, [FromQuery] string? stage = null)
        {
            var result = await _startupService.SearchStartupsAsync(model, industry, stage);
            return Ok(ApiResponse<object>.SuccessResponse(result, "Success"));
        }

        [HttpGet("by-status")]
        public async Task<IActionResult> GetByStatus([FromQuery] SieveModel model, [FromQuery] string? status = null)
        {
            var result = await _startupService.GetStartupsByStatusAsync(model, status);
            return Ok(ApiResponse<object>.SuccessResponse(result, "Success"));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateStartupRequest dto)
        {
            var userId = _currentUserService.GetUserId();
            var data = await _startupService.CreateStartupAsync(userId, dto);
            return CreatedAtAction(nameof(GetById), new { id = data.Id }, ApiResponse<object>.SuccessResponse(data, "Startup created successfully", 201));
        }

        [HttpPut("submit")]
        public async Task<IActionResult> ApproveStartup()
        {
            var userId = _currentUserService.GetUserId();
            await _startupService.ApproveStartupAsync(userId);
            return Ok(ApiResponse<object>.SuccessResponse(null!, "Approving Startup Successfully."));
        }
        [HttpPut("reject")]
        public async Task<IActionResult> RejectStartup([FromBody] RejectStartupRequest dto)
        {
            var userId = _currentUserService.GetUserId();
            await _startupService.RejectStartupAsync(userId, dto);
            return Ok(ApiResponse<object>.SuccessResponse(null!, "Startup rejected successfully."));
        }
    }
}
