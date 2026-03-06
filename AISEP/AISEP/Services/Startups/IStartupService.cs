using AISEP.DTOs;
using AISEP.DTOs.Requests;
using AISEP.DTOs.Responses;
using AISEP.Models.Enums;
using Sieve.Models;

namespace AISEP.Services.Startups
{
    public interface IStartupService
    {
       
        Task<PagedResult<StartupResponse>> GetAllStartupsAsync(SieveModel model);
        Task<PagedResult<StartupResponse>> GetStartupsByStatusAsync(SieveModel model, ApprovalStatus? status = null);
        Task<PagedResult<StartupResponse>> SearchStartupsAsync(SieveModel model, string? industry = null, DevelopmentStage? stage = null);
        Task<StartupResponse?> GetStartupByIdAsync(int id);

     
        Task<StartupResponse> CreateStartupAsync(int userId, CreateStartupRequest dto);
        Task ApproveStartupAsync(int userId);
        //Task<StartupResponseDto?> GetMyProfileAsync(int userId);

    
        //Task<PagedResultDto<StartupResponseDto>> GetPendingStartupsAsync(SieveModel model);
        //Task ReviewStartupAsync(int startupId, ReviewStartupDto dto);
    }
}
