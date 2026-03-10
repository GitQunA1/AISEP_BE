using AISEP.DAL.Entities;
using System.Security.Claims;

namespace AISEP.BLL.Services.Jwt
{
    public interface IJwtService
    {
        string GenerateAccessToken(User user);
        string GenerateRefreshToken();
        
    }
}
