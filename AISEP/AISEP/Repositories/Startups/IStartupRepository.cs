using AISEP.Models.Entities;
using AISEP.Models.Enums;

namespace AISEP.Repositories.Startups
{
    public interface IStartupRepository
    {
        IQueryable<Startup> SearchStartupsQuery(string? industry = null, DevelopmentStage? stage = null, string? searchTerm = null);
        IQueryable<Startup> GetByStatusQuery(ApprovalStatus? status = null);
        Task<Startup?> GetByIdAsync(int id);
        Task<Startup?> GetByUserIdAsync(int userId);
        IQueryable<Startup> GetStartupQuery();
        IQueryable<Startup> GetPendingStartupsQuery();
        Task AddAsync(Startup startup);
        void Update(Startup startup);
    }
}
