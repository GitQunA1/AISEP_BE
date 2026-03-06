using AISEP.Common;
using AISEP.DTOs;
using AISEP.Models.Enums;
using AISEP.Services.CurrentUser;
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
        private readonly ICurrentUserService _currentUserService;

        public StartupsController(IStartupService startupService, ICurrentUserService currentUserService)
        {
            _startupService = startupService;
            _currentUserService = currentUserService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] SieveModel model)
        {
            var result = await _startupService.GetAllStartupsAsync(model);
            return Ok(ApiResponse.Success(result));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var startup = await _startupService.GetStartupByIdAsync(id);
            if (startup is null)
                return NotFound(ApiResponse.Fail("Startup not found."));
            return Ok(ApiResponse.Success(startup));
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] SieveModel model, [FromQuery] string? industry = null, [FromQuery] DevelopmentStage? stage = null)
        {
            var result = await _startupService.SearchStartupsAsync(model, industry, stage);
            return Ok(ApiResponse.Success(result));
        }

        [HttpGet("by-status")]
        public async Task<IActionResult> GetByStatus([FromQuery] SieveModel model, [FromQuery] ApprovalStatus? status = null)
        {
            var result = await _startupService.GetStartupsByStatusAsync(model, status);
            return Ok(ApiResponse.Success(result));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateStartupDto dto)
        {
            var userId = _currentUserService.GetUserId();
            var data = await _startupService.CreateStartupAsync(userId, dto);
            return CreatedAtAction(nameof(GetById), new { id = data.Id }, ApiResponse.Success(data));
        }

        [HttpPut("submit")]
        public async Task<IActionResult> ApproveStartup()
        {
            var userId = _currentUserService.GetUserId();
            await _startupService.ApproveStartupAsync(userId);
            return Ok(ApiResponse.Success("Approving Startup Successfully."));
        }
    }
}
