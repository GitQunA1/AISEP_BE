using AISEP.DAL.Data;
using AISEP.DAL.Entities;
using AISEP.DAL.Enums;
using Microsoft.EntityFrameworkCore;

namespace AISEP.DAL.Repositories.Bookings
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
                .Include(b => b.ChatSession)
                .Include(b => b.ConsultingReport)
                .FirstOrDefaultAsync(b => b.BookingId == id);
        }

        public async Task<Booking?> GetByIdWithAdvisorWalletAsync(int id)
            => await _context.Bookings
                .Include(b => b.Advisor)
                    .ThenInclude(a => a.Wallet)
                .FirstOrDefaultAsync(b => b.BookingId == id);

        public async Task<Booking?> GetPendingByIdAndCustomerAsync(int bookingId, int customerId)
            => await _context.Bookings
                .FirstOrDefaultAsync(b => b.BookingId == bookingId
                                       && b.CustomerId == customerId
                                       && b.Status == BookingStatus.Pending);

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
