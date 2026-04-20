using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;
using AISEP.BLL.Helpers;
using AISEP.BLL.Services.Storage;
using AISEP.BLL.Services.Users;
using AISEP.BLL.Services.Notifications;
using AISEP.DAL.Common;
using AISEP.DAL.Entities;
using AISEP.DAL.Enums;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using Sieve.Services;
using System.Text.Json;

namespace AISEP.BLL.Services.UserReports
{
    public class UserReportService : IUserReportService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserService _userService;
        private readonly IStorageService _storageService;
        private readonly ISieveProcessor _sieveProcessor;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;

        public UserReportService(
            IUnitOfWork unitOfWork,
            IUserService userService,
            IStorageService storageService,
            ISieveProcessor sieveProcessor,
            IMapper mapper,
            INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _userService = userService;
            _storageService = storageService;
            _sieveProcessor = sieveProcessor;
            _mapper = mapper;
            _notificationService = notificationService;
        }

        public async Task<UserReportResponse> CreateAsync(CreateUserReportRequest request)
        {
            var reporterId = _userService.GetUserId();
            var booking = await _unitOfWork.Bookings.GetByIdAsync(request.BookingId)
                ?? throw new KeyNotFoundException("Booking not found.");
            var advisorUserId = booking.Advisor?.UserId
                ?? throw new InvalidOperationException("Booking advisor account is missing.");
            var customerUserId = booking.CustomerId;
            var isReporterParticipant = reporterId == customerUserId;

            if (!isReporterParticipant)
            {
                throw new InvalidOperationException("You are not a participant of this booking.");
            }

            var hasPendingReport = await _unitOfWork.UserReports.GetAll()
                .AnyAsync(x => x.BookingId == request.BookingId
                    && x.ReporterId == reporterId
                    && x.Status == UserReportStatus.Pending);
            if (hasPendingReport)
            {
                throw new InvalidOperationException("You already have a pending report for this booking.");
            }

            var uploadedImageUrls = new List<string>();
            if (request.EvidenceImages is not null && request.EvidenceImages.Count > 0)
            {
                foreach (var image in request.EvidenceImages)
                {
                    uploadedImageUrls.Add(await _storageService.UploadFileAsync(image, "user-reports"));
                }
            }

            var report = _mapper.Map<UserReport>(request);
            report.ReporterId = reporterId;
            report.Reason = request.Description.Trim();
            report.EvidenceImageUrls = uploadedImageUrls.Count == 0
                ? null
                : JsonSerializer.Serialize(uploadedImageUrls);
            report.EvidenceUrl = uploadedImageUrls.FirstOrDefault(); // legacy field for compatibility
            report.Status = UserReportStatus.Pending;
            report.CreatedAt = DateTime.UtcNow;
            report.ResolutionNote = null;
            report.ResolvedAt = null;
            report.ResolvedById = null;

            await _unitOfWork.UserReports.AddAsync(report);
            await _unitOfWork.SaveChangesAsync();
            await NotifyReviewersReportCreatedAsync(report);

            return _mapper.Map<UserReportResponse>(report);
        }

        public async Task<UserReportResponse> ResolveAsValidAsync(int reportId, string? resolutionNote)
        {
            var report = await _unitOfWork.UserReports.GetByIdAsync(reportId);
            if (report is null)
            {
                throw new KeyNotFoundException("User report not found.");
            }

            if (report.Status != UserReportStatus.Pending)
            {
                throw new InvalidOperationException($"Only Pending report can be updated. Current status: {report.Status}.");
            }

            var normalizedNote = string.IsNullOrWhiteSpace(resolutionNote)
                ? null
                : resolutionNote.Trim();

            report.Status = UserReportStatus.Resolved;
            report.ResolvedById = _userService.GetUserId();
            report.ResolvedAt = DateTime.UtcNow;
            report.ResolutionNote = normalizedNote;

            if (report.Booking is not null)
            {
                report.Booking.Status = BookingStatus.ComplaintAccepted;

                if (report.Booking.ChatSession is not null && report.Booking.ChatSession.IsOpen)
                {
                    report.Booking.ChatSession.IsOpen = false;
                    report.Booking.ChatSession.EndTime = DateTime.UtcNow;
                }
            }

            _unitOfWork.UserReports.Update(report);
            await _unitOfWork.SaveChangesAsync();
            await NotifyReportStatusChangedAsync(report);

            return _mapper.Map<UserReportResponse>(report);
        }

        public async Task<UserReportResponse> ResolveAsFalseAsync(int reportId, string? resolutionNote)
        {
            var report = await _unitOfWork.UserReports.GetByIdAsync(reportId);
            if (report is null)
            {
                throw new KeyNotFoundException("User report not found.");
            }

            if (report.Status != UserReportStatus.Pending)
            {
                throw new InvalidOperationException($"Only Pending report can be updated. Current status: {report.Status}.");
            }

            var normalizedNote = string.IsNullOrWhiteSpace(resolutionNote)
                ? null
                : resolutionNote.Trim();

            report.Status = UserReportStatus.Dismissed;
            report.ResolvedById = _userService.GetUserId();
            report.ResolvedAt = DateTime.UtcNow;
            report.ResolutionNote = normalizedNote;

            if (report.Booking is not null)
            {
                report.Booking.Status = BookingStatus.Completed;

                if (report.Booking.ChatSession is not null && report.Booking.ChatSession.IsOpen)
                {
                    report.Booking.ChatSession.IsOpen = false;
                    report.Booking.ChatSession.EndTime = DateTime.UtcNow;
                }
            }

            _unitOfWork.UserReports.Update(report);
            await _unitOfWork.SaveChangesAsync();
            await NotifyReportStatusChangedAsync(report);

            return _mapper.Map<UserReportResponse>(report);
        }

        public async Task<PagedResult<UserReportResponse>> GetUserReports(SieveModel sieveModel)
        {
            //return await PaginationHelper.PaginateAsync(query, model, _sieveProcessor, s => _mapper.Map<StartupResponse>(s));
            var query = _unitOfWork.UserReports.GetAll();
            return await PaginationHelper.PaginateAsync(query, sieveModel, _sieveProcessor, s=> _mapper.Map<UserReportResponse>(s));

        }

        public async Task<PagedResult<UserReportResponse>> GetMyReportsAsReporterAsync(SieveModel sieveModel)
        {
            var currentUserId = _userService.GetUserId();
            var query = _unitOfWork.UserReports.GetAll()
                .Where(x => x.ReporterId == currentUserId);

            return await PaginationHelper.PaginateAsync(
                query,
                sieveModel,
                _sieveProcessor,
                x => _mapper.Map<UserReportResponse>(x));
        }

        public async Task<PagedResult<UserReportResponse>> GetMyReportsAsReportedUserAsync(SieveModel sieveModel)
        {
            var currentUserId = _userService.GetUserId();
            var query = _unitOfWork.UserReports.GetAll()
                .Where(x =>
                    x.Booking != null &&
                    x.ReporterId != currentUserId &&
                    (x.Booking.CustomerId == currentUserId
                     || (x.Booking.Advisor != null && x.Booking.Advisor.UserId == currentUserId)));

            return await PaginationHelper.PaginateAsync(
                query,
                sieveModel,
                _sieveProcessor,
                x => _mapper.Map<UserReportResponse>(x));
        }

        private async Task NotifyReviewersReportCreatedAsync(UserReport report)
        {
            var reviewerIds = await _unitOfWork.Users.GetAllQuery()
                .Where(u => u.Role == UserRole.Staff || u.Role == UserRole.Admin)
                .Select(u => u.Id)
                .ToListAsync();

            if (reviewerIds.Count == 0)
            {
                return;
            }

            var title = "New user report pending review";
            var bookingPart = report.BookingId.HasValue
                ? " for a booking"
                : string.Empty;
            var message = $"A user report{bookingPart} is pending review.";

            foreach (var reviewerId in reviewerIds)
            {
                await _notificationService.SendNotificationAsync(
                    reviewerId,
                    title,
                    message,
                    NotificationType.System,
                    report.UserReportId,
                    "UserReport");
            }
        }

        private async Task NotifyReportStatusChangedAsync(UserReport report)
        {
            var statusText = report.Status.ToString();
            var suffix = string.IsNullOrWhiteSpace(report.ResolutionNote)
                ? string.Empty
                : $" Note: {report.ResolutionNote}";
            await _notificationService.SendNotificationAsync(
                report.ReporterId,
                "Your user report has been updated",
                $"Your report status is now {statusText}.{suffix}",
                NotificationType.General,
                report.UserReportId,
                "UserReport");

            await _notificationService.SendNotificationAsync(
                ResolveCounterpartyUserId(report),
                "A user report status has been updated",
                $"A report involving your account is now {statusText}.{suffix}",
                NotificationType.General,
                report.UserReportId,
                "UserReport");
        }

        private static int ResolveCounterpartyUserId(UserReport report)
        {
            if (report.Booking is null)
            {
                throw new InvalidOperationException("Report booking is missing.");
            }

            var advisorUserId = report.Booking.Advisor?.UserId
                ?? throw new InvalidOperationException("Report booking advisor account is missing.");
            var customerUserId = report.Booking.CustomerId;

            return report.ReporterId == customerUserId
                ? advisorUserId
                : customerUserId;
        }
    }
}

