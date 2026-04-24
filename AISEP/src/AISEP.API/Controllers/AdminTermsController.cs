using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.Helpers;
using AISEP.BLL.Services.SystemTerms;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;

namespace AISEP.API.Controllers
{
    [ApiController]
    [Route("api/admin/terms")]
    [Authorize(Roles = "Staff,Admin")]
    public class AdminTermsController : ControllerBase
    {
        private readonly ISystemTermService _systemTermService;

        public AdminTermsController(ISystemTermService systemTermService)
        {
            _systemTermService = systemTermService;
        }

        [HttpPost]
        public async Task<IActionResult> Publish([FromBody] CreateSystemTermRequest request)
        {
            var result = await _systemTermService.PublishAsync(request);
            return Ok(ApiResponse<object>.SuccessResponse(result, "System terms published successfully."));
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetHistory([FromQuery] SieveModel model)
        {
            var result = await _systemTermService.GetHistoryAsync(model);
            return Ok(ApiResponse<object>.SuccessResponse(result, "System terms history retrieved successfully."));
        }
    }
}
