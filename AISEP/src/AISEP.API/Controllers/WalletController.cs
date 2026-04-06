using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.Helpers;
using AISEP.BLL.Services.Users;
using AISEP.BLL.Services.Wallets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;

namespace AISEP.API.Controllers
{
    [ApiController]
    [Route("api/wallets")]
    public class WalletController : ControllerBase
    {
        private readonly IWalletService _walletService;
        private readonly IUserService _userService;

        public WalletController(IWalletService walletService, IUserService userService)
        {
            _walletService = walletService;
            _userService = userService;
        }

        [HttpPost("withdraw-requests")]
        [Authorize(Roles = "Advisor")]
        public async Task<IActionResult> CreateWithdrawRequest([FromBody] CreateWithdrawRequestDto dto)
        {
            try
            {
                var userId = _userService.GetUserId();
                var result = await _walletService.CreateWithdrawRequestAsync(userId, dto);
                return Ok(ApiResponse<object>.SuccessResponse(result, "Withdraw request created successfully."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(ex.Message, "Not Found", 404));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message, "Bad Request", 400));
            }
        }

        [HttpGet("withdraw-requests/me")]
        [Authorize(Roles = "Advisor")]
        public async Task<IActionResult> GetMyWithdrawRequests([FromQuery] SieveModel model)
        {
            try
            {
                var userId = _userService.GetUserId();
                var result = await _walletService.GetMyWithdrawRequestsAsync(userId, model);
                return Ok(ApiResponse<object>.SuccessResponse(result, "My withdraw requests retrieved successfully."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(ex.Message, "Not Found", 404));
            }
        }

        [HttpGet("withdraw-requests")]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> GetAllWithdrawRequests([FromQuery] SieveModel model)
        {
            var result = await _walletService.GetAllWithdrawRequestsAsync(model);
            return Ok(ApiResponse<object>.SuccessResponse(result, "Withdraw requests retrieved successfully."));
        }

        [HttpPatch("withdraw-requests/{withdrawRequestId:int}/approve")]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> ApproveWithdrawRequest(int withdrawRequestId, [FromBody] ProcessWithdrawRequestDto? dto)
        {
            try
            {
                var reviewerId = _userService.GetUserId();
                var result = await _walletService.ApproveWithdrawRequestAsync(withdrawRequestId, reviewerId, dto?.ProofImageUrl);
                return Ok(ApiResponse<object>.SuccessResponse(result, "Withdraw request approved successfully."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(ex.Message, "Not Found", 404));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message, "Bad Request", 400));
            }
        }

        [HttpPatch("withdraw-requests/{withdrawRequestId:int}/reject")]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> RejectWithdrawRequest(int withdrawRequestId, [FromBody] ProcessWithdrawRequestDto? dto)
        {
            try
            {
                var reviewerId = _userService.GetUserId();
                var result = await _walletService.RejectWithdrawRequestAsync(withdrawRequestId, reviewerId, dto?.Reason);
                return Ok(ApiResponse<object>.SuccessResponse(result, "Withdraw request rejected successfully."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(ex.Message, "Not Found", 404));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message, "Bad Request", 400));
            }
        }
    }
}
