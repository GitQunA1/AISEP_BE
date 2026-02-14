using AISEP.DTOs;

namespace AISEP.Services
{
    public interface IBookingService
    {
        Task<BookingResponseDto?> CreateBookingAsync(CreateBookingDto dto);
        Task<BookingResponseDto?> GetBookingByIdAsync(Guid id);
        Task<IEnumerable<BookingResponseDto>> GetAllBookingsAsync();
        Task<IEnumerable<BookingResponseDto>> GetBookingsByAdvisorIdAsync(Guid advisorId);
        Task<IEnumerable<BookingResponseDto>> GetBookingsByCustomerIdAsync(Guid customerId);
        Task<BookingResponseDto?> UpdateBookingAsync(Guid id, UpdateBookingDto dto);
        Task<bool> DeleteBookingAsync(Guid id);
    }
}
