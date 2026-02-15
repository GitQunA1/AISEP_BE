using AISEP.Common;
using AISEP.DTOs;
using AISEP.Models;
using AISEP.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using Sieve.Services;

namespace AISEP.Services
{
    public class BookingService : IBookingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISieveProcessor _sieveProcessor;
        private readonly ICurrentUserService _currentUserService;

        public BookingService(
            IUnitOfWork unitOfWork, 
            ISieveProcessor sieveProcessor,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _sieveProcessor = sieveProcessor;
            _currentUserService = currentUserService;
        }

        public async Task<BookingResponseDto?> CreateBookingAsync(BookingDto dto)
        {
           
            var currentUser =  _currentUserService.GetUserId();

            var booking = new Booking
            {
                Id = Guid.NewGuid(),
                AdvisorId = dto.AdvisorId,
                CustomerId = currentUser, 
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                Price = 200000, 
                Status = BookingStatus.Pending
            };

            await _unitOfWork.Bookings.AddAsync(booking);
            await _unitOfWork.SaveChangesAsync();

        
       
            return MapToResponseDto(booking);
        }

        public async Task<BookingResponseDto?> GetBookingByIdAsync(Guid id)
        {
            var booking = await _unitOfWork.Bookings.GetByIdAsync(id);
            return booking != null ? MapToResponseDto(booking) : null;
        }

        public async Task<PagedResultDto<BookingResponseDto>> GetAllBookingsAsync(SieveModel sieveModel)
        {
            var query = _unitOfWork.Bookings.GetQueryable();
            return await ApplySieveAndPaginateAsync(query, sieveModel);
        }

        public async Task<PagedResultDto<BookingResponseDto>> GetBookingsByAdvisorIdAsync(Guid advisorId, SieveModel sieveModel)
        {
            
            var query = _unitOfWork.Bookings.GetQueryable()
                .Where(b => b.AdvisorId == advisorId);

            return await ApplySieveAndPaginateAsync(query, sieveModel);
        }

        public async Task<PagedResultDto<BookingResponseDto>> GetBookingsByCustomerIdAsync(Guid customerId, SieveModel sieveModel)
        {
           
            var query = _unitOfWork.Bookings.GetQueryable()
                .Where(b => b.CustomerId == customerId);

            return await ApplySieveAndPaginateAsync(query, sieveModel);
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

     
        private async Task<PagedResultDto<BookingResponseDto>> ApplySieveAndPaginateAsync(
            IQueryable<Booking> query, 
            SieveModel sieveModel)
        {
       
            var totalCount = await _sieveProcessor
                .Apply(sieveModel, query, applyPagination: false, applySorting: false)
                .CountAsync();

         
            var items = await _sieveProcessor
                .Apply(sieveModel, query)
                .ToListAsync();

            var page = sieveModel.Page ?? 1;
            var pageSize = sieveModel.PageSize ?? 10;
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return new PagedResultDto<BookingResponseDto>
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages,
                Items = items.Select(MapToResponseDto)
            };
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
