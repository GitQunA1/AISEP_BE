using AISEP.Models;
using System.Security.Claims;

namespace AISEP.Services
{
    public interface IJwtService
    {
        string GenerateAccessToken(User user);
        string GenerateRefreshToken();
        
    }
}
