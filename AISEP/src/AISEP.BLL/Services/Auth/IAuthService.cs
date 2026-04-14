using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;

namespace AISEP.BLL.Services.Auth
{
    public interface IAuthService
    {
        Task<(bool Success, string Message, int? UserId, string? Email)> RegisterAsync(RegisterRequest model);
        Task<(bool Success, string Message)> ConfirmEmailAsync(string userId, string token);
        Task<(bool Success, string Message)> ResendConfirmationAsync(string email);
        Task<(bool Success, TokenResponse? TokenResponse, string Message)> LoginAsync(LoginRequest model);
        Task<(bool Success, TokenResponse? TokenResponse, string Message)> RefreshTokenAsync(string refreshToken);
        Task<(bool Success, string Message)> RevokeTokenAsync(string refreshToken);
        Task<(bool Success, string Message)> LogoutAsync(int userId);
        Task<(bool Success, string Message)> ForgotPasswordAsync(ForgotPasswordRequest model);
        Task<(bool Success, string Message)> ResetPasswordAsync(ResetPasswordRequest model);
        Task<(bool Success, string Message)> ChangePasswordAsync(int userId, ChangePasswordRequest model);
    }
}
