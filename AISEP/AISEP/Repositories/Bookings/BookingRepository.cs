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

      

        public async Task DeleteAsync(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking != null)
            {
                _context.Bookings.Remove(booking);
            }
        }

        public async Task<IEnumerable<Booking>> GetBookingsByAdvisorIdAsync(int advisorId)
        {
            return await _context.Bookings
                .Include(b => b.Advisor)
                    .ThenInclude(a => a.User)
                .Include(b => b.Customer)
                .Where(b => b.AdvisorId == advisorId)
                .OrderByDescending(b => b.StartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Booking>> GetBookingsByCustomerIdAsync(int customerId)
        {
            return await _context.Bookings
                .Include(b => b.Advisor)
                    .ThenInclude(a => a.User)
                .Include(b => b.Customer)
                .Where(b => b.CustomerId == customerId)
                .OrderByDescending(b => b.StartTime)
                .ToListAsync();
        }

       
        public IQueryable<Booking> GetQueryable()
        {
            return _context.Bookings
                .Include(b => b.Advisor)
                    .ThenInclude(a => a.User)
                .Include(b => b.Customer)
                .AsNoTracking();
        }
    }
}
