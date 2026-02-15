using AISEP.Models;

namespace AISEP.Repositories.Bookings
{
    public interface IBookingRepository
    {
       
        Task<Booking?> GetByIdAsync(Guid id);
        Task<IEnumerable<Booking>> GetAllAsync();
        Task AddAsync(Booking booking);
        Task DeleteAsync(Guid id);

       
        Task<IEnumerable<Booking>> GetBookingsByAdvisorIdAsync(Guid advisorId);
        Task<IEnumerable<Booking>> GetBookingsByCustomerIdAsync(Guid customerId);
        Task<Booking?> GetBookingWithDetailsAsync(Guid bookingId);
    

       IQueryable<Booking> GetQueryable();

    }
}
