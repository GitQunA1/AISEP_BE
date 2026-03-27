using AISEP.DAL.Data;
using AISEP.DAL.Entities;

namespace AISEP.DAL.Repositories.BookingSlots
{
    public class BookingSlotRepository : IBookingSlotRepository
    {
        private readonly ApplicationDbContext _context;

        public BookingSlotRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddRangeAsync(IEnumerable<BookingSlot> bookingSlots)
            => await _context.BookingSlots.AddRangeAsync(bookingSlots);
    }
}
