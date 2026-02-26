using AISEP.Data;
using AISEP.Models.Entities;
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

        public async Task<Booking?> GetByIdAsync(int id)
        {
            return await _context.Bookings
                .Include(b => b.Advisor)
                    .ThenInclude(a => a.User)
                .Include(b => b.Customer)
                .Include(b => b.ChatSessions)
                .Include(b => b.ConsultingReports)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task AddAsync(Booking booking)
        {
            await _context.Bookings.AddAsync(booking);
        }

        
      

        public async Task DeleteAsync(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking != null)
            {
                _context.Bookings.Remove(booking);
            }
        }

        public IQueryable<Booking> GetBookingQuery()
        {
            return _context.Bookings
                .Include(b => b.Advisor)
                    .ThenInclude(a => a.User)
                .Include(b => b.Customer)
                .AsNoTracking();
        }
    }
}
