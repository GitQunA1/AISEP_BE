using AISEP.DAL.Common;
using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;
using AISEP.BLL.Helpers;
using AISEP.DAL.Entities;
using AISEP.DAL.Enums;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using Sieve.Services;
using System.Security.Claims;

namespace AISEP.BLL.Services.Users
{
    public class UserService : IUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<User> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISieveProcessor _sieveProcessor;
        private readonly IMapper _mapper;

        public UserService(
            IHttpContextAccessor httpContextAccessor,
            UserManager<User> userManager,
            IUnitOfWork unitOfWork,
            ISieveProcessor sieveProcessor,
            IMapper mapper)
        {
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _sieveProcessor = sieveProcessor;
            _mapper = mapper;
        }

        public int GetUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                throw new UnauthorizedAccessException("User not authenticated");

            return int.Parse(userIdClaim);
        }

        public string? GetUserEmail()
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Email);
        }

        public string? GetUserName()
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Name);
        }

        public string? GetUserRole()
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Role);
        }

        public bool IsAuthenticated()
        {
            return _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
        }

        public async Task<PagedResult<UserResponse>> GetAllAsync(SieveModel model)
        {
            var query = _unitOfWork.Users.GetAllQuery();
            return await PaginationHelper.PaginateAsync(query, model, _sieveProcessor,
                u => _mapper.Map<UserResponse>(u));
        }

        public async Task<PagedResult<AdminUserResponse>> GetAllForAdminAsync(SieveModel model)
        {
            var query = _unitOfWork.Users.GetAllQuery();
            return await PaginationHelper.PaginateAsync(query, model, _sieveProcessor,
                u => _mapper.Map<AdminUserResponse>(u));
        }

        public async Task<UserResponse?> GetByIdAsync(int id)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            return user is null ? null : _mapper.Map<UserResponse>(user);
        }

        public async Task<AdminUserResponse?> GetByIdForAdminAsync(int id)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            return user is null ? null : _mapper.Map<AdminUserResponse>(user);
        }

        public async Task<AdminUserResponse> CreateForAdminAsync(AdminCreateUserRequest request)
        {
            var userName = request.UserName.Trim();
            var fullName = request.FullName.Trim();
            var email = request.Email.Trim();
            var phoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber)
                ? null
                : request.PhoneNumber.Trim();

            await EnsureUserNameIsUniqueAsync(userName);
            await EnsureEmailIsUniqueAsync(email);

            var user = new User
            {
                UserName = userName,
                FullName = fullName,
                Email = email,
                PhoneNumber = phoneNumber,
                Role = UserRole.Staff,
                Status = request.Status,
                IsPremium = false,
                EmailConfirmed = true,
                DateOfBirth = request.DateOfBirth.HasValue
                    ? NormalizeDateOfBirthToUtc(request.DateOfBirth.Value)
                    : null,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to create user: {errors}");
            }

            return _mapper.Map<AdminUserResponse>(user);
        }

        public async Task<UserResponse?> UpdateAsync(int id, UpdateUserRequest request)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user is null)
                return null;

            if (request.UserName is not null)
            {
                user.UserName = request.UserName.Trim();
            }

            if (request.FullName is not null)
            {
                user.FullName = request.FullName.Trim();
            }

            if (request.DateOfBirth.HasValue)
            {
                user.DateOfBirth = NormalizeDateOfBirthToUtc(request.DateOfBirth.Value);
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to update user: {errors}");
            }

            return _mapper.Map<UserResponse>(user);
        }

        public async Task<AdminUserResponse?> UpdateForAdminAsync(int id, AdminUpdateUserRequest request)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user is null)
            {
                return null;
            }

            if (request.UserName is not null)
            {
                var userName = request.UserName.Trim();
                await EnsureUserNameIsUniqueAsync(userName, user.Id);
                user.UserName = userName;
            }

            if (request.FullName is not null)
            {
                user.FullName = request.FullName.Trim();
            }

            if (request.Email is not null)
            {
                var email = request.Email.Trim();
                await EnsureEmailIsUniqueAsync(email, user.Id);
                user.Email = email;
                user.EmailConfirmed = true;
            }

            if (request.PhoneNumber is not null)
            {
                user.PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber)
                    ? null
                    : request.PhoneNumber.Trim();
            }

            if (request.Role.HasValue)
            {
                user.Role = request.Role.Value;
            }

            if (request.Status.HasValue)
            {
                user.Status = request.Status.Value;
            }

            if (request.IsPremium.HasValue)
            {
                user.IsPremium = request.IsPremium.Value;
            }

            if (request.DateOfBirth.HasValue)
            {
                user.DateOfBirth = NormalizeDateOfBirthToUtc(request.DateOfBirth.Value);
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to update user: {errors}");
            }

            return _mapper.Map<AdminUserResponse>(user);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user is null)
                return false;

            user.Status = UserStatus.Banned;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to update user: {errors}");
            }
            return true;
        }

        public async Task<bool> DeleteForAdminAsync(int id)
        {
            var currentUserId = GetUserId();
            if (currentUserId == id)
            {
                throw new InvalidOperationException("You cannot delete your own account.");
            }

            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user is null)
            {
                return false;
            }

            try
            {
                var result = await _userManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Failed to delete user permanently: {errors}");
                }
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException(
                    "Failed to delete user permanently because this account has related data.",
                    ex);
            }

            return true;
        }

        public async Task<bool> UnbanAsync(int id)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user is null)
                return false;

            user.Status = UserStatus.Active;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to update user: {errors}");
            }

            return true;
        }

        public async Task<UserResponse> GetByProjectId(int id)
        {
            var user = await _unitOfWork.Users.GetByProjectId(id);
            if (user == null)
                throw new KeyNotFoundException("User not found");

            return _mapper.Map<UserResponse>(user);
        }

        public async Task<int> GetBonusFreeBookingsAsync(int id)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user == null)
                throw new KeyNotFoundException("User not found");

            return user.BonusFreeBookings;
        }

        private static DateTime NormalizeDateOfBirthToUtc(DateTime dateOfBirth)
        {
            var dateOnly = dateOfBirth.Date;
            return DateTime.SpecifyKind(dateOnly, DateTimeKind.Utc);
        }

        private async Task EnsureEmailIsUniqueAsync(string email, int? excludingUserId = null)
        {
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null && (!excludingUserId.HasValue || existingUser.Id != excludingUserId.Value))
            {
                throw new InvalidOperationException("Email already registered.");
            }
        }

        private async Task EnsureUserNameIsUniqueAsync(string userName, int? excludingUserId = null)
        {
            var existingUser = await _userManager.FindByNameAsync(userName);
            if (existingUser != null && (!excludingUserId.HasValue || existingUser.Id != excludingUserId.Value))
            {
                throw new InvalidOperationException("Username already exists.");
            }
        }
    }
}
