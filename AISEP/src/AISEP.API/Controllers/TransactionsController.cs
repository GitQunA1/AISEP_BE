using AISEP.BLL.Helpers;
using AISEP.BLL.Services.Transactions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AISEP.API.Controllers
{
    [ApiController]
    [Route("api/transactions")]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionService _transactionService;

        public TransactionsController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }
        //l?y danh sách t?ng h?p hoa h?ng ð?t ph?ng ð? thu ðý?c
        [HttpGet("collected-bookingcommission")]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> GetCollectedBookingCommissionIds()
        {
            var result = await _transactionService.GetCollectedBookingCommissionSummaryAsync();
            return Ok(ApiResponse<object>.SuccessResponse(result, "Collected booking commissions retrieved"));
        }
    }
}
