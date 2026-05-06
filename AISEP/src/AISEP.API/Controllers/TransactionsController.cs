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

        [HttpGet]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> GetAll([FromQuery] Sieve.Models.SieveModel model)
        {
            var result = await _transactionService.GetAllForAdminAsync(model);
            return Ok(ApiResponse<object>.SuccessResponse(result, "Transactions retrieved successfully."));
        }

        //lấy danh sách tổng hợp hoa hồng booking đã thu được
        [HttpGet("collected-bookingcommission")]
        //[Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> GetCollectedBookingCommissionIds()
        {
            var result = await _transactionService.GetCollectedBookingCommissionSummaryAsync();
            return Ok(ApiResponse<object>.SuccessResponse(result, "Collected booking commissions retrieved"));
        }
    }
}
