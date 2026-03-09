using AISEP.Models.Entities;

namespace AISEP.Repositories.Bookings
{
    public interface IBookingRepository
    {

        Task<Booking?> GetByIdAsync(int id);
        Task AddAsync(Booking booking);
        Task DeleteAsync(int id);
        IQueryable<Booking> GetBookingQuery();

    }
}
