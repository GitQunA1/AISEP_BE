using AISEP.DAL.Common;
using AISEP.BLL.DTOs.Responses;
using AISEP.DAL.Entities;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace AISEP.BLL.Services.Users
{
    public class UserService : IUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<User> _userManager;
        private IUnitOfWork _unitOfWork;

        public UserService(
            IHttpContextAccessor httpContextAccessor,
            UserManager<User> userManager, IUnitOfWork unitOfWork)
        {
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
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

        public async Task<UserResponse> GetByProjectId(int id)
        {
            var user = await _unitOfWork.Users.GetByProjectId(id);
            if (user == null)
                throw new KeyNotFoundException("User not found");

            return new UserResponse
            {
                UserId      = user.Id,
                UserName    = user.UserName,
                Email       = user.Email,
                Role        = user.Role,
                Status      = user.Status,
                DateOfBirth = user.DateOfBirth,
                PhoneNumber = user.PhoneNumber
            };
        }
    }
}
