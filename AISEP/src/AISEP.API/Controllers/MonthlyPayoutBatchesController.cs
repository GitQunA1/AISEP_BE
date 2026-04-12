using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.Helpers;
using AISEP.BLL.Services.MonthlyPayouts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;

namespace AISEP.API.Controllers
{
    [ApiController]
    [Route("api/monthly-payout-batches")]
    public class MonthlyPayoutBatchesController : ControllerBase
    {
        private readonly IMonthlyPayoutBatchService _monthlyPayoutBatchService;

        public MonthlyPayoutBatchesController(IMonthlyPayoutBatchService monthlyPayoutBatchService)
        {
            _monthlyPayoutBatchService = monthlyPayoutBatchService;
        }

        [HttpPost("generate")]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> Generate([FromBody] GenerateMonthlyPayoutRequest request)
        {
            var result = await _monthlyPayoutBatchService.GenerateAsync(request);
            return Ok(ApiResponse<object>.SuccessResponse(result, "Monthly payout batch generated successfully."));
        }

        [HttpGet]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> GetBatches([FromQuery] SieveModel model)
        {
            var result = await _monthlyPayoutBatchService.GetBatchesAsync(model);
            return Ok(ApiResponse<object>.SuccessResponse(result, "Monthly payout batches retrieved successfully."));
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> GetBatchById(int id)
        {
            var result = await _monthlyPayoutBatchService.GetBatchByIdAsync(id);
            if (result is null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse("Monthly payout batch not found.", "Not Found", 404));
            }

            return Ok(ApiResponse<object>.SuccessResponse(result, "Success"));
        }

        [HttpGet("{id:int}/items")]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> GetItemsByBatchId(int id, [FromQuery] SieveModel model)
        {
            var result = await _monthlyPayoutBatchService.GetItemsByBatchIdAsync(id, model);
            return Ok(ApiResponse<object>.SuccessResponse(result, "Monthly payout items retrieved successfully."));
        }
    }
}
