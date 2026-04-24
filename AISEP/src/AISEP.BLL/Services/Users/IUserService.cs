using AISEP.BLL.DTOs.Responses;
using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.Helpers;
using Sieve.Models;

namespace AISEP.BLL.Services.Users
{
    public interface IUserService
    {
        int GetUserId();
        string? GetUserEmail();
        string? GetUserName();
        string? GetUserRole();
        bool IsAuthenticated();
        Task<PagedResult<UserResponse>> GetAllAsync(SieveModel model);
        Task<UserResponse?> GetByIdAsync(int id);
        Task<UserResponse?> UpdateAsync(int id, UpdateUserRequest request);
        Task<bool> DeleteAsync(int id);
        Task<bool> UnbanAsync(int id);
        Task<UserResponse> GetByProjectId(int id);
        Task<int> GetBonusFreeBookingsAsync(int id);
    }
}
