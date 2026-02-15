using AISEP.DTOs;
using Sieve.Models;

namespace AISEP.Services
{
    public interface IBookingService
    {
        Task<BookingResponseDto?> CreateBookingAsync(BookingDto dto);
        Task<BookingResponseDto?> GetBookingByIdAsync(Guid id);
        Task<PagedResultDto<BookingResponseDto>> GetAllBookingsAsync(SieveModel model);
        Task<PagedResultDto<BookingResponseDto>> GetBookingsByAdvisorIdAsync(Guid advisorId, SieveModel model);
        Task<PagedResultDto<BookingResponseDto>> GetBookingsByCustomerIdAsync(Guid customerId, SieveModel model);

        Task<bool> DeleteBookingAsync(Guid id);
    }
}
