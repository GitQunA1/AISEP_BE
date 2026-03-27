using AISEP.BLL.Helpers;
using AISEP.DAL.Common;
using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;
using AISEP.DAL.Entities;
using AISEP.DAL.Enums;
using AISEP.BLL.Services.Users;
using AutoMapper;
using Sieve.Models;
using Sieve.Services;

namespace AISEP.BLL.Services.Bookings
{
    public class BookingService : IBookingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISieveProcessor _sieveProcessor;
        private readonly IUserService _currentUserService;
        private readonly IMapper _mapper;

        public BookingService(
            IUnitOfWork unitOfWork, 
            ISieveProcessor sieveProcessor,
            IUserService currentUserService,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _sieveProcessor = sieveProcessor;
            _currentUserService = currentUserService;
            _mapper = mapper;
        }

        public async Task<BookingResponse?> CreateBookingAsync(CreateBookingRequest dto)
        {
            var currentUser = _currentUserService.GetUserId();
            var advisor = await _unitOfWork.Advisors.GetByIdAsync(dto.AdvisorId)
                ?? throw new KeyNotFoundException("Advisor not found.");

            if (dto.EndTime <= dto.StartTime)
            {
                throw new InvalidOperationException("Booking time range is invalid.");
            }

            var booking = _mapper.Map<Booking>(dto);
            booking.CustomerId = currentUser;
            booking.Status = BookingStatus.Pending;

            var subscription = await _unitOfWork.Subscriptions.GetLatestActiveAsync(currentUser);
            if (subscription is not null && subscription.RemainingFreeBookings > 0)
            {
                booking.Price = 0;
                subscription.RemainingFreeBookings -= 1;
                _unitOfWork.Subscriptions.Update(subscription);
            }
            else
            {
                var hourlyRate = advisor.HourlyRate ?? 0;
                var totalHours = Math.Max((decimal)(dto.EndTime - dto.StartTime).TotalHours, 1m);
                booking.Price = Math.Round(hourlyRate * totalHours, 2, MidpointRounding.AwayFromZero);
            }

            await _unitOfWork.Bookings.AddAsync(booking);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<BookingResponse>(booking);
        }

        public async Task<BookingResponse?> GetBookingByIdAsync(int id)
        {
            var booking = await _unitOfWork.Bookings.GetByIdAsync(id);
            return booking != null ? _mapper.Map<BookingResponse>(booking) : null;
        }

        public async Task<PagedResult<BookingResponse>> GetAllBookingsAsync(SieveModel sieveModel)
        {
            var query = _unitOfWork.Bookings.GetBookingQuery();
            return await PaginationHelper.PaginateAsync(query, sieveModel, _sieveProcessor, b => _mapper.Map<BookingResponse>(b));
        }

        public async Task<BookingResponse?> GetMyBookingAsync()
        {
            var currentUserId = _currentUserService.GetUserId();
            var booking = _unitOfWork.Bookings.GetBookingQuery()
                .Where(b => b.CustomerId == currentUserId)
                .OrderByDescending(b => b.BookingId)
                .FirstOrDefault();

            return await Task.FromResult(booking != null ? _mapper.Map<BookingResponse>(booking) : null);
        }

        public async Task<PagedResult<BookingResponse>> GetBookingsByAdvisorIdAsync(int advisorId, SieveModel sieveModel)
        {
            var query = _unitOfWork.Bookings.GetBookingQuery()
                .Where(b => b.AdvisorId == advisorId);
            return await PaginationHelper.PaginateAsync(query, sieveModel, _sieveProcessor, b => _mapper.Map<BookingResponse>(b));
        }

        public async Task<PagedResult<BookingResponse>> GetBookingsByCustomerIdAsync(int customerId, SieveModel sieveModel)
        {
            var query = _unitOfWork.Bookings.GetBookingQuery()
                .Where(b => b.CustomerId == customerId);
            return await PaginationHelper.PaginateAsync(query, sieveModel, _sieveProcessor, b => _mapper.Map<BookingResponse>(b));
        }

        public async Task<bool> DeleteBookingAsync(int id)
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

    }
}
