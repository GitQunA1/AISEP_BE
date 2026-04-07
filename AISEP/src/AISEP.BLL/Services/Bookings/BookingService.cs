using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;
using AISEP.BLL.Helpers;
using AISEP.BLL.Services.Notifications;
using AISEP.BLL.Services.Users;
using AISEP.DAL.Common;
using AISEP.DAL.Entities;
using AISEP.DAL.Enums;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Sieve.Models;
using Sieve.Services;

namespace AISEP.BLL.Services.Bookings
{
    public class BookingService : IBookingService
    {
        private static readonly TimeSpan MinAdvanceNotice = TimeSpan.FromHours(12);
        private static readonly TimeSpan AdvisorResponseDeadline = TimeSpan.FromMinutes(10);

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISieveProcessor _sieveProcessor;
        private readonly IUserService _currentUserService;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;

        public BookingService(
            IUnitOfWork unitOfWork,
            ISieveProcessor sieveProcessor,
            IUserService currentUserService,
            IMapper mapper,
            INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _sieveProcessor = sieveProcessor;
            _currentUserService = currentUserService;
            _mapper = mapper;
            _notificationService = notificationService;
        }

        public async Task<BookingResponse?> CreateBookingAsync(CreateBookingRequest dto)
        {
            if (dto.AdvisorAvailabilitySlotIds is null || dto.AdvisorAvailabilitySlotIds.Count == 0)
                throw new InvalidOperationException("At least one slot must be selected.");

            var currentUser = _currentUserService.GetUserId();
            var currentRole = _currentUserService.GetUserRole();
            var advisor = await _unitOfWork.Advisors.GetByIdAsync(dto.AdvisorId)
                ?? throw new KeyNotFoundException("Advisor not found.");
            var project = await _unitOfWork.Projects.GetByIdAsync(dto.ProjectId)
                ?? throw new KeyNotFoundException("Project not found.");
            var assignment = await _unitOfWork.ProjectAdvisorAssignments.GetByProjectIdAsync(dto.ProjectId)
                ?? throw new InvalidOperationException("Project has not been assigned to any advisor yet.");
            var isAssignedAdvisor = assignment.AdvisorId == dto.AdvisorId;
            if (!isAssignedAdvisor)
            {
                if (!dto.SourceBookingId.HasValue)
                    throw new InvalidOperationException("Selected advisor is not assigned to this project.");

                var sourceBooking = await _unitOfWork.Bookings.GetByIdAsync(dto.SourceBookingId.Value)
                    ?? throw new KeyNotFoundException("Source booking not found.");

                if (sourceBooking.CustomerId != currentUser)
                    throw new InvalidOperationException("Source booking does not belong to current user.");

                if (sourceBooking.ProjectId != dto.ProjectId)
                    throw new InvalidOperationException("Source booking project does not match current booking project.");

                if (sourceBooking.Status != BookingStatus.Cancel && sourceBooking.Status != BookingStatus.NoResponse)
                    throw new InvalidOperationException("Only rejected/no-response booking can choose a replacement advisor.");

                var replacementAdvisors = await FindReplacementAdvisorsAsync(project, sourceBooking.AdvisorId, 5);
                if (!replacementAdvisors.Any(a => a.AdvisorId == dto.AdvisorId))
                    throw new InvalidOperationException("Selected advisor is not in replacement suggestions for this booking.");
            }

            await EnsureProjectSelectableForCurrentUserAsync(project, currentUser, currentRole);

            var selectedSlots = await _unitOfWork.AdvisorAvailabilities.GetByIdsAsync(dto.AdvisorAvailabilitySlotIds);
            if (selectedSlots.Count == 0)
                throw new InvalidOperationException("At least one slot must be selected.");

            if (selectedSlots.Count != dto.AdvisorAvailabilitySlotIds.Count)
                throw new KeyNotFoundException("One or more selected slots were not found.");

            if (selectedSlots.Any(slot => slot.AdvisorId != dto.AdvisorId))
                throw new InvalidOperationException("All selected slots must belong to the same advisor.");

            if (selectedSlots.Any(slot => slot.Status != AdvisorAvailabilityStatus.Available))
                throw new InvalidOperationException("One or more selected slots are no longer available.");

            var now = DateTime.UtcNow;
            if (selectedSlots.Any(slot => slot.SlotDate.Date < now.Date
                || (slot.SlotDate.Date == now.Date && slot.StartTime <= TimeOnly.FromDateTime(now))))
            {
                throw new InvalidOperationException("Selected slots must be in the future.");
            }

            for (var i = 1; i < selectedSlots.Count; i++)
            {
                var previousEnd = selectedSlots[i - 1].SlotDate.Date.Add(selectedSlots[i - 1].EndTime.ToTimeSpan());
                var currentStart = selectedSlots[i].SlotDate.Date.Add(selectedSlots[i].StartTime.ToTimeSpan());
                if (previousEnd != currentStart)
                    throw new InvalidOperationException("Selected slots must be consecutive in time.");
            }

            var bookingStart = DateTime.SpecifyKind(
                selectedSlots[0].SlotDate.Date.Add(selectedSlots[0].StartTime.ToTimeSpan()),
                DateTimeKind.Utc);
            var bookingEnd = DateTime.SpecifyKind(
                selectedSlots[^1].SlotDate.Date.Add(selectedSlots[^1].EndTime.ToTimeSpan()),
                DateTimeKind.Utc);
            if (bookingStart < DateTime.UtcNow.Add(MinAdvanceNotice))
                throw new InvalidOperationException("Bookings must be made at least 12 hours in advance.");

            var booking = new Booking
            {
                AdvisorId = dto.AdvisorId,
                ProjectId = dto.ProjectId,
                CustomerId = currentUser,
                StartTime = bookingStart,
                EndTime = bookingEnd,
                Status = BookingStatus.Pending,
                Note = dto.Note
            };

            var subscription = await _unitOfWork.Subscriptions.GetLatestActiveAsync(currentUser);
            var bookingDurationHours = (decimal)(bookingEnd - bookingStart).TotalHours;
            var isEligibleForFreeBooking = bookingDurationHours <= 3m;

            if (subscription is not null
                && subscription.RemainingFreeBookings > 0
                && isEligibleForFreeBooking)
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

            var commissionConfig = await _unitOfWork.SystemCommissionConfigs.GetCurrentAsync(DateTime.UtcNow);
            booking.SystemCommissionConfigId = commissionConfig?.SystemCommissionConfigId;
            booking.SystemCommissionPercent = commissionConfig?.Percent ?? 0m;
            booking.SystemCommissionAmount = Math.Round(
                booking.Price * (booking.SystemCommissionPercent / 100m),
                2,
                MidpointRounding.AwayFromZero);

            try
            {
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
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx)
            {
                if (pgEx.ConstraintName?.Contains("IX_booking_slots_AdvisorAvailabilityId", StringComparison.OrdinalIgnoreCase) == true)
                {
                    throw new InvalidOperationException(
                        "Database still has unique index on booking_slots.AdvisorAvailabilityId. Please run latest migration/update DB.");
                }

                if (pgEx.SqlState == PostgresErrorCodes.UniqueViolation)
                {
                    throw new InvalidOperationException(
                        $"Unique constraint violated while creating booking (constraint: {pgEx.ConstraintName ?? "unknown"}).");
                }

                throw new InvalidOperationException(
                    $"Database error while creating booking (SQLSTATE: {pgEx.SqlState}).");
            }
            catch (DbUpdateException)
            {
                throw new InvalidOperationException(
                    "Database update failed while creating booking. Please check database schema/migrations.");
            }

            var createdBooking = await _unitOfWork.Bookings.GetByIdAsync(booking.BookingId);
            return createdBooking != null ? _mapper.Map<BookingResponse>(createdBooking) : null;
        }

        public async Task<List<BookingProjectOptionResponse>> GetBookingProjectOptionsAsync()
        {
            var currentUserId = _currentUserService.GetUserId();
            var currentRole = _currentUserService.GetUserRole();

            IQueryable<Project> query;
            if (string.Equals(currentRole, "Investor", StringComparison.OrdinalIgnoreCase))
            {
                query = _unitOfWork.Projects.GetByStatusQuery(ProjectStatus.Approved)
                    .Where(p => p.ProjectAdvisorAssignment != null);
            }
            else if (string.Equals(currentRole, "Startup", StringComparison.OrdinalIgnoreCase))
            {
                var startup = await _unitOfWork.Startups.GetByUserIdAsync(currentUserId)
                    ?? throw new KeyNotFoundException("Startup profile not found for this account.");
                query = _unitOfWork.Projects.GetByStartupIdQuery(startup.StartupId)
                    .Where(p => p.ProjectAdvisorAssignment != null);
            }
            else
            {
                return [];
            }

            return await query
                .OrderBy(p => p.ProjectName)
                .Select(p => new BookingProjectOptionResponse
                {
                    ProjectId = p.ProjectId,
                    ProjectName = p.ProjectName
                })
                .ToListAsync();
        }

        public async Task<List<BookingAdvisorOptionResponse>> GetBookingAdvisorOptionsAsync(int projectId)
        {
            var currentUserId = _currentUserService.GetUserId();
            var currentRole = _currentUserService.GetUserRole();
            var project = await _unitOfWork.Projects.GetByIdAsync(projectId)
                ?? throw new KeyNotFoundException("Project not found.");

            await EnsureProjectSelectableForCurrentUserAsync(project, currentUserId, currentRole);

            var assignment = await _unitOfWork.ProjectAdvisorAssignments.GetByProjectIdAsync(projectId);
            if (assignment is null)
            {
                return [];
            }

            var advisor = await _unitOfWork.Advisors.GetByIdAsync(assignment.AdvisorId);
            if (advisor is null || advisor.ApprovalStatus != ApprovalStatus.Approved)
            {
                return [];
            }

            return
            [
                new BookingAdvisorOptionResponse
                {
                    AdvisorId = advisor.AdvisorId,
                    AdvisorName = advisor.User is null
                        ? $"Advisor {advisor.AdvisorId}"
                        : (advisor.User.UserName ?? $"Advisor {advisor.AdvisorId}")
                }
            ];
        }

        public async Task<List<BookingAdvisorOptionResponse>> GetReplacementAdvisorOptionsAsync(int bookingId)
        {
            var currentUserId = _currentUserService.GetUserId();
            var booking = await _unitOfWork.Bookings.GetByIdAsync(bookingId)
                ?? throw new KeyNotFoundException("Booking not found.");

            if (booking.CustomerId != currentUserId)
                throw new InvalidOperationException("You do not have permission to view replacement advisors for this booking.");

            if (booking.Status != BookingStatus.Cancel && booking.Status != BookingStatus.NoResponse)
                throw new InvalidOperationException("Replacement advisors are only available for rejected/no-response bookings.");

            if (!booking.ProjectId.HasValue || booking.Project is null)
                throw new InvalidOperationException("Booking project is missing.");

            var suggestedAdvisors = await FindReplacementAdvisorsAsync(booking.Project, booking.AdvisorId, 3);
            return suggestedAdvisors.Select(a => new BookingAdvisorOptionResponse
            {
                AdvisorId = a.AdvisorId,
                AdvisorName = a.User is null
                    ? $"Advisor {a.AdvisorId}"
                    : (a.User.UserName ?? $"Advisor {a.AdvisorId}")
            }).ToList();
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

        public async Task<PagedResult<BookingResponse>> GetMyCustomerBookingsAsync(SieveModel sieveModel)
        {
            var currentUserId = _currentUserService.GetUserId();
            var query = _unitOfWork.Bookings.GetBookingQuery()
                .Where(b => b.CustomerId == currentUserId);

            return await PaginationHelper.PaginateAsync(query, sieveModel, _sieveProcessor, b => _mapper.Map<BookingResponse>(b));
        }

        public async Task<PagedResult<BookingResponse>> GetMyAdvisorBookingsAsync(SieveModel sieveModel)
        {
            var currentUserId = _currentUserService.GetUserId();
            var advisor = await _unitOfWork.Advisors.GetByUserIdAsync(currentUserId)
                ?? throw new KeyNotFoundException("Advisor profile not found for this account.");

            var query = _unitOfWork.Bookings.GetBookingQuery()
                .Where(b => b.AdvisorId == advisor.AdvisorId);

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

            await _notificationService.SendNotificationAsync(
                booking.CustomerId,
                "Booking rejected",
                "Advisor rejected this booking. You can rebook another time, or cancel to choose another advisor.",
                NotificationType.General);

            return _mapper.Map<BookingResponse>(booking);
        }

        public async Task<int> ExpirePendingAdvisorResponsesAsync()
        {
            var expiredBookings = await _unitOfWork.Bookings
                .GetExpiredAwaitingAdvisorResponseAsync(DateTime.UtcNow.Subtract(AdvisorResponseDeadline));

            foreach (var booking in expiredBookings)
            {
                booking.Status = BookingStatus.NoResponse;
                booking.Note = string.IsNullOrWhiteSpace(booking.Note)
                    ? "[System] Booking marked as no-response because advisor did not respond within 1 minute."
                    : $"{booking.Note} | [System] Advisor response timeout (1m), marked as no-response.";
                ReleaseBookedSlots(booking);
            }

            if (expiredBookings.Count > 0)
            {
                await _unitOfWork.SaveChangesAsync();

                foreach (var booking in expiredBookings)
                {
                    await NotifyNoResponseAndSuggestNextAdvisorAsync(booking);
                }
            }

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
                booking.Status = BookingStatus.NoResponse;
                booking.Note = string.IsNullOrWhiteSpace(booking.Note)
                    ? "[System] Booking marked as no-response because advisor response deadline passed."
                    : $"{booking.Note} | [System] Advisor response deadline passed, marked as no-response.";
                ReleaseBookedSlots(booking);
                await _unitOfWork.SaveChangesAsync();
                await NotifyNoResponseAndSuggestNextAdvisorAsync(booking);
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

        private async Task EnsureProjectSelectableForCurrentUserAsync(Project project, int currentUserId, string? currentRole)
        {
            if (string.Equals(currentRole, "Investor", StringComparison.OrdinalIgnoreCase))
            {
                if (project.Status != ProjectStatus.Approved)
                {
                    throw new InvalidOperationException("Investor can only select approved projects.");
                }

                return;
            }

            if (string.Equals(currentRole, "Startup", StringComparison.OrdinalIgnoreCase))
            {
                var startup = await _unitOfWork.Startups.GetByUserIdAsync(currentUserId)
                    ?? throw new KeyNotFoundException("Startup profile not found for this account.");
                if (project.StartupId != startup.StartupId)
                {
                    throw new InvalidOperationException("Startup can only select its own projects.");
                }

                return;
            }

            throw new InvalidOperationException("Current role is not allowed to select a project for booking.");
        }

        private async Task NotifyNoResponseAndSuggestNextAdvisorAsync(Booking booking)
        {
            var suggestedAdvisors = await FindReplacementAdvisorsAsync(booking.Project, booking.AdvisorId, 3);
            if (suggestedAdvisors.Count == 0)
            {
                await _notificationService.SendNotificationAsync(
                    booking.CustomerId,
                    "Advisor no response",
                    "Booking timed out and was marked as NoResponse. No suitable replacement advisor is available now. Please choose another advisor.",
                    NotificationType.General);
                return;
            }

            var suggestedText = string.Join(", ", suggestedAdvisors.Select(x =>
                $"{(x.User.UserName ?? $"Advisor {x.AdvisorId}")} (ID: {x.AdvisorId})"));

            await _notificationService.SendNotificationAsync(
                booking.CustomerId,
                "Advisor no response",
                $"Booking timed out and was marked as NoResponse. Suggested advisors: {suggestedText}. Please book again with one of them.",
                NotificationType.General);
        }

        private async Task<List<Advisor>> FindReplacementAdvisorsAsync(Project? project, int excludedAdvisorId, int topN)
        {
            if (project is null)
            {
                return [];
            }

            var advisors = await _unitOfWork.Advisors.GetAllQuery()
                .Where(a => a.ApprovalStatus == ApprovalStatus.Approved
                            && a.AdvisorIndustries.Any(ai => ai.Industry == project.Industry)
                            && a.AdvisorId != excludedAdvisorId)
                .ToListAsync();

            if (advisors.Count == 0)
            {
                return [];
            }

            var advisorIds = advisors.Select(a => a.AdvisorId).ToList();
            var today = DateTime.UtcNow.Date;
            var weekEndExclusive = today.AddDays(7);

            var availableCounts = await _unitOfWork.AdvisorAvailabilities.GetQuery()
                .Where(x => advisorIds.Contains(x.AdvisorId)
                            && x.Status == AdvisorAvailabilityStatus.Available
                            && x.SlotDate >= today
                            && x.SlotDate < weekEndExclusive)
                .GroupBy(x => x.AdvisorId)
                .Select(g => new { AdvisorId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.AdvisorId, x => x.Count);

            var rejectedCounts = await _unitOfWork.Bookings.GetBookingQuery()
                .Where(b => advisorIds.Contains(b.AdvisorId)
                            && b.Status == BookingStatus.Cancel
                            && b.Note != null
                            && b.Note.Contains("[Advisor Reject]"))
                .GroupBy(b => b.AdvisorId)
                .Select(g => new { AdvisorId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.AdvisorId, x => x.Count);

            var noResponseCounts = await _unitOfWork.Bookings.GetBookingQuery()
                .Where(b => advisorIds.Contains(b.AdvisorId)
                            && b.Status == BookingStatus.NoResponse)
                .GroupBy(b => b.AdvisorId)
                .Select(g => new { AdvisorId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.AdvisorId, x => x.Count);

            var bestAdvisorIds = advisors
                .Select(a =>
                {
                    var availability = availableCounts.GetValueOrDefault(a.AdvisorId, 0);
                    var rejected = rejectedCounts.GetValueOrDefault(a.AdvisorId, 0);
                    var noResponse = noResponseCounts.GetValueOrDefault(a.AdvisorId, 0);
                    var rating = (double)(a.Rating ?? 0);

                    var score = availability - (rejected * 2) - (noResponse * 3) + (rating * 0.5);
                    return new { a.AdvisorId, Score = score, availability, rejected, noResponse };
                })
                .OrderByDescending(x => x.Score)
                .ThenByDescending(x => x.availability)
                .ThenBy(x => x.noResponse)
                .ThenBy(x => x.rejected)
                .Select(x => x.AdvisorId)
                .Take(topN)
                .ToList();

            var advisorById = advisors.ToDictionary(a => a.AdvisorId);
            return bestAdvisorIds
                .Where(advisorById.ContainsKey)
                .Select(id => advisorById[id])
                .ToList();
        }
    }
}
