using AISEP.DTOs;
using AISEP.Models.Entities;
using AISEP.Models.Enums;

namespace AISEP.Repositories.Startups
{
    public interface IStartupRepository
    {
        IQueryable<Startup> SearchStartupsQuery(string? industry = null, DevelopmentStage? stage = null);
        Task<Startup?> GetByIdAsync(Guid id);
        IQueryable<Startup> GetStartupQuery();
    }
}
