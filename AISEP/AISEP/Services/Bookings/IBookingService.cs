using AISEP.DTOs;
using Sieve.Models;

namespace AISEP.Services.Bookings
{
    public interface IBookingService
    {
        Task<BookingResponseDto?> CreateBookingAsync(BookingDto dto);
        Task<BookingResponseDto?> GetBookingByIdAsync(int id);
        Task<PagedResultDto<BookingResponseDto>> GetAllBookingsAsync(SieveModel model);
        Task<PagedResultDto<BookingResponseDto>> GetBookingsByAdvisorIdAsync(int advisorId, SieveModel model);
        Task<PagedResultDto<BookingResponseDto>> GetBookingsByCustomerIdAsync(int customerId, SieveModel model);

        Task<bool> DeleteBookingAsync(int id);
    }
}
