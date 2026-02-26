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

        /// <summary>
        /// Search startups by industry and/or development stage with pagination
        /// </summary>
        [HttpGet("search")]
        public async Task<IActionResult> SearchStartups(
            [FromQuery] SieveModel model,
            [FromQuery] string? industry = null,
            [FromQuery] DevelopmentStage? stage = null
            )
        {
            var result = await _startupService.SearchStartupsAsync(model, industry, stage);
            return Ok(new { success = true, data = result });
        }

        /// <summary>
        /// Get startup by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetStartupById(Guid id)
        {
            var startup = await _startupService.GetStartupByIdAsync(id);
            if (startup == null)
                return NotFound(new { success = false, message = "Startup not found" });

            return Ok(new { success = true, data = startup });
        }

        /// <summary>
        /// Get all startups with pagination and filtering (using Sieve)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllStartups([FromQuery] SieveModel model)
        {
            var result = await _startupService.GetAllStartupsAsync(model);
            return Ok(new { success = true, data = result });
        }
    }
}
