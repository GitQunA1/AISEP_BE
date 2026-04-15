using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.Helpers;
using AISEP.BLL.Services.Users;
using AISEP.BLL.Services.MonthlyPayouts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;

namespace AISEP.API.Controllers
{
    [ApiController]
    [Route("api/monthly-payouts")]
    public class MonthlyPayoutController : ControllerBase
    {
        private readonly IMonthlyPayoutService _monthlyPayoutService;
        private readonly IUserService _userService;

        public MonthlyPayoutController(IMonthlyPayoutService monthlyPayoutService, IUserService userService)
        {
            _monthlyPayoutService = monthlyPayoutService;
            _userService = userService;
        }

        [HttpPatch("{id:int}/mark-paid")]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> MarkPaid(int id, [FromBody] MarkMonthlyPayoutPaidRequest request)
        {
            var staffId = _userService.GetUserId();
            var result = await _monthlyPayoutService.MarkPaidAsync(id, staffId, request);
            return Ok(ApiResponse<object>.SuccessResponse(result, "Monthly payout marked as paid."));
        }

        [HttpPatch("{id:int}/reject")]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> Reject(int id, [FromBody] RejectMonthlyPayoutRequest request)
        {
            var staffId = _userService.GetUserId();
            var result = await _monthlyPayoutService.RejectAsync(id, staffId, request);
            return Ok(ApiResponse<object>.SuccessResponse(result, "Monthly payout rejected successfully."));
        }

        [HttpPatch("{id:int}/request-retry")]
        [Authorize(Roles = "Advisor")]
        public async Task<IActionResult> RequestRetry(int id, [FromBody] RequestMonthlyPayoutRetryRequest request)
        {
            var advisorUserId = _userService.GetUserId();
            var result = await _monthlyPayoutService.RequestRetryAsync(id, advisorUserId, request);
            return Ok(ApiResponse<object>.SuccessResponse(result, "Retry request submitted successfully."));
        }

        [HttpGet]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> GetAll([FromQuery] SieveModel model)
        {
            var result = await _monthlyPayoutService.GetAllAsync(model);
            return Ok(ApiResponse<object>.SuccessResponse(result, "Monthly payouts retrieved successfully."));
        }

        [HttpGet("me")]
        [Authorize(Roles = "Advisor")]
        public async Task<IActionResult> GetMine([FromQuery] SieveModel model)
        {
            var userId = _userService.GetUserId();
            var result = await _monthlyPayoutService.GetMineAsync(userId, model);
            return Ok(ApiResponse<object>.SuccessResponse(result, "My monthly payouts retrieved successfully."));
        }

    }
}
