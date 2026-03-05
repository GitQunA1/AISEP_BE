using AISEP.DTOs;
using AISEP.Models.Enums;
using Sieve.Models;

namespace AISEP.Services.Startups
{
    public interface IStartupService
    {
       
        Task<PagedResultDto<StartupResponseDto>> GetAllStartupsAsync(SieveModel model);
        Task<PagedResultDto<StartupResponseDto>> GetStartupsByStatusAsync(SieveModel model, ApprovalStatus? status = null);
        Task<PagedResultDto<StartupResponseDto>> SearchStartupsAsync(SieveModel model, string? industry = null, DevelopmentStage? stage = null);
        Task<StartupResponseDto?> GetStartupByIdAsync(int id);

     
        Task<StartupResponseDto> CreateStartupAsync(int userId, CreateStartupDto dto);
        Task ApproveStartupAsync(int userId);
        //Task<StartupResponseDto?> GetMyProfileAsync(int userId);

    
        //Task<PagedResultDto<StartupResponseDto>> GetPendingStartupsAsync(SieveModel model);
        //Task ReviewStartupAsync(int startupId, ReviewStartupDto dto);
    }
}
