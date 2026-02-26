using AISEP.DTOs;
using AISEP.Models.Enums;
using Sieve.Models;

namespace AISEP.Services.Startups
{
    public interface IStartupService
    {
        Task<PagedResultDto<StartupResponseDto>> SearchStartupsAsync(SieveModel model, string? industry = null, DevelopmentStage? stage = null);
        Task<StartupResponseDto?> GetStartupByIdAsync(Guid id);
        Task<PagedResultDto<StartupResponseDto>> GetAllStartupsAsync(SieveModel model);
    }
}
