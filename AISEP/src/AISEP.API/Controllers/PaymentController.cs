using AISEP.BLL.Helpers;
using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.Services.Payments;
using AISEP.BLL.Services.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AISEP.API.Controllers
{
    [ApiController]
    [Route("api/payments")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IUserService _currentUserService;

        public PaymentController(IPaymentService paymentService, IUserService currentUserService)
        {
            _paymentService = paymentService;
            _currentUserService = currentUserService;
        }

        [HttpGet("packages")]
        [Authorize]
        public async Task<IActionResult> GetPackages()
        {
            var result = await _paymentService.GetPackagesAsync();
            return Ok(ApiResponse<object>.SuccessResponse(result, "Packages retrieved successfully"));
        }

        // Create transaction + return VietQR code URL
        [HttpPost("checkout")]
        [Authorize]
        public async Task<IActionResult> Checkout([FromForm] CheckoutRequest request)
        {
            try
            {
                var userId = _currentUserService.GetUserId();
                var result = await _paymentService.CheckoutAsync(userId, request);
                return Ok(ApiResponse<object>.SuccessResponse(result, "Checkout created successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(ex.Message, "Not found", 404));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message, "Bad request", 400));
            }
        }

        // FE polls this every 3-5s to check payment status
        [HttpGet("{transactionId}/status")]
        [Authorize]
        public async Task<IActionResult> GetTransactionStatus(int transactionId)
        {
            try
            {
                var userId = _currentUserService.GetUserId();
                var result = await _paymentService.GetTransactionStatusAsync(userId, transactionId);
                return Ok(ApiResponse<object>.SuccessResponse(result, "Transaction status retrieved"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(ex.Message, "Not found", 404));
            }
        }

        // SePay calls this webhook when money arrives
        [HttpPost("sepay-webhook")]
        public async Task<IActionResult> SePayWebhook([FromBody] SePayWebhookRequest request)
        {
            try
            {
                await _paymentService.ProcessSePayWebhookAsync(request);
                return Ok(ApiResponse<object>.SuccessResponse(null!, "Webhook processed successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(ex.Message, "Not found", 404));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message, "Bad request", 400));
            }
        }
    }
}
