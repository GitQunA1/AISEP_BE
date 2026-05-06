using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;
using AISEP.BLL.Helpers;
using AISEP.BLL.Services.Admins;
using AISEP.BLL.Services.Transactions;
using AISEP.BLL.Services.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;

namespace AISEP.API.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ITransactionService _transactionService;
        private readonly IAdminService _adminService;

        public AdminController(IUserService userService, ITransactionService transactionService, IAdminService adminService)
        {
            _userService = userService;
            _transactionService = transactionService;
            _adminService = adminService;
        }

        [HttpGet("transactions")]
        public async Task<IActionResult> GetTransactions([FromQuery] SieveModel model)
        {
            var result = await _transactionService.GetAllForAdminAsync(model);
            return Ok(ApiResponse<object>.SuccessResponse(result, "Transactions retrieved successfully."));
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers([FromQuery] SieveModel model)
        {
            var result = await _userService.GetAllForAdminAsync(model);
            return Ok(ApiResponse<object>.SuccessResponse(result, "Users retrieved successfully."));
        }

        [HttpGet("platform-overview")]
        public async Task<IActionResult> GetPlatformOverview([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            var response = await _adminService.GetPlatformOverviewAsync(from, to);

            return Ok(ApiResponse<object>.SuccessResponse(response, "Platform overview retrieved successfully."));
        }

        [HttpGet("project-status")]
        public async Task<IActionResult> GetProjectStatusBreakdown()
        {
            var response = await _adminService.GetProjectStatusBreakdownAsync();
            return Ok(ApiResponse<object>.SuccessResponse(response, "Project status breakdown retrieved successfully."));
        }

        [HttpGet("investment-trends")]
        public async Task<IActionResult> GetInvestmentTrends(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate)
        {
            var response = await _adminService.GetInvestmentTrendsAsync(fromDate ?? from, toDate ?? to);
            return Ok(ApiResponse<object>.SuccessResponse(response, "Investment trends retrieved successfully."));
        }

        [HttpGet("platform-revenue")]
        public async Task<IActionResult> GetPlatformRevenue(
            [FromQuery] int? month,
            [FromQuery] int? year,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate)
        {
            var response = await _adminService.GetPlatformRevenueStatisticsAsync(
                month,
                year,
                fromDate ?? from,
                toDate ?? to);
            return Ok(ApiResponse<object>.SuccessResponse(response, "Platform revenue statistics retrieved successfully."));
        }

        [HttpGet("users/{id:int}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _userService.GetByIdForAdminAsync(id);
            if (user is null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse("User not found.", "Not found", 404));
            }

            return Ok(ApiResponse<object>.SuccessResponse(user, "User retrieved successfully."));
        }

        [HttpPost("create-staff")]
        public async Task<IActionResult> CreateStaff([FromBody] AdminCreateUserRequest request)
        {
            var user = await _userService.CreateForAdminAsync(request);
            return Ok(ApiResponse<object>.SuccessResponse(user, "Staff account created successfully."));
        }

        [HttpPut("users/{id:int}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] AdminUpdateUserRequest request)
        {
            var user = await _userService.UpdateForAdminAsync(id, request);
            if (user is null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse("User not found.", "Not found", 404));
            }

            return Ok(ApiResponse<object>.SuccessResponse(user, "User updated successfully."));
        }

        [HttpDelete("users/{id:int}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var deleted = await _userService.DeleteForAdminAsync(id);
            if (!deleted)
            {
                return NotFound(ApiResponse<object>.ErrorResponse("User not found.", "Not found", 404));
            }

            return Ok(ApiResponse<object>.SuccessResponse(null!, "User deleted permanently successfully."));
        }
    }
}
