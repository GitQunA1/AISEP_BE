using AISEP.DAL.Entities;

namespace AISEP.DAL.Repositories.PremiumFreeBookingUsageLogs
{
    public interface IPremiumFreeBookingUsageLogRepository
    {
        Task AddAsync(PremiumFreeBookingUsageLog log);
        Task<PremiumFreeBookingUsageLog?> GetByBookingIdAsync(int bookingId);
        IQueryable<PremiumFreeBookingUsageLog> GetByUserIdQuery(int userId);
    }
}
