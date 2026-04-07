using AISEP.DAL.Entities;

namespace AISEP.DAL.Repositories.SystemCommissionConfigs
{
    public interface ISystemCommissionConfigRepository
    {
        Task<SystemCommissionConfig?> GetCurrentAsync(DateTime asOfUtc);
        Task<SystemCommissionConfig?> GetActiveAsync();
        IQueryable<SystemCommissionConfig> GetQuery();
        Task AddAsync(SystemCommissionConfig config);
        void Update(SystemCommissionConfig config);
    }
}
