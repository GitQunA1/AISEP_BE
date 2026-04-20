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
                .Include(b => b.Project)
                .Include(b => b.Customer)
                .Include(b => b.SystemCommissionConfig)
                .Include(b => b.BookingSlots)
                    .ThenInclude(bs => bs.AdvisorAvailability)
                .Include(b => b.ChatSession)
                .Include(b => b.ConsultingReport)
                .Include(b => b.Review)
                .FirstOrDefaultAsync(b => b.BookingId == id);
        }

        public async Task<Booking?> GetByIdForAdvisorActionAsync(int id)
            => await _context.Bookings
                .Include(b => b.Advisor)
                .Include(b => b.Project)
                .Include(b => b.BookingSlots)
                    .ThenInclude(bs => bs.AdvisorAvailability)
                .FirstOrDefaultAsync(b => b.BookingId == id);

        public async Task<Booking?> GetByIdWithAdvisorWalletAsync(int id)
            => await _context.Bookings
                .Include(b => b.Advisor)
                    .ThenInclude(a => a.User)
                .Include(b => b.Advisor)
                    .ThenInclude(a => a.Wallet)
                .FirstOrDefaultAsync(b => b.BookingId == id);

        public async Task<Booking?> GetPayableByIdAndCustomerAsync(int bookingId, int customerId)
            => await _context.Bookings
                .FirstOrDefaultAsync(b => b.BookingId == bookingId
                                       && b.CustomerId == customerId
                                       && b.Status == BookingStatus.ApprovedAwaitingPayment);

        public async Task<List<Booking>> GetExpiredAwaitingAdvisorResponseAsync(DateTime thresholdUtc)
            => await _context.Bookings
                .Include(b => b.Project)
                .Include(b => b.BookingSlots)
                    .ThenInclude(bs => bs.AdvisorAvailability)
                .Where(b => b.Status == BookingStatus.Pending
                         && b.CreatedAt <= thresholdUtc)
                .ToListAsync();

        public async Task<bool> ExistsFreeRebookFromComplaintByOldBookingIdAsync(int oldBookingId)
            => await _context.Bookings.AnyAsync(b =>
                b.OldBookingId == oldBookingId
                && b.IsFreeRebookFromComplaint);

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
                .Include(b => b.Project)
                .Include(b => b.Customer)
                .Include(b => b.SystemCommissionConfig)
                .Include(b => b.BookingSlots)
                    .ThenInclude(bs => bs.AdvisorAvailability)
                .AsNoTracking();
        }
    }
}
