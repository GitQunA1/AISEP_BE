using AISEP.Common;
using AISEP.DTOs;
using AISEP.Models.Enums;
using AISEP.Services.Startups;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;

namespace AISEP.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StartupsController : ControllerBase
    {
        private readonly IStartupService _startupService;

        public StartupsController(IStartupService startupService)
        {
            _startupService = startupService;
        }

       
        [HttpGet("search")]
        public async Task<IActionResult> SearchStartups(
            [FromQuery] SieveModel model,
            [FromQuery] string? industry = null,
            [FromQuery] DevelopmentStage? stage = null)
        {
            var result = await _startupService.SearchStartupsAsync(model, industry, stage);
            return Ok(ApiResponse.Success(result));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetStartupById(int id)
        {
            var startup = await _startupService.GetStartupByIdAsync(id);
            if (startup is null)
                return NotFound(ApiResponse.Fail("Startup not found."));

            return Ok(ApiResponse.Success(startup));
        }

        [HttpGet]
        public async Task<IActionResult> GetAllStartups([FromQuery] SieveModel model)
        {
            var result = await _startupService.GetAllStartupsAsync(model);
            return Ok(ApiResponse.Success(result));
        }
    }
}
