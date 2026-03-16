using AISEP.BLL.Helpers;
using AISEP.DAL.Common;
using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;
using Sieve.Models;

namespace AISEP.BLL.Services.Startups
{
    public interface IStartupService
    {
       
        Task<PagedResult<StartupResponse>> GetAllStartupsAsync(SieveModel model);
        Task<PagedResult<StartupResponse>> GetStartupsByStatusAsync(SieveModel model, string? status = null);
        Task<PagedResult<StartupResponse>> SearchStartupsAsync(SieveModel model, string? industry = null, string? stage = null);
        Task<StartupResponse?> GetStartupByIdAsync(int id);

     
        Task<StartupResponse> CreateStartupAsync( CreateStartupRequest dto);
        Task<StartupResponse> UpdateStartupAsync(int id,UpdateStartupRequest dto);
        Task ApproveStartupAsync(int startupId);
        Task RejectStartupAsync(int startupId, RejectStartupRequest dto);
       
        Task<StartupResponse?> GetMyProfileAsync();
    
        //Task<PagedResultDto<StartupResponseDto>> GetPendingStartupsAsync(SieveModel model);
        //Task ReviewStartupAsync(int startupId, ReviewStartupDto dto);
    }
}
