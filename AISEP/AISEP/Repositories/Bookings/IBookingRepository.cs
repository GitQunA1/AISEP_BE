using AISEP.Models.Entities;

namespace AISEP.Repositories.Bookings
{
    public interface IBookingRepository
    {
       
        Task<Booking?> GetByIdAsync(int id);
        Task<IEnumerable<Booking>> GetAllAsync();
        Task AddAsync(Booking booking);
        Task DeleteAsync(int id);

       
        Task<IEnumerable<Booking>> GetBookingsByAdvisorIdAsync(int advisorId);
        Task<IEnumerable<Booking>> GetBookingsByCustomerIdAsync(int customerId);
    

       IQueryable<Booking> GetQueryable();

    }
}
