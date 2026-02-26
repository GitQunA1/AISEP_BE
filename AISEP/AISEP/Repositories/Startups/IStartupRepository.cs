using AISEP.DTOs;
using AISEP.Models.Entities;
using AISEP.Models.Enums;

namespace AISEP.Repositories.Startups
{
    public interface IStartupRepository
    {
        IQueryable<Startup> SearchStartupsQuery(string? industry = null, DevelopmentStage? stage = null, string? searchTerm = null);
        Task<Startup?> GetByIdAsync(int id);
        IQueryable<Startup> GetStartupQuery();
    }
}
