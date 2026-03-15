using AISEP.DAL.Entities;

namespace AISEP.DAL.Repositories.Bookings
{
    public interface IBookingRepository
    {
        Task<Booking?> GetByIdAsync(int id);
        Task<Booking?> GetByIdWithAdvisorWalletAsync(int id);
        Task<Booking?> GetPendingByIdAndCustomerAsync(int bookingId, int customerId);
        Task AddAsync(Booking booking);
        Task DeleteAsync(int id);
        IQueryable<Booking> GetBookingQuery();
    }
}
