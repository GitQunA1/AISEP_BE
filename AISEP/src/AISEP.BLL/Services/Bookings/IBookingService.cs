using AISEP.BLL.Helpers;
using AISEP.DAL.Common;
using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;
using Sieve.Models;

namespace AISEP.BLL.Services.Bookings
{
    public interface IBookingService
    {
        Task<BookingResponse?> CreateBookingAsync(CreateBookingRequest dto);
        Task<BookingResponse?> GetBookingByIdAsync(int id);
        Task<PagedResult<BookingResponse>> GetAllBookingsAsync(SieveModel model);
        Task<BookingResponse?> GetMyBookingAsync();
        Task<PagedResult<BookingResponse>> GetBookingsByAdvisorIdAsync(int advisorId, SieveModel model);
        Task<PagedResult<BookingResponse>> GetBookingsByCustomerIdAsync(int customerId, SieveModel model);
        Task<BookingResponse?> ApproveBookingAsync(int id);
        Task<BookingResponse?> RejectBookingAsync(int id, string? reason);
        Task<bool> DeleteBookingAsync(int id);
        Task<int> ExpirePendingAdvisorResponsesAsync();
    }
}
