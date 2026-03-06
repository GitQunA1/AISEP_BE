using AISEP.DTOs;

namespace AISEP.Services.CurrentUser
{

    public interface ICurrentUserService
    {

        int GetUserId();

      
        string? GetUserEmail();

        string? GetUserName();

        string? GetUserRole();

      
        bool IsAuthenticated();

      
        
    }
}
