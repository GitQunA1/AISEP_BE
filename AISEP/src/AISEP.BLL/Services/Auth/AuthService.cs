using AISEP.DAL.Common;
using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;
using AISEP.DAL.Entities;
using AISEP.DAL.Enums;
using AISEP.BLL.Services.Email;
using AISEP.BLL.Services.Jwt;
using AISEP.BLL.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using System.Text;

namespace AISEP.BLL.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IJwtService _jwtService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly JwtSettings _jwtSettings;

        public AuthService(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IJwtService jwtService,
            IUnitOfWork unitOfWork,
            IEmailService emailService,
            IConfiguration configuration,
            IOptions<JwtSettings> jwtSettings)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtService = jwtService;
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _configuration = configuration;
            _jwtSettings = jwtSettings.Value;
        }

        public async Task<(bool Success, string Message, int? UserId, string? Email)> RegisterAsync(RegisterRequest model)
        {
         
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                return (false, "Email already registered", null, null);
            }

            // Validate role (optional)
            if (model.Role == UserRole.Admin || model.Role == UserRole.Staff)
            {
                return (false, "Cannot register as Admin or Staff through public registration", null, null);
            }


            var userName = model.Name.Trim();

         

            var user = new User
            {
                UserName = userName,
                FullName = model.FullName,
                Email = model.Email,
                Role = model.Role,
                Status = UserStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return (false, errors, null, null);
            }

            // Generate email confirmation token
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var confirmationLink = $"{_configuration["AppUrl"]}/api/auth/confirm-email?userId={user.Id}&token={encodedToken}";

            // Send confirmation email
            //try
            //{
                await _emailService.SendEmailConfirmationAsync(user.Email!, user.UserName!, confirmationLink);
            //}
            //catch (Exception ex)
            //{
               
            //    Console.WriteLine($"Failed to send confirmation email: {ex.Message}");
            //}

            return (true, "User registered successfully. Please check your email to confirm your account.", user.Id, user.Email);
        }

        public async Task<(bool Success, string Message)> ConfirmEmailAsync(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return (false, "User not found");
            }

            if (user.EmailConfirmed)
            {
                return (true, "Email already confirmed");
            }

            // Decode token
            var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));

            var result = await _userManager.ConfirmEmailAsync(user, decodedToken);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return (false, $"Email confirmation failed: {errors}");
            }

         
            user.EmailConfirmed = true;
            user.Status = UserStatus.Active;
            await _userManager.UpdateAsync(user);

            return (true, "Email confirmed successfully! You can now login.");
        }

        public async Task<(bool Success, string Message)> ResendConfirmationAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
            
                return (true, "If the email exists, a confirmation link has been sent.");
            }

            if (user.EmailConfirmed)
            {
                return (false, "Email is already confirmed");
            }

            // Generate new token
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var confirmationLink = $"{_configuration["AppUrl"]}/auth/confirm-email?userId={user.Id}&token={encodedToken}";

            // Send email
            try
            {
                await _emailService.SendEmailConfirmationAsync(user.Email!, user.UserName!, confirmationLink);
                return (true, "Confirmation email has been resent. Please check your inbox.");
            }
            catch (Exception ex)
            {
                return (false, "Failed to send email. Please try again later.");
            }
        }

        public async Task<(bool Success, TokenResponse? TokenResponse, string Message)> LoginAsync(LoginRequest model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return (false, null, "Invalid email or password");
            }

          
            if (user.Status == UserStatus.Banned)
            {
                return (false, null, "Account has been banned");
            }

            if (!user.EmailConfirmed)
            {
                return (false, null, "Email has not been confirmed. Please check your inbox and confirm your email before logging in.");
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: true);

            if (result.IsLockedOut)
            {
                return (false, null, "Account locked due to multiple failed login attempts");
            }

            if (!result.Succeeded)
            {
                return (false, null, "Invalid email or password");
            }

            // Generate tokens
            var accessToken = _jwtService.GenerateAccessToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();

            // Save refresh token to database using UnitOfWork
            var refreshTokenEntity = new RefreshToken
            {
                UserId = user.Id,
                Token = refreshToken,
                ExpiryDate = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
                CreatedAt = DateTime.UtcNow,
                CreatedByIp = null 
            };

            await _unitOfWork.RefreshTokens.AddAsync(refreshTokenEntity);
            await _unitOfWork.SaveChangesAsync();

            var tokenResponse = new TokenResponse
            {
                UserId = user.Id,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AccessTokenExpiration = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
                RefreshTokenExpiration = refreshTokenEntity.ExpiryDate

            };

            return (true, tokenResponse, "Login successful");
        }

        public async Task<(bool Success, TokenResponse? TokenResponse, string Message)> RefreshTokenAsync(string refreshToken)
        {
           
            var refreshTokenEntity = await _unitOfWork.RefreshTokens.GetByTokenAsync(refreshToken);

            if (refreshTokenEntity == null)
            {
                return (false, null, "Invalid refresh token");
            }

            
            var user = await _userManager.FindByIdAsync(refreshTokenEntity.UserId.ToString());
            if (user == null)
            {
                return (false, null, "User not found");
            }

           
            if (!refreshTokenEntity.IsActive)
            {
                return (false, null, "Refresh token is no longer active");
            }

            // Generate new tokens
            var newAccessToken = _jwtService.GenerateAccessToken(user);
            var newRefreshToken = _jwtService.GenerateRefreshToken();

            // Revoke old refresh token
            refreshTokenEntity.IsRevoked = true;
            refreshTokenEntity.RevokedAt = DateTime.UtcNow;
            refreshTokenEntity.ReplacedByToken = newRefreshToken;

            // Create new refresh token
            var newRefreshTokenEntity = new RefreshToken
            {
                UserId = refreshTokenEntity.UserId,
                Token = newRefreshToken,
                ExpiryDate = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
                CreatedAt = DateTime.UtcNow,
                CreatedByIp = null 
            };

            await _unitOfWork.RefreshTokens.UpdateAsync(refreshTokenEntity);
            await _unitOfWork.RefreshTokens.AddAsync(newRefreshTokenEntity);
            await _unitOfWork.SaveChangesAsync();

            var tokenResponse = new TokenResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                AccessTokenExpiration = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
                RefreshTokenExpiration = newRefreshTokenEntity.ExpiryDate
            };

            return (true, tokenResponse, "Token refreshed successfully");
        }

        public async Task<(bool Success, string Message)> RevokeTokenAsync(string refreshToken)
        {
            var refreshTokenEntity = await _unitOfWork.RefreshTokens.GetByTokenAsync(refreshToken);

            if (refreshTokenEntity == null)
            {
                return (false, "Refresh token not found");
            }

            if (!refreshTokenEntity.IsActive)
            {
                return (false, "Token is already inactive");
            }

            refreshTokenEntity.IsRevoked = true;
            refreshTokenEntity.RevokedAt = DateTime.UtcNow;

            await _unitOfWork.RefreshTokens.UpdateAsync(refreshTokenEntity);
            await _unitOfWork.SaveChangesAsync();

            return (true, "Token revoked successfully");
        }

        public async Task<(bool Success, string Message)> LogoutAsync(int userId)
        {
            await RevokeAllActiveRefreshTokensAsync(userId);
            await _unitOfWork.SaveChangesAsync();
            await _signInManager.SignOutAsync();

            return (true, "Logout successful");
        }

        public async Task<(bool Success, string Message)> ForgotPasswordAsync(ForgotPasswordRequest model)
        {
            var email = model.Email?.Trim();
            if (string.IsNullOrWhiteSpace(email))
            {
                return (true, "If the account exists, a password reset link has been sent.");
            }

            var user = await _userManager.FindByEmailAsync(email);

            if (user is null || string.IsNullOrWhiteSpace(user.Email))
            {
                return (true, "If the account exists, a password reset link has been sent.");
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var appUrl = _configuration["FrontendUrl"] ?? _configuration["AppUrl"] ?? string.Empty;
            var resetLink = $"{appUrl.TrimEnd('/')}/reset-password?userId={user.Id}&token={encodedToken}";

            try
            {
                await _emailService.SendPasswordResetAsync(user.Email, user.UserName ?? user.Email, resetLink);
            }
            catch
            {
                return (false, "Failed to send reset email. Please try again later.");
            }

            return (true, "If the account exists, a password reset link has been sent.");
        }

        public async Task<(bool Success, string Message)> ResetPasswordAsync(ResetPasswordRequest model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user is null)
            {
                return (false, "Invalid reset request.");
            }

            string decodedToken;
            try
            {
                decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Token));
            }
            catch
            {
                decodedToken = model.Token;
            }

            var result = await _userManager.ResetPasswordAsync(user, decodedToken, model.NewPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return (false, errors);
            }

            await RevokeAllActiveRefreshTokensAsync(user.Id);
            await _unitOfWork.SaveChangesAsync();
            return (true, "Password has been reset successfully. Please login again.");
        }

        public async Task<(bool Success, string Message)> ChangePasswordAsync(int userId, ChangePasswordRequest model)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user is null)
            {
                return (false, "User not found.");
            }

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return (false, errors);
            }

            await RevokeAllActiveRefreshTokensAsync(user.Id);
            await _unitOfWork.SaveChangesAsync();
            return (true, "Password changed successfully. Please login again.");
        }

        private async Task RevokeAllActiveRefreshTokensAsync(int userId)
        {
            var refreshTokens = await _unitOfWork.RefreshTokens.GetActiveTokensByUserIdAsync(userId);
            foreach (var token in refreshTokens)
            {
                token.IsRevoked = true;
                token.RevokedAt = DateTime.UtcNow;
            }

            await _unitOfWork.RefreshTokens.UpdateRangeAsync(refreshTokens);
        }
    }
}
