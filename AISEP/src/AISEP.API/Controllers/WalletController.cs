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
        private readonly IWalletQueryService _walletQueryService;
        private readonly IUserService _userService;

        public WalletController(IWalletQueryService walletQueryService, IUserService userService)
        {
            _walletQueryService = walletQueryService;
            _userService = userService;
        }

        [HttpGet("me")]
        [Authorize(Roles = "Advisor")]
        public async Task<IActionResult> GetMyWallet()
        {
            try
            {
                var userId = _userService.GetUserId();
                var result = await _walletQueryService.GetMyWalletAsync(userId);
                return Ok(ApiResponse<object>.SuccessResponse(result, "Wallet retrieved successfully."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(ex.Message, "Not Found", 404));
            }
        }

        [HttpGet("me/transactions")]
        [Authorize(Roles = "Advisor")]
        public async Task<IActionResult> GetMyWalletTransactions([FromQuery] SieveModel model)
        {
            try
            {
                var userId = _userService.GetUserId();
                var result = await _walletQueryService.GetMyWalletTransactionsAsync(userId, model);
                return Ok(ApiResponse<object>.SuccessResponse(result, "Wallet transactions retrieved successfully."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(ex.Message, "Not Found", 404));
            }
        }

        [HttpGet]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> GetAllAdvisorWallets([FromQuery] SieveModel model)
        {
            var result = await _walletQueryService.GetAllAdvisorWalletsAsync(model);
            return Ok(ApiResponse<object>.SuccessResponse(result, "Advisor wallets retrieved successfully."));
        }
    }
}
