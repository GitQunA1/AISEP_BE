using AISEP.DTOs;

namespace AISEP.Services
{

    public interface ICurrentUserService
    {

        Guid GetUserId();

      
        string GetUserEmail();

      
        string GetUserName();

      
        string GetUserRole();

      
        bool IsAuthenticated();

      
        
    }
}
