using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.Helpers;
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

        public AdminController(IUserService userService, ITransactionService transactionService)
        {
            _userService = userService;
            _transactionService = transactionService;
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
