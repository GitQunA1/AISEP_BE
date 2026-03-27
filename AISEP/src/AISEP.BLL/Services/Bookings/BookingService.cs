using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;
using AISEP.BLL.Helpers;
using AISEP.BLL.Services.Users;
using AISEP.DAL.Common;
using AISEP.DAL.Entities;
using AISEP.DAL.Enums;
using AutoMapper;
using Sieve.Models;
using Sieve.Services;

namespace AISEP.BLL.Services.Bookings
{
    public class BookingService : IBookingService
    {
        private static readonly TimeSpan MinAdvanceNotice = TimeSpan.FromHours(48);
        private static readonly TimeSpan AdvisorResponseDeadline = TimeSpan.FromHours(12);

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

            var selectedSlots = await _unitOfWork.AdvisorAvailabilities.GetByIdsAsync(dto.AdvisorAvailabilitySlotIds);
            if (selectedSlots.Count == 0)
                throw new InvalidOperationException("At least one slot must be selected.");

            if (selectedSlots.Count != dto.AdvisorAvailabilitySlotIds.Count)
                throw new KeyNotFoundException("One or more selected slots were not found.");

            if (selectedSlots.Any(slot => slot.AdvisorId != dto.AdvisorId))
                throw new InvalidOperationException("All selected slots must belong to the same advisor.");

            if (selectedSlots.Any(slot => slot.Status != AdvisorAvailabilityStatus.Available))
                throw new InvalidOperationException("One or more selected slots are no longer available.");

            var slotDate = selectedSlots[0].SlotDate.Date;
            if (selectedSlots.Any(slot => slot.SlotDate.Date != slotDate))
                throw new InvalidOperationException("Selected slots must be on the same date.");

            var now = DateTime.UtcNow;
            if (selectedSlots.Any(slot => slot.SlotDate.Date < now.Date
                || (slot.SlotDate.Date == now.Date && slot.StartTime <= TimeOnly.FromDateTime(now))))
            {
                throw new InvalidOperationException("Selected slots must be in the future.");
            }

            for (var i = 1; i < selectedSlots.Count; i++)
            {
                if (selectedSlots[i - 1].EndTime != selectedSlots[i].StartTime)
                    throw new InvalidOperationException("Selected slots must be consecutive.");
            }

            var bookingStart = slotDate.Add(selectedSlots[0].StartTime.ToTimeSpan());
            var bookingEnd = slotDate.Add(selectedSlots[^1].EndTime.ToTimeSpan());
            if (bookingStart < DateTime.UtcNow.Add(MinAdvanceNotice))
                throw new InvalidOperationException("Bookings must be made at least 48 hours in advance.");

            var booking = new Booking
            {
                AdvisorId = dto.AdvisorId,
                CustomerId = currentUser,
                StartTime = bookingStart,
                EndTime = bookingEnd,
                Status = BookingStatus.Pending,
                Note = dto.Note
            };

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
                booking.Price = Math.Round(hourlyRate * selectedSlots.Count, 2, MidpointRounding.AwayFromZero);
            }

            await _unitOfWork.Bookings.AddAsync(booking);
            await _unitOfWork.SaveChangesAsync();

            foreach (var slot in selectedSlots)
            {
                slot.Status = AdvisorAvailabilityStatus.Booked;
                slot.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.AdvisorAvailabilities.Update(slot);
            }

            await _unitOfWork.BookingSlots.AddRangeAsync(selectedSlots.Select(slot => new BookingSlot
            {
                BookingId = booking.BookingId,
                AdvisorAvailabilityId = slot.AdvisorAvailabilityId
            }));

            await _unitOfWork.SaveChangesAsync();

            var createdBooking = await _unitOfWork.Bookings.GetByIdAsync(booking.BookingId);
            return createdBooking != null ? _mapper.Map<BookingResponse>(createdBooking) : null;
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

            ReleaseBookedSlots(booking);

            await _unitOfWork.Bookings.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<BookingResponse?> ApproveBookingAsync(int id)
        {
            var booking = await _unitOfWork.Bookings.GetByIdForAdvisorActionAsync(id)
                ?? throw new KeyNotFoundException("Booking not found.");

            await EnsureAdvisorCanRespondAsync(booking);

            if (booking.Status == BookingStatus.ApprovedAwaitingPayment
                || booking.Status == BookingStatus.Confirmed)
                return _mapper.Map<BookingResponse>(booking);

            if (booking.Status != BookingStatus.Pending)
                throw new InvalidOperationException("Only pending bookings can be approved.");

            booking.Status = BookingStatus.ApprovedAwaitingPayment;
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<BookingResponse>(booking);
        }

        public async Task<BookingResponse?> RejectBookingAsync(int id, string? reason)
        {
            var booking = await _unitOfWork.Bookings.GetByIdForAdvisorActionAsync(id)
                ?? throw new KeyNotFoundException("Booking not found.");

            await EnsureAdvisorCanRespondAsync(booking);

            if (booking.Status == BookingStatus.Cancel)
                return _mapper.Map<BookingResponse>(booking);

            if (booking.Status != BookingStatus.Pending && booking.Status != BookingStatus.ApprovedAwaitingPayment)
                throw new InvalidOperationException("Only pending/approved-awaiting-payment bookings can be rejected.");

            booking.Status = BookingStatus.Cancel;
            booking.Note = string.IsNullOrWhiteSpace(reason)
                ? booking.Note
                : $"[Advisor Reject] {reason}";
            ReleaseBookedSlots(booking);

            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<BookingResponse>(booking);
        }

        public async Task<int> ExpirePendingAdvisorResponsesAsync()
        {
            var expiredBookings = await _unitOfWork.Bookings
                .GetExpiredAwaitingAdvisorResponseAsync(DateTime.UtcNow.Subtract(AdvisorResponseDeadline));

            foreach (var booking in expiredBookings)
            {
                booking.Status = BookingStatus.Cancel;
                booking.Note = string.IsNullOrWhiteSpace(booking.Note)
                    ? "[System] Booking auto-cancelled because advisor did not respond within 12 hours."
                    : $"{booking.Note} | [System] Advisor response timeout (12h).";
                ReleaseBookedSlots(booking);
            }

            if (expiredBookings.Count > 0)
                await _unitOfWork.SaveChangesAsync();

            return expiredBookings.Count;
        }

        private async Task EnsureAdvisorCanRespondAsync(Booking booking)
        {
            var userId = _currentUserService.GetUserId();
            var advisor = await _unitOfWork.Advisors.GetByUserIdAsync(userId)
                ?? throw new KeyNotFoundException("Advisor profile not found.");

            if (booking.AdvisorId != advisor.AdvisorId)
                throw new InvalidOperationException("You are not assigned to this booking.");

            var responseDeadline = booking.CreatedAt.Add(AdvisorResponseDeadline);
            if (DateTime.UtcNow > responseDeadline
                && booking.Status == BookingStatus.Pending)
            {
                booking.Status = BookingStatus.Cancel;
                booking.Note = string.IsNullOrWhiteSpace(booking.Note)
                    ? "[System] Booking auto-cancelled because advisor response deadline passed."
                    : $"{booking.Note} | [System] Advisor response deadline passed.";
                ReleaseBookedSlots(booking);
                await _unitOfWork.SaveChangesAsync();
                throw new InvalidOperationException("Booking response window has expired.");
            }
        }

        private void ReleaseBookedSlots(Booking booking)
        {
            foreach (var bookingSlot in booking.BookingSlots)
            {
                bookingSlot.AdvisorAvailability.Status = AdvisorAvailabilityStatus.Available;
                bookingSlot.AdvisorAvailability.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.AdvisorAvailabilities.Update(bookingSlot.AdvisorAvailability);
            }
        }
    }
}
