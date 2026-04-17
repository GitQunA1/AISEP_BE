using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.Helpers;
using AISEP.BLL.Services.Payouts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;

namespace AISEP.API.Controllers
{
    [ApiController]
    [Route("api/payout-groups")]
    public class PayoutGroupsController : ControllerBase
    {
        private readonly IPayoutGroupService _payoutGroupService;

        public PayoutGroupsController(IPayoutGroupService payoutGroupService)
        {
            _payoutGroupService = payoutGroupService;
        }

        [HttpPost("generate")]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> Generate([FromBody] GeneratePayoutGroupRequest request)
        {
            var result = await _payoutGroupService.GenerateAsync(request);
            return Ok(ApiResponse<object>.SuccessResponse(result, "Monthly payout batch generated successfully."));
        }

        [HttpGet]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> GetBatches([FromQuery] SieveModel model)
        {
            var result = await _payoutGroupService.GetBatchesAsync(model);
            return Ok(ApiResponse<object>.SuccessResponse(result, "Monthly payout batches retrieved successfully."));
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> GetBatchById(int id)
        {
            var result = await _payoutGroupService.GetBatchByIdAsync(id);
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
            var result = await _payoutGroupService.GetItemsByBatchIdAsync(id, model);
            return Ok(ApiResponse<object>.SuccessResponse(result, "Monthly payout items retrieved successfully."));
        }
    }
}



