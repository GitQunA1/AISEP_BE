using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.Helpers;
using AISEP.BLL.Services.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;

namespace AISEP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] SieveModel model)
        {
            var result = await _userService.GetAllAsync(model);
            return Ok(ApiResponse<object>.SuccessResponse(result, "Success"));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user is null)
                return NotFound(ApiResponse<object>.ErrorResponse("User not found.", "Not found", 404));

            return Ok(ApiResponse<object>.SuccessResponse(user, "Success"));
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateUserRequest request)
        {
            try
            {
                var user = await _userService.UpdateAsync(id, request);
                if (user is null)
                    return NotFound(ApiResponse<object>.ErrorResponse("User not found.", "Not found", 404));

                return Ok(ApiResponse<object>.SuccessResponse(user, "User updated successfully."));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message, "Invalid operation", 400));
            }
        }

        [HttpPatch("{id:int}")]
        public async Task<IActionResult> Ban(int id)
        {
            try
            {
                var deleted = await _userService.DeleteAsync(id);
                if (!deleted)
                    return NotFound(ApiResponse<object>.ErrorResponse("User not found.", "Not found", 404));

                return Ok(ApiResponse<object>.SuccessResponse(null!, "User deleted successfully."));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message, "Invalid operation", 400));
            }
        }
    }
}
