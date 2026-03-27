using AISEP.DAL.Entities;

namespace AISEP.DAL.Repositories.BookingSlots
{
    public interface IBookingSlotRepository
    {
        Task AddRangeAsync(IEnumerable<BookingSlot> bookingSlots);
    }
}
