using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.Services.Auth;
using AISEP.BLL.Services.Email;
using AISEP.BLL.Services.Jwt;
using AISEP.BLL.Settings;
using AISEP.DAL.Common;
using AISEP.DAL.Entities;
using AISEP.DAL.Enums;
using AISEP.DAL.Repositories.RefreshTokens;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Text;
using Xunit;

namespace AISEP.BLL.Tests.Auth;

public class AuthServiceGroupedTests
{
    [Fact]
    public async Task UT001_RegisterAsync_ShouldHandleGroupedScenarios()
    {
        var baseRequest = new RegisterRequest
        {
            Name = "newuser",
            FullName = "New User",
            Email = "newuser@test.local",
            Password = "P@ssw0rd!",
            ConfirmPassword = "P@ssw0rd!",
            Role = UserRole.Startup
        };

        // Scenario 1: email already registered
        var (service1, userManager1, _, _, _, _, _) = CreateSut();
        userManager1
            .Setup(x => x.FindByEmailAsync(baseRequest.Email))
            .ReturnsAsync(new User { Id = 900, Email = baseRequest.Email });

        var emailExistsResult = await service1.RegisterAsync(baseRequest);
        Assert.False(emailExistsResult.Success);
        Assert.Equal("Email already registered", emailExistsResult.Message);

        // Scenario 2: role admin/staff is blocked
        var adminRequest = new RegisterRequest
        {
            Name = "admin",
            FullName = "Admin",
            Email = "admin@test.local",
            Password = "P@ssw0rd!",
            ConfirmPassword = "P@ssw0rd!",
            Role = UserRole.Admin
        };

        var (service2, userManager2, _, _, _, _, _) = CreateSut();
        userManager2.Setup(x => x.FindByEmailAsync(adminRequest.Email)).ReturnsAsync((User?)null);

        var blockedRoleResult = await service2.RegisterAsync(adminRequest);
        Assert.False(blockedRoleResult.Success);
        Assert.Contains("Cannot register as Admin or Staff", blockedRoleResult.Message);

        // Scenario 3: identity create failed
        var (service3, userManager3, _, _, _, _, _) = CreateSut();
        userManager3.Setup(x => x.FindByEmailAsync(baseRequest.Email)).ReturnsAsync((User?)null);
        userManager3
            .Setup(x => x.CreateAsync(It.IsAny<User>(), baseRequest.Password))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Create user failed" }));

        var createFailedResult = await service3.RegisterAsync(baseRequest);
        Assert.False(createFailedResult.Success);
        Assert.Contains("Create user failed", createFailedResult.Message);

        // Scenario 4 + 5: registration success sends email and returns user info
        var (service4, userManager4, _, _, _, _, emailService4) = CreateSut();
        userManager4.Setup(x => x.FindByEmailAsync(baseRequest.Email)).ReturnsAsync((User?)null);
        userManager4
            .Setup(x => x.CreateAsync(It.IsAny<User>(), baseRequest.Password))
            .Callback<User, string>((u, _) => u.Id = 1234)
            .ReturnsAsync(IdentityResult.Success);
        userManager4.Setup(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<User>())).ReturnsAsync("token-raw");
        emailService4
            .Setup(x => x.SendEmailConfirmationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var successResult = await service4.RegisterAsync(baseRequest);
        Assert.True(successResult.Success);
        Assert.Equal(1234, successResult.UserId);
        Assert.Equal(baseRequest.Email, successResult.Email);
        emailService4.Verify(
            x => x.SendEmailConfirmationAsync(
                baseRequest.Email,
                baseRequest.Name,
                It.Is<string>(link => link.Contains("/api/auth/confirm-email?userId=1234&token="))),
            Times.Once);
    }

    [Fact]
    public async Task UT002_ConfirmEmailAsync_ShouldHandleGroupedScenarios()
    {
        // Scenario 1: user not found
        var (service1, userManager1, _, _, _, _, _) = CreateSut();
        userManager1.Setup(x => x.FindByIdAsync("1")).ReturnsAsync((User?)null);

        var notFoundResult = await service1.ConfirmEmailAsync("1", EncodeToken("token"));
        Assert.False(notFoundResult.Success);
        Assert.Equal("User not found", notFoundResult.Message);

        // Scenario 2: already confirmed
        var (service2, userManager2, _, _, _, _, _) = CreateSut();
        var confirmedUser = new User { Id = 2, EmailConfirmed = true, Status = UserStatus.Active };
        userManager2.Setup(x => x.FindByIdAsync("2")).ReturnsAsync(confirmedUser);

        var alreadyConfirmedResult = await service2.ConfirmEmailAsync("2", EncodeToken("token"));
        Assert.True(alreadyConfirmedResult.Success);
        Assert.Equal("Email already confirmed", alreadyConfirmedResult.Message);

        // Scenario 3: token decode is valid but confirm fails at identity layer
        var (service3, userManager3, _, _, _, _, _) = CreateSut();
        var pendingUser1 = new User { Id = 3, EmailConfirmed = false, Status = UserStatus.Pending };
        userManager3.Setup(x => x.FindByIdAsync("3")).ReturnsAsync(pendingUser1);
        userManager3
            .Setup(x => x.ConfirmEmailAsync(pendingUser1, "bad-token"))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Invalid token" }));

        var confirmFailedResult = await service3.ConfirmEmailAsync("3", EncodeToken("bad-token"));
        Assert.False(confirmFailedResult.Success);
        Assert.Contains("Email confirmation failed", confirmFailedResult.Message);

        // Scenario 4: success updates status and email confirmation
        var (service4, userManager4, _, _, _, _, _) = CreateSut();
        var pendingUser2 = new User { Id = 4, EmailConfirmed = false, Status = UserStatus.Pending };
        userManager4.Setup(x => x.FindByIdAsync("4")).ReturnsAsync(pendingUser2);
        userManager4
            .Setup(x => x.ConfirmEmailAsync(pendingUser2, "valid-token"))
            .ReturnsAsync(IdentityResult.Success);
        userManager4.Setup(x => x.UpdateAsync(pendingUser2)).ReturnsAsync(IdentityResult.Success);

        var successResult = await service4.ConfirmEmailAsync("4", EncodeToken("valid-token"));
        Assert.True(successResult.Success);
        Assert.True(pendingUser2.EmailConfirmed);
        Assert.Equal(UserStatus.Active, pendingUser2.Status);
        userManager4.Verify(x => x.UpdateAsync(pendingUser2), Times.Once);
    }

    [Fact]
    public async Task UT003_ResendConfirmationAsync_ShouldHandleGroupedScenarios()
    {
        // Scenario 1: user not found should return generic success
        var (service1, userManager1, _, _, _, _, _) = CreateSut();
        userManager1.Setup(x => x.FindByEmailAsync("missing@test.local")).ReturnsAsync((User?)null);

        var notFoundResult = await service1.ResendConfirmationAsync("missing@test.local");
        Assert.True(notFoundResult.Success);
        Assert.Contains("If the email exists", notFoundResult.Message);

        // Scenario 2: already confirmed
        var (service2, userManager2, _, _, _, _, _) = CreateSut();
        userManager2
            .Setup(x => x.FindByEmailAsync("confirmed@test.local"))
            .ReturnsAsync(new User
            {
                Id = 10,
                Email = "confirmed@test.local",
                UserName = "confirmed",
                EmailConfirmed = true,
                Status = UserStatus.Active
            });

        var alreadyConfirmedResult = await service2.ResendConfirmationAsync("confirmed@test.local");
        Assert.False(alreadyConfirmedResult.Success);
        Assert.Equal("Email is already confirmed", alreadyConfirmedResult.Message);

        // Scenario 3: sending email fails
        var (service3, userManager3, _, _, _, _, emailService3) = CreateSut();
        var pendingUser1 = new User
        {
            Id = 11,
            Email = "pending1@test.local",
            UserName = "pending1",
            EmailConfirmed = false,
            Status = UserStatus.Pending
        };
        userManager3.Setup(x => x.FindByEmailAsync(pendingUser1.Email!)).ReturnsAsync(pendingUser1);
        userManager3.Setup(x => x.GenerateEmailConfirmationTokenAsync(pendingUser1)).ReturnsAsync("token-retry");
        emailService3
            .Setup(x => x.SendEmailConfirmationAsync(pendingUser1.Email!, pendingUser1.UserName!, It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("SMTP error"));

        var sendFailedResult = await service3.ResendConfirmationAsync(pendingUser1.Email!);
        Assert.False(sendFailedResult.Success);
        Assert.Equal("Failed to send email. Please try again later.", sendFailedResult.Message);

        // Scenario 4: success path
        var (service4, userManager4, _, _, _, _, emailService4) = CreateSut();
        var pendingUser2 = new User
        {
            Id = 12,
            Email = "pending2@test.local",
            UserName = "pending2",
            EmailConfirmed = false,
            Status = UserStatus.Pending
        };
        userManager4.Setup(x => x.FindByEmailAsync(pendingUser2.Email!)).ReturnsAsync(pendingUser2);
        userManager4.Setup(x => x.GenerateEmailConfirmationTokenAsync(pendingUser2)).ReturnsAsync("token-ok");
        emailService4
            .Setup(x => x.SendEmailConfirmationAsync(pendingUser2.Email!, pendingUser2.UserName!, It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var successResult = await service4.ResendConfirmationAsync(pendingUser2.Email!);
        Assert.True(successResult.Success);
        Assert.Contains("Confirmation email has been resent", successResult.Message);
        emailService4.Verify(
            x => x.SendEmailConfirmationAsync(
                pendingUser2.Email!,
                pendingUser2.UserName!,
                It.Is<string>(link => link.Contains("/auth/confirm-email?userId=12&token="))),
            Times.Once);
    }

    [Fact]
    public async Task UT004_LoginAsync_ShouldFail_WhenGroupedFailureScenariosOccur()
    {
        var request = new LoginRequest { Email = "user@test.local", Password = "P@ssw0rd!" };

        // Scenario 1: user not found
        var (service1, userManager1, _, _, _, _, _) = CreateSut();
        userManager1.Setup(x => x.FindByEmailAsync(request.Email)).ReturnsAsync((User?)null);

        var userMissingResult = await service1.LoginAsync(request);
        Assert.False(userMissingResult.Success);
        Assert.Null(userMissingResult.TokenResponse);
        Assert.Equal("Invalid email or password", userMissingResult.Message);

        // Scenario 2: banned user
        var (service2, userManager2, _, _, _, _, _) = CreateSut();
        userManager2.Setup(x => x.FindByEmailAsync(request.Email)).ReturnsAsync(new User
        {
            Id = 20,
            Email = request.Email,
            UserName = "banned",
            EmailConfirmed = true,
            Status = UserStatus.Banned
        });

        var bannedResult = await service2.LoginAsync(request);
        Assert.False(bannedResult.Success);
        Assert.Null(bannedResult.TokenResponse);
        Assert.Equal("Account has been banned", bannedResult.Message);

        // Scenario 3: email not confirmed
        var (service3, userManager3, _, _, _, _, _) = CreateSut();
        userManager3.Setup(x => x.FindByEmailAsync(request.Email)).ReturnsAsync(new User
        {
            Id = 21,
            Email = request.Email,
            UserName = "unconfirmed",
            EmailConfirmed = false,
            Status = UserStatus.Active
        });

        var emailNotConfirmedResult = await service3.LoginAsync(request);
        Assert.False(emailNotConfirmedResult.Success);
        Assert.Null(emailNotConfirmedResult.TokenResponse);
        Assert.Contains("Email has not been confirmed", emailNotConfirmedResult.Message);

        // Scenario 4: account locked out after password check
        var (service4, userManager4, signInManager4, _, _, _, _) = CreateSut();
        var lockedUser = new User
        {
            Id = 22,
            Email = request.Email,
            UserName = "locked",
            EmailConfirmed = true,
            Status = UserStatus.Active
        };
        userManager4.Setup(x => x.FindByEmailAsync(request.Email)).ReturnsAsync(lockedUser);
        signInManager4
            .Setup(x => x.CheckPasswordSignInAsync(lockedUser, request.Password, true))
            .ReturnsAsync(SignInResult.LockedOut);

        var lockedOutResult = await service4.LoginAsync(request);
        Assert.False(lockedOutResult.Success);
        Assert.Null(lockedOutResult.TokenResponse);
        Assert.Equal("Account locked due to multiple failed login attempts", lockedOutResult.Message);

        // Scenario 5: invalid credentials after password check
        var (service5, userManager5, signInManager5, _, _, _, _) = CreateSut();
        var wrongPasswordUser = new User
        {
            Id = 23,
            Email = request.Email,
            UserName = "wrong-password",
            EmailConfirmed = true,
            Status = UserStatus.Active
        };
        userManager5.Setup(x => x.FindByEmailAsync(request.Email)).ReturnsAsync(wrongPasswordUser);
        signInManager5
            .Setup(x => x.CheckPasswordSignInAsync(wrongPasswordUser, request.Password, true))
            .ReturnsAsync(SignInResult.Failed);

        var wrongPasswordResult = await service5.LoginAsync(request);
        Assert.False(wrongPasswordResult.Success);
        Assert.Null(wrongPasswordResult.TokenResponse);
        Assert.Equal("Invalid email or password", wrongPasswordResult.Message);
    }

    [Fact]
    public async Task UT005_LoginAsync_ShouldReturnTokens_WhenCredentialsValid()
    {
        var request = new LoginRequest { Email = "good@test.local", Password = "P@ssw0rd!" };
        var (service, userManager, signInManager, jwtService, unitOfWork, refreshTokenRepo, _) = CreateSut();

        var validUser = new User
        {
            Id = 77,
            Email = request.Email,
            UserName = "good-user",
            EmailConfirmed = true,
            Status = UserStatus.Active
        };

        userManager.Setup(x => x.FindByEmailAsync(request.Email)).ReturnsAsync(validUser);
        signInManager
            .Setup(x => x.CheckPasswordSignInAsync(validUser, request.Password, true))
            .ReturnsAsync(SignInResult.Success);

        jwtService.Setup(x => x.GenerateAccessToken(validUser)).Returns("access-token-123");
        jwtService.Setup(x => x.GenerateRefreshToken()).Returns("refresh-token-123");

        RefreshToken? savedRefreshToken = null;
        refreshTokenRepo
            .Setup(x => x.AddAsync(It.IsAny<RefreshToken>()))
            .Callback<RefreshToken>(token => savedRefreshToken = token)
            .Returns(Task.CompletedTask);
        unitOfWork.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        var result = await service.LoginAsync(request);

        Assert.True(result.Success);
        Assert.NotNull(result.TokenResponse);
        Assert.Equal(77, result.TokenResponse!.UserId);
        Assert.Equal("access-token-123", result.TokenResponse.AccessToken);
        Assert.Equal("refresh-token-123", result.TokenResponse.RefreshToken);

        Assert.NotNull(savedRefreshToken);
        Assert.Equal(77, savedRefreshToken!.UserId);
        Assert.Equal("refresh-token-123", savedRefreshToken.Token);

        refreshTokenRepo.Verify(x => x.AddAsync(It.IsAny<RefreshToken>()), Times.Once);
        unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
        jwtService.Verify(x => x.GenerateAccessToken(validUser), Times.Once);
        jwtService.Verify(x => x.GenerateRefreshToken(), Times.Once);
    }

    [Fact]
    public async Task UT008_LogoutAsync_ShouldRevokeAllActiveTokensAndSignOut()
    {
        var (service, _, signInManager, _, unitOfWork, refreshTokenRepo, _) = CreateSut();
        var activeTokens = new List<RefreshToken>
        {
            new()
            {
                UserId = 2001,
                Token = "a1",
                ExpiryDate = DateTime.UtcNow.AddDays(1),
                CreatedAt = DateTime.UtcNow,
                IsRevoked = false
            },
            new()
            {
                UserId = 2001,
                Token = "a2",
                ExpiryDate = DateTime.UtcNow.AddDays(2),
                CreatedAt = DateTime.UtcNow,
                IsRevoked = false
            }
        };

        refreshTokenRepo.Setup(x => x.GetActiveTokensByUserIdAsync(2001)).ReturnsAsync(activeTokens);
        refreshTokenRepo.Setup(x => x.UpdateRangeAsync(It.IsAny<IEnumerable<RefreshToken>>())).Returns(Task.CompletedTask);
        signInManager.Setup(x => x.SignOutAsync()).Returns(Task.CompletedTask);
        unitOfWork.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        var result = await service.LogoutAsync(2001);

        Assert.True(result.Success);
        Assert.Equal("Logout successful", result.Message);
        Assert.All(activeTokens, token =>
        {
            Assert.True(token.IsRevoked);
            Assert.NotNull(token.RevokedAt);
        });

        refreshTokenRepo.Verify(
            x => x.UpdateRangeAsync(It.Is<IEnumerable<RefreshToken>>(tokens => tokens.Count() == 2)),
            Times.Once);
        unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
        signInManager.Verify(x => x.SignOutAsync(), Times.Once);
    }

    [Fact]
    public async Task UT009_ForgotPasswordAsync_ShouldHandleGroupedScenarios()
    {
        // Scenario 1: empty email returns generic success
        var (service1, _, _, _, _, _, _) = CreateSut();
        var emptyEmailResult = await service1.ForgotPasswordAsync(new ForgotPasswordRequest { Email = "  " });
        Assert.True(emptyEmailResult.Success);
        Assert.Contains("If the account exists", emptyEmailResult.Message);

        // Scenario 2: user not found also returns generic success
        var (service2, userManager2, _, _, _, _, _) = CreateSut();
        userManager2.Setup(x => x.FindByEmailAsync("nouser@test.local")).ReturnsAsync((User?)null);

        var notFoundResult = await service2.ForgotPasswordAsync(new ForgotPasswordRequest { Email = "nouser@test.local" });
        Assert.True(notFoundResult.Success);
        Assert.Contains("If the account exists", notFoundResult.Message);

        // Scenario 3: email sending throws
        var (service3, userManager3, _, _, _, _, emailService3) = CreateSut();
        var forgotUser1 = new User { Id = 3001, Email = "forgot1@test.local", UserName = "forgot1" };
        userManager3.Setup(x => x.FindByEmailAsync("forgot1@test.local")).ReturnsAsync(forgotUser1);
        userManager3.Setup(x => x.GeneratePasswordResetTokenAsync(forgotUser1)).ReturnsAsync("reset-token-1");
        emailService3
            .Setup(x => x.SendPasswordResetAsync(forgotUser1.Email!, forgotUser1.UserName!, It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("SMTP fail"));

        var sendFailedResult = await service3.ForgotPasswordAsync(new ForgotPasswordRequest { Email = "forgot1@test.local" });
        Assert.False(sendFailedResult.Success);
        Assert.Equal("Failed to send reset email. Please try again later.", sendFailedResult.Message);

        // Scenario 4: user exists and reset email is sent
        var (service4, userManager4, _, _, _, _, emailService4) = CreateSut();
        var forgotUser2 = new User { Id = 3002, Email = "forgot2@test.local", UserName = "forgot2" };
        userManager4.Setup(x => x.FindByEmailAsync("forgot2@test.local")).ReturnsAsync(forgotUser2);
        userManager4.Setup(x => x.GeneratePasswordResetTokenAsync(forgotUser2)).ReturnsAsync("reset-token-2");
        emailService4
            .Setup(x => x.SendPasswordResetAsync(forgotUser2.Email!, forgotUser2.UserName!, It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var successResult = await service4.ForgotPasswordAsync(new ForgotPasswordRequest { Email = "forgot2@test.local" });
        Assert.True(successResult.Success);
        Assert.Contains("If the account exists", successResult.Message);

        emailService4.Verify(
            x => x.SendPasswordResetAsync(
                forgotUser2.Email!,
                forgotUser2.UserName!,
                It.Is<string>(link => link.Contains("https://frontend.test.local/reset-password?userId=3002&token="))),
            Times.Once);
    }

    [Fact]
    public async Task UT010_ResetPasswordAsync_ShouldHandleGroupedScenarios()
    {
        // Scenario 1: user not found
        var (service1, userManager1, _, _, _, _, _) = CreateSut();
        userManager1.Setup(x => x.FindByIdAsync("4001")).ReturnsAsync((User?)null);

        var userMissingResult = await service1.ResetPasswordAsync(new ResetPasswordRequest
        {
            UserId = "4001",
            Token = "token",
            NewPassword = "NewP@ssw0rd!",
            ConfirmPassword = "NewP@ssw0rd!"
        });
        Assert.False(userMissingResult.Success);
        Assert.Equal("Invalid reset request.", userMissingResult.Message);

        // Scenario 2: base64 decode fails and service falls back to raw token
        var (service2, userManager2, _, _, _, _, _) = CreateSut();
        var user2 = new User { Id = 4002, Email = "reset2@test.local" };
        var invalidBase64Token = "***invalid-base64***";
        userManager2.Setup(x => x.FindByIdAsync("4002")).ReturnsAsync(user2);
        userManager2
            .Setup(x => x.ResetPasswordAsync(user2, invalidBase64Token, "NewP@ssw0rd!"))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Invalid token" }));

        var fallbackResult = await service2.ResetPasswordAsync(new ResetPasswordRequest
        {
            UserId = "4002",
            Token = invalidBase64Token,
            NewPassword = "NewP@ssw0rd!",
            ConfirmPassword = "NewP@ssw0rd!"
        });
        Assert.False(fallbackResult.Success);
        Assert.Contains("Invalid token", fallbackResult.Message);
        userManager2.Verify(
            x => x.ResetPasswordAsync(user2, invalidBase64Token, "NewP@ssw0rd!"),
            Times.Once);

        // Scenario 3: successful reset revokes active refresh tokens
        var (service3, userManager3, _, _, unitOfWork3, refreshTokenRepo3, _) = CreateSut();
        var user3 = new User { Id = 4003, Email = "reset3@test.local" };
        var activeTokens = new List<RefreshToken>
        {
            new()
            {
                UserId = 4003,
                Token = "r1",
                ExpiryDate = DateTime.UtcNow.AddDays(1),
                CreatedAt = DateTime.UtcNow
            }
        };
        userManager3.Setup(x => x.FindByIdAsync("4003")).ReturnsAsync(user3);
        userManager3
            .Setup(x => x.ResetPasswordAsync(user3, "decoded-token", "NewP@ssw0rd!"))
            .ReturnsAsync(IdentityResult.Success);
        refreshTokenRepo3.Setup(x => x.GetActiveTokensByUserIdAsync(4003)).ReturnsAsync(activeTokens);
        refreshTokenRepo3.Setup(x => x.UpdateRangeAsync(It.IsAny<IEnumerable<RefreshToken>>())).Returns(Task.CompletedTask);
        unitOfWork3.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        var successResult = await service3.ResetPasswordAsync(new ResetPasswordRequest
        {
            UserId = "4003",
            Token = EncodeToken("decoded-token"),
            NewPassword = "NewP@ssw0rd!",
            ConfirmPassword = "NewP@ssw0rd!"
        });

        Assert.True(successResult.Success);
        Assert.Equal("Password has been reset successfully. Please login again.", successResult.Message);
        Assert.All(activeTokens, token =>
        {
            Assert.True(token.IsRevoked);
            Assert.NotNull(token.RevokedAt);
        });

        refreshTokenRepo3.Verify(x => x.UpdateRangeAsync(It.IsAny<IEnumerable<RefreshToken>>()), Times.Once);
        unitOfWork3.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UT011_ChangePasswordAsync_ShouldHandleGroupedScenarios()
    {
        // Scenario 1: user not found
        var (service1, userManager1, _, _, _, _, _) = CreateSut();
        userManager1.Setup(x => x.FindByIdAsync("5001")).ReturnsAsync((User?)null);

        var userMissingResult = await service1.ChangePasswordAsync(5001, new ChangePasswordRequest
        {
            CurrentPassword = "OldP@ssw0rd!",
            NewPassword = "NewP@ssw0rd!",
            ConfirmPassword = "NewP@ssw0rd!"
        });
        Assert.False(userMissingResult.Success);
        Assert.Equal("User not found.", userMissingResult.Message);

        // Scenario 2: identity change password fails
        var (service2, userManager2, _, _, _, _, _) = CreateSut();
        var user2 = new User { Id = 5002, Email = "change2@test.local" };
        userManager2.Setup(x => x.FindByIdAsync("5002")).ReturnsAsync(user2);
        userManager2
            .Setup(x => x.ChangePasswordAsync(user2, "OldP@ssw0rd!", "NewP@ssw0rd!"))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Current password is incorrect" }));

        var failedResult = await service2.ChangePasswordAsync(5002, new ChangePasswordRequest
        {
            CurrentPassword = "OldP@ssw0rd!",
            NewPassword = "NewP@ssw0rd!",
            ConfirmPassword = "NewP@ssw0rd!"
        });
        Assert.False(failedResult.Success);
        Assert.Contains("Current password is incorrect", failedResult.Message);

        // Scenario 3: successful change revokes active refresh tokens
        var (service3, userManager3, _, _, unitOfWork3, refreshTokenRepo3, _) = CreateSut();
        var user3 = new User { Id = 5003, Email = "change3@test.local" };
        var activeTokens = new List<RefreshToken>
        {
            new()
            {
                UserId = 5003,
                Token = "cp1",
                ExpiryDate = DateTime.UtcNow.AddDays(1),
                CreatedAt = DateTime.UtcNow
            }
        };
        userManager3.Setup(x => x.FindByIdAsync("5003")).ReturnsAsync(user3);
        userManager3
            .Setup(x => x.ChangePasswordAsync(user3, "OldP@ssw0rd!", "NewP@ssw0rd!"))
            .ReturnsAsync(IdentityResult.Success);
        refreshTokenRepo3.Setup(x => x.GetActiveTokensByUserIdAsync(5003)).ReturnsAsync(activeTokens);
        refreshTokenRepo3.Setup(x => x.UpdateRangeAsync(It.IsAny<IEnumerable<RefreshToken>>())).Returns(Task.CompletedTask);
        unitOfWork3.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        var successResult = await service3.ChangePasswordAsync(5003, new ChangePasswordRequest
        {
            CurrentPassword = "OldP@ssw0rd!",
            NewPassword = "NewP@ssw0rd!",
            ConfirmPassword = "NewP@ssw0rd!"
        });

        Assert.True(successResult.Success);
        Assert.Equal("Password changed successfully. Please login again.", successResult.Message);
        Assert.All(activeTokens, token =>
        {
            Assert.True(token.IsRevoked);
            Assert.NotNull(token.RevokedAt);
        });

        refreshTokenRepo3.Verify(x => x.UpdateRangeAsync(It.IsAny<IEnumerable<RefreshToken>>()), Times.Once);
        unitOfWork3.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    private static string EncodeToken(string token)
    {
        return WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
    }

    private static (
        AuthService Service,
        Mock<UserManager<User>> UserManager,
        Mock<SignInManager<User>> SignInManager,
        Mock<IJwtService> JwtService,
        Mock<IUnitOfWork> UnitOfWork,
        Mock<IRefreshTokenRepository> RefreshTokenRepository,
        Mock<IEmailService> EmailService) CreateSut()
    {
        var userManagerMock = CreateUserManagerMock();
        var signInManagerMock = CreateSignInManagerMock(userManagerMock);
        var jwtServiceMock = new Mock<IJwtService>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        var emailServiceMock = new Mock<IEmailService>();

        unitOfWorkMock.SetupGet(x => x.RefreshTokens).Returns(refreshTokenRepositoryMock.Object);
        unitOfWorkMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AppUrl"] = "https://app.test.local",
                ["FrontendUrl"] = "https://frontend.test.local"
            })
            .Build();

        var jwtSettings = Options.Create(new JwtSettings
        {
            AccessTokenExpirationMinutes = 60,
            RefreshTokenExpirationDays = 7
        });

        var service = new AuthService(
            userManagerMock.Object,
            signInManagerMock.Object,
            jwtServiceMock.Object,
            unitOfWorkMock.Object,
            emailServiceMock.Object,
            configuration,
            jwtSettings);

        return (
            service,
            userManagerMock,
            signInManagerMock,
            jwtServiceMock,
            unitOfWorkMock,
            refreshTokenRepositoryMock,
            emailServiceMock);
    }

    private static Mock<UserManager<User>> CreateUserManagerMock()
    {
        var store = new Mock<IUserStore<User>>();

        return new Mock<UserManager<User>>(
            store.Object,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!);
    }

    private static Mock<SignInManager<User>> CreateSignInManagerMock(Mock<UserManager<User>> userManagerMock)
    {
        var contextAccessor = new Mock<IHttpContextAccessor>();
        var claimsFactory = new Mock<IUserClaimsPrincipalFactory<User>>();
        var options = Options.Create(new IdentityOptions());
        var logger = new Mock<ILogger<SignInManager<User>>>();
        var schemes = new Mock<IAuthenticationSchemeProvider>();
        var confirmation = new Mock<IUserConfirmation<User>>();

        return new Mock<SignInManager<User>>(
            userManagerMock.Object,
            contextAccessor.Object,
            claimsFactory.Object,
            options,
            logger.Object,
            schemes.Object,
            confirmation.Object);
    }
}
