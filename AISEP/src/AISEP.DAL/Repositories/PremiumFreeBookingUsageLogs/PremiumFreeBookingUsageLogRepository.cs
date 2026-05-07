using AISEP.DAL.Data;
using AISEP.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace AISEP.DAL.Repositories.PremiumFreeBookingUsageLogs
{
    public class PremiumFreeBookingUsageLogRepository : IPremiumFreeBookingUsageLogRepository
    {
        private readonly ApplicationDbContext _context;

        public PremiumFreeBookingUsageLogRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(PremiumFreeBookingUsageLog log)
            => await _context.PremiumFreeBookingUsageLogs.AddAsync(log);

        public async Task<PremiumFreeBookingUsageLog?> GetByBookingIdAsync(int bookingId)
            => await _context.PremiumFreeBookingUsageLogs
                .FirstOrDefaultAsync(x => x.BookingId == bookingId);

        public IQueryable<PremiumFreeBookingUsageLog> GetByUserIdQuery(int userId)
            => _context.PremiumFreeBookingUsageLogs
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.UsedAt)
                .AsNoTracking();
    }
}
