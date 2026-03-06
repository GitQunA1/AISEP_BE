using AISEP.Common;
using AISEP.DTOs;
using AISEP.Services.Auth;
using AISEP.Services.CurrentUser;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AISEP.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ICurrentUserService _currentUserService;

        public AuthController(IAuthService authService, ICurrentUserService currentUserService)
        {
            _authService = authService;
            _currentUserService = currentUserService;
        }

       
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (model.Password != model.ConfirmPassword)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Passwords do not match", "Validation failed"));
            }

            var (success, message, userId, email) = await _authService.RegisterAsync(model);

            if (!success)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(message, message));
            }

            return Ok(ApiResponse<object>.SuccessResponse(new { userId, email, emailSent = true }, message));
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Invalid email confirmation link", "Validation failed"));
            }

            var (success, message) = await _authService.ConfirmEmailAsync(userId, token);

            if (!success)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(message, message));
            }

            return Ok(ApiResponse<object>.SuccessResponse(null!, message));
        }

       
        [HttpPost("resend-confirmation")]
        public async Task<IActionResult> ResendConfirmation([FromBody] ResendConfirmationDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (success, message) = await _authService.ResendConfirmationAsync(model.Email);

            if (!success)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(message, message));
            }

            return Ok(ApiResponse<object>.SuccessResponse(null!, message));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (success, tokenResponse, message) = await _authService.LoginAsync(model);

            if (!success)
            {
                return Unauthorized(ApiResponse<object>.ErrorResponse(message, message, 401));
            }

            return Ok(ApiResponse<object>.SuccessResponse(tokenResponse, "Login successful"));
        }

      
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto model)
        {
            if (string.IsNullOrEmpty(model.RefreshToken))
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Refresh token is required", "Validation failed"));
            }

            var (success, tokenResponse, message) = await _authService.RefreshTokenAsync(model.RefreshToken);

            if (!success)
            {
                return Unauthorized(ApiResponse<object>.ErrorResponse(message, message, 401));
            }

            return Ok(ApiResponse<object>.SuccessResponse(tokenResponse, "Token refreshed successfully"));
        }

        [HttpPost("revoke-token")]
        [Authorize]
        public async Task<IActionResult> RevokeToken([FromBody] RefreshTokenDto model)
        {
            if (string.IsNullOrEmpty(model.RefreshToken))
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Refresh token is required", "Validation failed"));
            }

            var (success, message) = await _authService.RevokeTokenAsync(model.RefreshToken);

            if (!success)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(message, message));
            }

            return Ok(ApiResponse<object>.SuccessResponse(null!, message));
        }

      
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var (success, message) = await _authService.LogoutAsync(int.Parse(userId));

            if (!success)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(message, message));
            }

            return Ok(ApiResponse<object>.SuccessResponse(null!, message));
        }

    
    
    }
}
