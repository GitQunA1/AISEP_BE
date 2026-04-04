using AISEP.BLL.Helpers;
using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.Services.Payments;
using AISEP.BLL.Services.Users;
using AISEP.DAL.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;

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

        [HttpGet("packages/investor")]
        [Authorize(Roles = "Investor")]
        public async Task<IActionResult> GetInvestorPackages()
        {
            var result = await _paymentService.GetInvestorPackagesAsync();
            return Ok(ApiResponse<object>.SuccessResponse(result, "Investor packages retrieved successfully"));
        }

        [HttpGet("packages/startup")]
        [Authorize(Roles = "Startup")]
        public async Task<IActionResult> GetStartupPackages()
        {
            var result = await _paymentService.GetStartupPackagesAsync();
            return Ok(ApiResponse<object>.SuccessResponse(result, "Startup packages retrieved successfully"));
        }

        [HttpPost("bookings/{bookingId:int}/checkout")]
        [Authorize(Roles = "Investor,Startup")]
        public async Task<IActionResult> CheckoutBooking(int bookingId)
        {
            try
            {
                var userId = _currentUserService.GetUserId();
                var result = await _paymentService.CheckoutAsync(userId, new CheckoutRequest
                {
                    ReferenceType = ReferenceType.Booking,
                    ReferenceId = bookingId
                });
                return Ok(ApiResponse<object>.SuccessResponse(result, "Booking checkout created successfully"));
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
        // Backward-compatible endpoint for subscription checkout
        [HttpPost("checkout")]
        [Authorize(Roles = "Investor,Startup")]
        public async Task<IActionResult> Checkout([FromBody] CheckoutRequest request)
        {
            try
            {
                if (request.ReferenceType != ReferenceType.Subscription)
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        "This endpoint only supports subscription checkout.",
                        "Bad request",
                        400));
                }

                var userId = _currentUserService.GetUserId();
                var result = await _paymentService.CheckoutAsync(userId, request);
                return Ok(ApiResponse<object>.SuccessResponse(result, "Subscription checkout created successfully"));
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

        [HttpGet("bookings/{bookingId:int}/status")]
        [Authorize(Roles = "Investor,Startup")]
        public async Task<IActionResult> GetBookingPaymentStatus(int bookingId)
        {
            try
            {
                var userId = _currentUserService.GetUserId();
                var result = await _paymentService.GetBookingPaymentStatusAsync(userId, bookingId);
                return Ok(ApiResponse<object>.SuccessResponse(result, "Booking payment status retrieved"));
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

        [HttpGet("bookings/transactions")]
        [Authorize(Roles = "Investor,Startup")]
        public async Task<IActionResult> GetBookingPaymentTransactions([FromQuery] SieveModel model)
        {
            var userId = _currentUserService.GetUserId();
            var result = await _paymentService.GetBookingPaymentTransactionsAsync(userId, model);
            return Ok(ApiResponse<object>.SuccessResponse(result, "Booking payment transactions retrieved"));
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
