using AISEP.DTOs;

namespace AISEP.Services.Auth
{
    public interface IAuthService
    {
        Task<(bool Success, string Message, Guid? UserId, string? Email)> RegisterAsync(RegisterDto model);
        Task<(bool Success, string Message)> ConfirmEmailAsync(string userId, string token);
        Task<(bool Success, string Message)> ResendConfirmationAsync(string email);
        Task<(bool Success, TokenResponseDto? TokenResponse, string Message)> LoginAsync(LoginDto model);
        Task<(bool Success, TokenResponseDto? TokenResponse, string Message)> RefreshTokenAsync(string refreshToken);
        Task<(bool Success, string Message)> RevokeTokenAsync(string refreshToken);
        Task<(bool Success, string Message)> LogoutAsync(Guid userId);
    }
}
