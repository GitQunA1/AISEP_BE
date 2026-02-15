using AISEP.DTOs;
using AISEP.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace AISEP.Services
{
    /// <summary>
    /// Service implementation để lấy thông tin user hiện tại từ JWT claims
    /// </summary>
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<User> _userManager;

        public CurrentUserService(
            IHttpContextAccessor httpContextAccessor,
            UserManager<User> userManager)
        {
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
        }

        /// <summary>
        /// Lấy User ID từ JWT claims
        /// </summary>
        public Guid GetUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) {
                throw new UnauthorizedAccessException("User not authenticated");
            }


            return Guid.Parse(userIdClaim) ;
        }

        /// <summary>
        /// Lấy Email từ JWT claims
        /// </summary>
        public string GetUserEmail()
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Email);
        }

        /// <summary>
        /// Lấy Username từ JWT claims
        /// </summary>
        public string GetUserName()
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Name);
        }

        /// <summary>
        /// Lấy Role từ JWT claims
        /// </summary>
        public string GetUserRole()
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Role);
        }

        /// <summary>
        /// Kiểm tra user đã đăng nhập chưa
        /// </summary>
        public bool IsAuthenticated()
        {
            return _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
        }

        /// <summary>
        /// Lấy toàn bộ thông tin user từ database
        /// </summary>
      
    }
}
