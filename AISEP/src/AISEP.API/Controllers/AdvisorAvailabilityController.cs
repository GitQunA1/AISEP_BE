using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.Helpers;
using AISEP.BLL.Services.AdvisorAvailabilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;

namespace AISEP.API.Controllers
{
    [ApiController]
    [Route("api/advisor-availabilities")]
    public class AdvisorAvailabilityController : ControllerBase
    {
        private readonly IAdvisorAvailabilityService _advisorAvailabilityService;

        public AdvisorAvailabilityController(IAdvisorAvailabilityService advisorAvailabilityService)
        {
            _advisorAvailabilityService = advisorAvailabilityService;
        }

        [HttpGet("advisor/{advisorId:int}")]
        public async Task<IActionResult> GetByAdvisorId(int advisorId, [FromQuery] SieveModel model)
        {
            var result = await _advisorAvailabilityService.GetByAdvisorIdAsync(advisorId, model);
            return Ok(ApiResponse<object>.SuccessResponse(result, "Success"));
        }

        [HttpGet("me")]
        [Authorize(Roles = "Advisor,Staff,Admin")]
        public async Task<IActionResult> GetMyAvailabilities([FromQuery] SieveModel model)
        {
            var result = await _advisorAvailabilityService.GetMyAvailabilitiesAsync(model);
            return Ok(ApiResponse<object>.SuccessResponse(result, "Success"));
        }

        [HttpPost("me")]
        [Authorize(Roles = "Advisor")]
        public async Task<IActionResult> CreateMyAvailability([FromBody] CreateAdvisorAvailabilityRequest request)
        {
            var result = await _advisorAvailabilityService.CreateMyAvailabilityAsync(request);
            return Ok(ApiResponse<object>.SuccessResponse(result, "Advisor availability slots created successfully."));
        }

        [HttpPut("me/{availabilityId:int}")]
        [Authorize(Roles = "Advisor")]
        public async Task<IActionResult> UpdateMyAvailability(int availabilityId, [FromBody] UpdateAdvisorAvailabilityRequest request)
        {
            var result = await _advisorAvailabilityService.UpdateMyAvailabilityAsync(availabilityId, request);
            return Ok(ApiResponse<object>.SuccessResponse(result, "Advisor availability updated successfully."));
        }

        [HttpDelete("me/{availabilityId:int}")]
        [Authorize(Roles = "Advisor")]
        public async Task<IActionResult> DeleteMyAvailability(int availabilityId)
        {
            var deleted = await _advisorAvailabilityService.DeleteMyAvailabilityAsync(availabilityId);
            if (!deleted)
                return NotFound(ApiResponse<object>.ErrorResponse("Availability slot not found.", "Not found", 404));

            return Ok(ApiResponse<object>.SuccessResponse(null, "Advisor availability deleted successfully."));
        }
    }
}
