using AISEP.Models.Entities;

namespace AISEP.Repositories.Bookings
{
    public interface IBookingRepository
    {
       
        Task<Booking?> GetByIdAsync(Guid id);
        Task AddAsync(Booking booking);
        Task DeleteAsync(Guid id);
        IQueryable<Booking> GetBookingQuery();

    }
}
