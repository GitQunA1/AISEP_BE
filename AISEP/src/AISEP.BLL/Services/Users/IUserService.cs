using AISEP.BLL.DTOs.Responses;

namespace AISEP.BLL.Services.Users
{
    public interface IUserService
    {
        int GetUserId();
        string? GetUserEmail();
        string? GetUserName();
        string? GetUserRole();
        bool IsAuthenticated();
        Task<UserResponse> GetByProjectId(int id);
    }
}
