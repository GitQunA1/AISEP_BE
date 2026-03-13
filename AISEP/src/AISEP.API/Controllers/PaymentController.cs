using AISEP.BLL.Helpers;
using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.Services.Payments;
using Microsoft.AspNetCore.Mvc;

namespace AISEP.API.Controllers
{
    [ApiController]
    [Route("api/payments")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

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
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message, "Internal server error", 500));
            }
        }
    }
}
