using AISEP.Data;
using AISEP.Models;
using AISEP.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace AISEP.Repositories.Bookings
{
    public class BookingRepository : IBookingRepository
    {
        private readonly ApplicationDbContext _context;

        public BookingRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Booking?> GetByIdAsync(Guid id)
        {
            return await _context.Bookings
                .Include(b => b.Advisor)
                    .ThenInclude(a => a.User)
                .Include(b => b.Customer)
                .Include(b => b.ChatSessions)
                .Include(b => b.ConsultingReports)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<IEnumerable<Booking>> GetAllAsync()
        {
            return await _context.Bookings
                .Include(b => b.Advisor)
                    .ThenInclude(a => a.User)
                .Include(b => b.Customer)
                .OrderByDescending(b => b.StartTime)
                .ToListAsync();
        }

        public async Task AddAsync(Booking booking)
        {
            await _context.Bookings.AddAsync(booking);
        }

        public async Task UpdateAsync(Booking booking)
        {
            _context.Bookings.Update(booking);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(Guid id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking != null)
            {
                _context.Bookings.Remove(booking);
            }
        }

        public async Task<IEnumerable<Booking>> GetBookingsByAdvisorIdAsync(Guid advisorId)
        {
            return await _context.Bookings
                .Include(b => b.Advisor)
                    .ThenInclude(a => a.User)
                .Include(b => b.Customer)
                .Where(b => b.AdvisorId == advisorId)
                .OrderByDescending(b => b.StartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Booking>> GetBookingsByCustomerIdAsync(Guid customerId)
        {
            return await _context.Bookings
                .Include(b => b.Advisor)
                    .ThenInclude(a => a.User)
                .Include(b => b.Customer)
                .Where(b => b.CustomerId == customerId)
                .OrderByDescending(b => b.StartTime)
                .ToListAsync();
        }

        public async Task<Booking?> GetBookingWithDetailsAsync(Guid bookingId)
        {
            return await _context.Bookings
                .Include(b => b.Advisor)
                    .ThenInclude(a => a.User)
                .Include(b => b.Customer)
                .Include(b => b.ChatSessions)
                .Include(b => b.ConsultingReports)
                .FirstOrDefaultAsync(b => b.Id == bookingId);
        }

        public async Task<bool> IsAdvisorAvailableAsync(Guid advisorId, DateTime startTime, DateTime endTime)
        {
            return !await _context.Bookings.AnyAsync(b =>
                b.AdvisorId == advisorId &&
                b.Status != BookingStatus.Cancelled &&
                ((b.StartTime <= startTime && b.EndTime > startTime) ||
                 (b.StartTime < endTime && b.EndTime >= endTime) ||
                 (b.StartTime >= startTime && b.EndTime <= endTime))
            );
        }
    }
}
