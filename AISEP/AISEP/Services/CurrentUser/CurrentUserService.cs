using AISEP.DTOs;
using AISEP.Models.Entities;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace AISEP.Services.CurrentUser
{
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

      
        public int GetUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) {
                throw new UnauthorizedAccessException("User not authenticated");
            }


            return int.Parse(userIdClaim) ;
        }

        
        public string GetUserEmail()
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Email);
        }

     
        public string GetUserName()
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Name);
        }

        public string GetUserRole()
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Role);
        }

        public bool IsAuthenticated()
        {
            return _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
        }

       
        
    }
}
