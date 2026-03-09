using AISEP.DTOs.Responses;

namespace AISEP.Services.Users
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
