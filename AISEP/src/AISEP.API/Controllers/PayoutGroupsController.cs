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
            return Ok(ApiResponse<object>.SuccessResponse(result, "payout groups generated successfully."));
        }

        [HttpGet]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> GetGroups([FromQuery] SieveModel model)
        {
            var result = await _payoutGroupService.GetGroupsAsync(model);
            return Ok(ApiResponse<object>.SuccessResponse(result, "payout groups retrieved successfully."));
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> GetGroupById(int id)
        {
            var result = await _payoutGroupService.GetGroupByIdAsync(id);
            if (result is null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse("payout group not found.", "Not Found", 404));
            }

            return Ok(ApiResponse<object>.SuccessResponse(result, "payout group retrieved successfully."));
        }

        [HttpGet("{id:int}/items")]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> GetItemsByGroupId(int id, [FromQuery] SieveModel model)
        {
            var result = await _payoutGroupService.GetItemsByGroupIdAsync(id, model);
            return Ok(ApiResponse<object>.SuccessResponse(result, "payout items retrieved successfully."));
        }
    }
}



