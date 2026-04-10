using AISEP.DAL.Entities;

namespace AISEP.DAL.Repositories.PremiumFreeBookingUsageLogs
{
    public interface IPremiumFreeBookingUsageLogRepository
    {
        Task AddAsync(PremiumFreeBookingUsageLog log);
        IQueryable<PremiumFreeBookingUsageLog> GetByUserIdQuery(int userId);
    }
}
