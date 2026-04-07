using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.Helpers;
using AISEP.BLL.Services.SystemCommissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;

namespace AISEP.API.Controllers
{
    [ApiController]
    [Route("api/system-commissions")]
    public class SystemCommissionController : ControllerBase
    {
        private readonly ISystemCommissionService _systemCommissionService;

        public SystemCommissionController(ISystemCommissionService systemCommissionService)
        {
            _systemCommissionService = systemCommissionService;
        }

        [HttpGet("current")]
        [Authorize]
        public async Task<IActionResult> GetCurrent()
        {
            var result = await _systemCommissionService.GetCurrentAsync();
            return Ok(ApiResponse<object>.SuccessResponse(result, "Current system commission retrieved successfully."));
        }

        [HttpPut("current")]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> UpdateCurrent([FromBody] UpdateSystemCommissionRequest request)
        {
            try
            {
                var result = await _systemCommissionService.UpdateCurrentAsync(request);
                return Ok(ApiResponse<object>.SuccessResponse(result, "System commission updated successfully."));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ApiResponse<object>.ErrorResponse(ex.Message, "Conflict", 409));
            }
        }

        [HttpGet("history")]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> GetHistory([FromQuery] SieveModel model)
        {
            var result = await _systemCommissionService.GetHistoryAsync(model);
            return Ok(ApiResponse<object>.SuccessResponse(result, "System commission history retrieved successfully."));
        }
    }
}
