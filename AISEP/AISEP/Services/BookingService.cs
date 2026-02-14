using AISEP.Common;
using AISEP.DTOs;
using AISEP.Models;
using AISEP.Models.Enums;

namespace AISEP.Services
{
    public class BookingService : IBookingService
    {
        private readonly IUnitOfWork _unitOfWork;

        public BookingService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<BookingResponseDto?> CreateBookingAsync(CreateBookingDto dto)
        {
            // Validate advisor exists
            var advisor = await _unitOfWork.Bookings.GetByIdAsync(dto.AdvisorId);
            if (advisor == null)
            {
                throw new Exception("Advisor not found");
            }

            // Check if advisor is available
            var isAvailable = await _unitOfWork.Bookings.IsAdvisorAvailableAsync(
                dto.AdvisorId,
                dto.StartTime,
                dto.EndTime
            );

            if (!isAvailable)
            {
                throw new Exception("Advisor is not available at this time");
            }

            // Create booking
            var booking = new Booking
            {
                Id = Guid.NewGuid(),
                AdvisorId = dto.AdvisorId,
                CustomerId = dto.CustomerId,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                Price = dto.Price,
                Status = BookingStatus.Pending
            };

            await _unitOfWork.Bookings.AddAsync(booking);
            await _unitOfWork.SaveChangesAsync();

            // Get booking with details
            var createdBooking = await _unitOfWork.Bookings.GetBookingWithDetailsAsync(booking.Id);
            return MapToResponseDto(createdBooking);
        }

        public async Task<BookingResponseDto?> GetBookingByIdAsync(Guid id)
        {
            var booking = await _unitOfWork.Bookings.GetByIdAsync(id);
            return booking != null ? MapToResponseDto(booking) : null;
        }

        public async Task<IEnumerable<BookingResponseDto>> GetAllBookingsAsync()
        {
            var bookings = await _unitOfWork.Bookings.GetAllAsync();
            return bookings.Select(MapToResponseDto);
        }

        public async Task<IEnumerable<BookingResponseDto>> GetBookingsByAdvisorIdAsync(Guid advisorId)
        {
            var bookings = await _unitOfWork.Bookings.GetBookingsByAdvisorIdAsync(advisorId);
            return bookings.Select(MapToResponseDto);
        }

        public async Task<IEnumerable<BookingResponseDto>> GetBookingsByCustomerIdAsync(Guid customerId)
        {
            var bookings = await _unitOfWork.Bookings.GetBookingsByCustomerIdAsync(customerId);
            return bookings.Select(MapToResponseDto);
        }

        public async Task<BookingResponseDto?> UpdateBookingAsync(Guid id, UpdateBookingDto dto)
        {
            var booking = await _unitOfWork.Bookings.GetByIdAsync(id);
            if (booking == null)
            {
                return null;
            }

            // Update properties
            if (dto.StartTime.HasValue)
                booking.StartTime = dto.StartTime.Value;

            if (dto.EndTime.HasValue)
                booking.EndTime = dto.EndTime.Value;

            if (dto.Price.HasValue)
                booking.Price = dto.Price.Value;

            if (dto.Status.HasValue)
                booking.Status = dto.Status.Value;

            await _unitOfWork.Bookings.UpdateAsync(booking);
            await _unitOfWork.SaveChangesAsync();

            var updatedBooking = await _unitOfWork.Bookings.GetByIdAsync(id);
            return MapToResponseDto(updatedBooking);
        }

        public async Task<bool> DeleteBookingAsync(Guid id)
        {
            var booking = await _unitOfWork.Bookings.GetByIdAsync(id);
            if (booking == null)
            {
                return false;
            }

            await _unitOfWork.Bookings.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        private BookingResponseDto MapToResponseDto(Booking? booking)
        {
            if (booking == null)
            {
                throw new ArgumentNullException(nameof(booking));
            }

            return new BookingResponseDto
            {
                Id = booking.Id,
                AdvisorId = booking.AdvisorId,
                AdvisorName = booking.Advisor?.User?.UserName ?? "Unknown",
                CustomerId = booking.CustomerId,
                CustomerName = booking.Customer?.UserName ?? "Unknown",
                StartTime = booking.StartTime,
                EndTime = booking.EndTime,
                Price = booking.Price,
                Status = booking.Status
            };
        }
    }
}
