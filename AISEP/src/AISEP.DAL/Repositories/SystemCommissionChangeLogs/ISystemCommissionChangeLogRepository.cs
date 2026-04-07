using AISEP.DAL.Entities;

namespace AISEP.DAL.Repositories.SystemCommissionChangeLogs
{
    public interface ISystemCommissionChangeLogRepository
    {
        IQueryable<SystemCommissionChangeLog> GetQuery();
        Task AddAsync(SystemCommissionChangeLog log);
    }
}
