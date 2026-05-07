using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.Helpers;
using AISEP.BLL.Services.Users;
using AISEP.BLL.Services.Payouts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;

namespace AISEP.API.Controllers
{
    [ApiController]
    [Route("api/payouts")]
    public class PayoutController : ControllerBase
    {
        private readonly IPayoutService _payoutService;
        private readonly IUserService _userService;

        public PayoutController(IPayoutService payoutService, IUserService userService)
        {
            _payoutService = payoutService;
            _userService = userService;
        }

        [HttpPatch("{id:int}/mark-paid")]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> MarkPaid(int id, [FromForm] MarkPayoutPaidRequest request)
        {
            var staffId = _userService.GetUserId();
            var result = await _payoutService.MarkPaidAsync(id, staffId, request);
            return Ok(ApiResponse<object>.SuccessResponse(result, "payout marked as paid."));
        }

        [HttpPatch("{id:int}/reject")]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> Reject(int id, [FromBody] RejectPayoutRequest request)
        {
            var staffId = _userService.GetUserId();
            var result = await _payoutService.RejectAsync(id, staffId, request);
            return Ok(ApiResponse<object>.SuccessResponse(result, "payout rejected successfully."));
        }

        [HttpPatch("{id:int}/request-retry")]
        [Authorize(Roles = "Advisor")]
        public async Task<IActionResult> RequestRetry(int id, [FromBody] RequestPayoutRetryRequest request)
        {
            var advisorUserId = _userService.GetUserId();
            var result = await _payoutService.RequestRetryAsync(id, advisorUserId, request);
            return Ok(ApiResponse<object>.SuccessResponse(result, "Retry request submitted successfully."));
        }

        [HttpGet]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> GetAll([FromQuery] SieveModel model)
        {
            var result = await _payoutService.GetAllAsync(model);
            return Ok(ApiResponse<object>.SuccessResponse(result, "payouts retrieved successfully."));
        }

        [HttpGet("me")]
        [Authorize(Roles = "Advisor")]
        public async Task<IActionResult> GetMine([FromQuery] SieveModel model)
        {
            var userId = _userService.GetUserId();
            var result = await _payoutService.GetMineAsync(userId, model);
            return Ok(ApiResponse<object>.SuccessResponse(result, "My payouts retrieved successfully."));
        }

    }
}



