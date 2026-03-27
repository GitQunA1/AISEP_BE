using AISEP.DAL.Data;
using AISEP.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace AISEP.DAL.Repositories.ConsultingReports
{
    public class ConsultingReportRepository : IConsultingReportRepository
    {
        private readonly ApplicationDbContext _context;

        public ConsultingReportRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ConsultingReport?> GetByIdAsync(int id)
        {
            return await _context.ConsultingReports
                .Include(r => r.Booking)
                    .ThenInclude(b => b.Advisor)
                        .ThenInclude(a => a.User)
                .Include(r => r.Booking)
                    .ThenInclude(b => b.Customer)
                .FirstOrDefaultAsync(r => r.ConsultingReportId == id);
        }

        public async Task<ConsultingReport?> GetByBookingIdAsync(int bookingId)
        {
            return await _context.ConsultingReports
                .Include(r => r.Booking)
                    .ThenInclude(b => b.Advisor)
                        .ThenInclude(a => a.User)
                .Include(r => r.Booking)
                    .ThenInclude(b => b.Customer)
                .FirstOrDefaultAsync(r => r.BookingId == bookingId);
        }

        public async Task AddAsync(ConsultingReport report)
        {
            await _context.ConsultingReports.AddAsync(report);
        }
    }
}
