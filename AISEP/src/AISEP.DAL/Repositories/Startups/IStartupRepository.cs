using AISEP.DAL.Entities;
using AISEP.DAL.Enums;

namespace AISEP.DAL.Repositories.Startups
{
    public interface IStartupRepository
    {
        IQueryable<Startup> SearchStartupsQuery(string? industry = null, string? stage = null, string? searchTerm = null);
        IQueryable<Startup> GetByStatusQuery(ApprovalStatus? status = null);
        Task<Startup?> GetByIdAsync(int id);
        Task<Startup?> GetByUserIdAsync(int userId);
        IQueryable<Startup> GetStartupQuery();
        IQueryable<Startup> GetPendingStartupsQuery();
        Task AddAsync(Startup startup);
        void Update(Startup startup);
    }
}
