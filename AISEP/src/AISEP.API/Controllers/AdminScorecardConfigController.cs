using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.Helpers;
using AISEP.BLL.Services.ScorecardConfigs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AISEP.API.Controllers
{
    [ApiController]
    [Route("api/admin/scorecard-configs")]
    [Authorize(Roles = "Admin")]
    public class AdminScorecardConfigController : ControllerBase
    {
        private readonly IScorecardConfigService _scorecardConfigService;

        public AdminScorecardConfigController(IScorecardConfigService scorecardConfigService)
        {
            _scorecardConfigService = scorecardConfigService;
        }

        [HttpGet("default")]
        public async Task<IActionResult> GetDefault()
        {
            try
            {
                var result = await _scorecardConfigService.GetDefaultConfigAsync();
                return Ok(ApiResponse<object>.SuccessResponse(result, "Scorecard config retrieved successfully."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(ex.Message, "Not Found", 404));
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateScorecardWeightRequest request)
        {
            try
            {
                var result = await _scorecardConfigService.UpdateConfigAsync(id, request);
                return Ok(ApiResponse<object>.SuccessResponse(result, "Scorecard config updated successfully."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(ex.Message, "Not Found", 404));
            }
        }
    }
}
