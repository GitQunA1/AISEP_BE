using AISEP.Models.Entities;
using System.Security.Claims;

namespace AISEP.Services.Jwt
{
    public interface IJwtService
    {
        string GenerateAccessToken(User user);
        string GenerateRefreshToken();
        
    }
}
