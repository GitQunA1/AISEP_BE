using AISEP.DAL.Entities;

namespace AISEP.DAL.Repositories.Bookings
{
    public interface IBookingRepository
    {

        Task<Booking?> GetByIdAsync(int id);
        Task AddAsync(Booking booking);
        Task DeleteAsync(int id);
        IQueryable<Booking> GetBookingQuery();

    }
}
