using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;
using AISEP.BLL.Exceptions;
using AISEP.BLL.Services.Users;
using AISEP.DAL.Common;
using AISEP.DAL.Entities;
using AISEP.DAL.Enums;
using AutoMapper;

namespace AISEP.BLL.Services.ConsultingReports
{
    public class ConsultingReportService : IConsultingReportService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public ConsultingReportService(IUnitOfWork unitOfWork, IUserService userService, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _userService = userService;
            _mapper = mapper;
        }

        public async Task<ConsultingReportResponse> CreateAsync(CreateConsultingReportRequest request)
        {
            var userId = _userService.GetUserId();
            var advisor = await _unitOfWork.Advisors.GetByUserIdAsync(userId)
                ?? throw new ForbiddenAccessException("Only advisor can create consulting report.");

            var booking = await _unitOfWork.Bookings.GetByIdAsync(request.BookingId)
                ?? throw new KeyNotFoundException("Booking not found.");

            if (booking.AdvisorId != advisor.AdvisorId)
                throw new ForbiddenAccessException("You are not assigned to this booking.");

            if (booking.Status != BookingStatus.Confirmed && booking.Status != BookingStatus.Completed)
                throw new InvalidOperationException("Consulting report can only be created for confirmed/completed booking.");

            var existing = await _unitOfWork.ConsultingReports.GetByBookingIdAsync(request.BookingId);
            if (existing is not null)
                throw new InvalidOperationException("Consulting report already exists for this booking.");

            var report = _mapper.Map<ConsultingReport>(request);
            report.CreatedAt = DateTime.UtcNow;

            await _unitOfWork.ConsultingReports.AddAsync(report);

            if (booking.Status != BookingStatus.Completed)
            {
                booking.Status = BookingStatus.Completed;
            }

            await _unitOfWork.SaveChangesAsync();

            var created = await _unitOfWork.ConsultingReports.GetByIdAsync(report.ConsultingReportId)
                ?? throw new InvalidOperationException("Failed to load created consulting report.");

            return _mapper.Map<ConsultingReportResponse>(created);
        }

        public async Task<ConsultingReportResponse?> GetByIdAsync(int id)
        {
            var report = await _unitOfWork.ConsultingReports.GetByIdAsync(id);
            if (report is null)
            {
                return null;
            }

            EnsureCanAccess(report.Booking);
            return _mapper.Map<ConsultingReportResponse>(report);
        }

        public async Task<ConsultingReportResponse?> GetByBookingIdAsync(int bookingId)
        {
            var report = await _unitOfWork.ConsultingReports.GetByBookingIdAsync(bookingId);
            if (report is null)
            {
                return null;
            }

            EnsureCanAccess(report.Booking);
            return _mapper.Map<ConsultingReportResponse>(report);
        }

        private void EnsureCanAccess(Booking booking)
        {
            var userId = _userService.GetUserId();
            var role = _userService.GetUserRole();

            if (string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase)
                || string.Equals(role, "Staff", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var isAdvisor = booking.Advisor?.UserId == userId;
            var isCustomer = booking.CustomerId == userId;
            if (!isAdvisor && !isCustomer)
            {
                throw new ForbiddenAccessException("You do not have permission to access this consulting report.");
            }
        }
    }
}
