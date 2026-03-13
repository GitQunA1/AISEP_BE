using AISEP.BLL.Helpers;
using AISEP.DAL.Common;
using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;
using AISEP.DAL.Entities;
using AISEP.DAL.Enums;
using AISEP.BLL.Services.Users;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
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
            return await PaginationHelper.PaginateAsync(query, sieveModel, _sieveProcessor, b => _mapper.Map<BookingResponse>(b));
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
