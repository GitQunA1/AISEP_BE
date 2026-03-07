using AISEP.Common;
using AISEP.DTOs.Requests;
using AISEP.DTOs.Responses;
using Sieve.Models;

namespace AISEP.Services.Startups
{
    public interface IStartupService
    {
       
        Task<PagedResult<StartupResponse>> GetAllStartupsAsync(SieveModel model);
        Task<PagedResult<StartupResponse>> GetStartupsByStatusAsync(SieveModel model, string? status = null);
        Task<PagedResult<StartupResponse>> SearchStartupsAsync(SieveModel model, string? industry = null, string? stage = null);
        Task<StartupResponse?> GetStartupByIdAsync(int id);

     
        Task<StartupResponse> CreateStartupAsync(int userId, CreateStartupRequest dto);
        Task ApproveStartupAsync(int userId);
        //Task<StartupResponseDto?> GetMyProfileAsync(int userId);

    
        //Task<PagedResultDto<StartupResponseDto>> GetPendingStartupsAsync(SieveModel model);
        //Task ReviewStartupAsync(int startupId, ReviewStartupDto dto);
    }
}
