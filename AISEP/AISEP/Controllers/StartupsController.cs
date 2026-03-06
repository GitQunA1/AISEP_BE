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
        public async Task<IActionResult> Search([FromQuery] SieveModel model, [FromQuery] string? industry = null, [FromQuery] DevelopmentStage? stage = null)
        {
            var result = await _startupService.SearchStartupsAsync(model, industry, stage);
            return Ok(ApiResponse<object>.SuccessResponse(result, "Success"));
        }

        [HttpGet("by-status")]
        public async Task<IActionResult> GetByStatus([FromQuery] SieveModel model, [FromQuery] ApprovalStatus? status = null)
        {
            var result = await _startupService.GetStartupsByStatusAsync(model, status);
            return Ok(ApiResponse<object>.SuccessResponse(result, "Success"));
        }

        // Startup user

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateStartupDto dto)
        {
            var userId = _currentUserService.GetUserId();
            var data = await _startupService.CreateStartupAsync(userId, dto);
            return CreatedAtAction(nameof(GetById), new { id = data.Id }, ApiResponse<object>.SuccessResponse(data, "Startup created successfully", 201));
        }

        //[HttpGet("me")]
        //public async Task<IActionResult> GetMyProfile()
        //{
        //    var userId = _currentUserService.GetUserId();
        //    var startup = await _startupService.GetMyProfileAsync(userId);
        //    if (startup is null)
        //        return NotFound(ApiResponse.Fail("Startup profile not found."));
        //    return Ok(ApiResponse.Success(startup));
        //}

        [HttpPut("submit")]
        public async Task<IActionResult> ApproveStartup()
        {
            var userId = _currentUserService.GetUserId();
            await _startupService.ApproveStartupAsync(userId);
            return Ok(ApiResponse<object>.SuccessResponse(null!, "Approving Startup Successfully."));
        }

        //// Staff / Admin

        //[HttpGet("pending")]
        //public async Task<IActionResult> GetPending([FromQuery] SieveModel model)
        //{
        //    var result = await _startupService.GetPendingStartupsAsync(model);
        //    return Ok(ApiResponse.Success(result));
        //}

        //[HttpPut("{id:int}/review")]
        //public async Task<IActionResult> Review(int id, [FromBody] ReviewStartupDto dto)
        //{
        //    await _startupService.ReviewStartupAsync(id, dto);
        //    return Ok(ApiResponse.Success("Startup reviewed successfully."));
        //}
    }
}
