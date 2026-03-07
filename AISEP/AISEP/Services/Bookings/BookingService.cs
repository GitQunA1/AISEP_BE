using AISEP.Common;
using AISEP.DTOs.Requests;
using AISEP.DTOs.Responses;
using AISEP.Models.Entities;
using AISEP.Models.Enums;
using AISEP.Services.Users;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using Sieve.Services;

namespace AISEP.Services.Bookings
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

            var booking = _mapper.Map<Booking>(dto);
            booking.CustomerId = currentUser;
            booking.Price = 200000;
            booking.Status = BookingStatus.Pending;

            await _unitOfWork.Bookings.AddAsync(booking);
            await _unitOfWork.SaveChangesAsync();

            
            var created = await _unitOfWork.Bookings.GetByIdAsync(booking.BookingId);
           
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
            return await ApplySieveAndPaginateAsync(query, sieveModel);
        }

        public async Task<PagedResult<BookingResponse>> GetBookingsByAdvisorIdAsync(int advisorId, SieveModel sieveModel)
        {
            var query = _unitOfWork.Bookings.GetBookingQuery()
                .Where(b => b.AdvisorId == advisorId);
            return await ApplySieveAndPaginateAsync(query, sieveModel);
        }

        public async Task<PagedResult<BookingResponse>> GetBookingsByCustomerIdAsync(int customerId, SieveModel sieveModel)
        {
            var query = _unitOfWork.Bookings.GetBookingQuery()
                .Where(b => b.CustomerId == customerId);
            return await ApplySieveAndPaginateAsync(query, sieveModel);
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

     
        private async Task<PagedResult<BookingResponse>> ApplySieveAndPaginateAsync(
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

            return new PagedResult<BookingResponse>
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages,
                Items = items.Select(b => _mapper.Map<BookingResponse>(b))
            };
        }
    }
}
