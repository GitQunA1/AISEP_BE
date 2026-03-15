using AISEP.DAL.Common;
using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;
using AISEP.BLL.Helpers;
using AISEP.DAL.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
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

        public async Task<UserResponse?> GetByIdAsync(int id)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            return user is null ? null : _mapper.Map<UserResponse>(user);
        }

        public async Task<UserResponse?> UpdateAsync(int id, UpdateUserRequest request)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user is null)
                return null;

            user.UserName = string.IsNullOrWhiteSpace(request.UserName)
                ? user.UserName
                : request.UserName;

            user.DateOfBirth = request.DateOfBirth ?? user.DateOfBirth;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to update user: {errors}");
            }

            return _mapper.Map<UserResponse>(user);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user is null)
                return false;

            user.Status = DAL.Enums.UserStatus.Banned;

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
    }
}
