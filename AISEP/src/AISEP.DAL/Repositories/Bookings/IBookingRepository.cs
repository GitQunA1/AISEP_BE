using AISEP.DAL.Entities;

namespace AISEP.DAL.Repositories.Bookings
{
    public interface IBookingRepository
    {
        Task<Booking?> GetByIdAsync(int id);
        Task<Booking?> GetByIdForAdvisorActionAsync(int id);
        Task<Booking?> GetByIdWithAdvisorWalletAsync(int id);
        Task<Booking?> GetPayableByIdAndCustomerAsync(int bookingId, int customerId);
        Task<List<Booking>> GetExpiredAwaitingAdvisorResponseAsync(DateTime thresholdUtc);
        Task AddAsync(Booking booking);
        Task DeleteAsync(int id);
        IQueryable<Booking> GetBookingQuery();
    }
}
