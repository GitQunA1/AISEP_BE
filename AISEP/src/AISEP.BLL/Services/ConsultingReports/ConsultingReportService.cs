using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;
using AISEP.BLL.Exceptions;
using AISEP.BLL.Services.Users;
using AISEP.DAL.Common;
using AISEP.DAL.Entities;
using AISEP.DAL.Enums;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace AISEP.BLL.Services.ConsultingReports
{
    public class ConsultingReportService : IConsultingReportService
    {
        private static readonly TimeSpan AdvisorSubmitWindow = TimeSpan.FromHours(24);
        private static readonly TimeSpan StartupReviewWindow = TimeSpan.FromHours(24);
        private const int MaxRevisionCount = 3;
        private const decimal AdvisorPayoutRate = 0.8m;

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
                ?? throw new ForbiddenAccessException("Only advisor can submit consulting report.");

            var booking = await _unitOfWork.Bookings.GetByIdAsync(request.BookingId)
                ?? throw new KeyNotFoundException("Booking not found.");

            if (booking.AdvisorId != advisor.AdvisorId)
                throw new ForbiddenAccessException("You are not assigned to this booking.");

            if (booking.Status != BookingStatus.Confirmed)
                throw new InvalidOperationException("Consulting report can only be submitted for confirmed booking.");

            var now = DateTime.UtcNow;
            var report = await _unitOfWork.ConsultingReports.GetByBookingIdAsync(request.BookingId);

            if (report is null)
            {
                if (now > booking.EndTime.Add(AdvisorSubmitWindow))
                    throw new InvalidOperationException("Submission window (24h after booking end time) has expired.");

                report = _mapper.Map<ConsultingReport>(request);
                report.CreatedAt = now;
                report.LastSubmittedAt = now;
                report.StartupReviewDueAt = now.Add(StartupReviewWindow);
                report.Status = ConsultingReportStatus.Submitted;

                await _unitOfWork.ConsultingReports.AddAsync(report);
                await _unitOfWork.SaveChangesAsync();
            }
            else
            {
                if (report.Status != ConsultingReportStatus.RevisionRequested)
                    throw new InvalidOperationException("Consulting report already exists for this booking.");

                if (report.AdvisorRevisionDueAt.HasValue && now > report.AdvisorRevisionDueAt.Value)
                    throw new InvalidOperationException("Revision window has expired. This report is escalated to staff.");

                report.MeetingTitle = request.MeetingTitle;
                report.Location = request.Location;
                report.MeetingTime = request.MeetingTime;
                report.MeetingPurpose = request.MeetingPurpose;
                report.Content = request.Content;
                report.DecisionsMade = request.DecisionsMade;
                report.LastSubmittedAt = now;
                report.StartupReviewDueAt = now.Add(StartupReviewWindow);
                report.AdvisorRevisionDueAt = null;
                report.StartupReviewedAt = null;
                report.RevisionRequestReason = null;
                report.Status = ConsultingReportStatus.Submitted;

                _unitOfWork.ConsultingReports.Update(report);
                await _unitOfWork.SaveChangesAsync();
            }

            var created = await _unitOfWork.ConsultingReports.GetByIdAsync(report.ConsultingReportId)
                ?? throw new InvalidOperationException("Failed to load consulting report.");

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

        public async Task<ConsultingReportResponse> ApproveAsync(int reportId)
        {
            var report = await _unitOfWork.ConsultingReports.GetByIdAsync(reportId)
                ?? throw new KeyNotFoundException("Consulting report not found.");

            EnsureCurrentUserIsBookingCustomer(report.Booking);

            if (report.Status != ConsultingReportStatus.Submitted)
                throw new InvalidOperationException("Only submitted report can be approved.");

            var now = DateTime.UtcNow;
            report.Status = ConsultingReportStatus.ApprovedByStartup;
            report.StartupReviewedAt = now;
            report.StartupReviewDueAt = null;
            report.AdvisorRevisionDueAt = null;

            MarkBookingCompletedAndCloseChat(report.Booking);
            await DisburseAdvisorAsync(report);

            _unitOfWork.ConsultingReports.Update(report);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ConsultingReportResponse>(report);
        }

        public async Task<ConsultingReportResponse> RequestRevisionAsync(int reportId, string reason)
        {
            var report = await _unitOfWork.ConsultingReports.GetByIdAsync(reportId)
                ?? throw new KeyNotFoundException("Consulting report not found.");

            EnsureCurrentUserIsBookingCustomer(report.Booking);

            if (report.Status != ConsultingReportStatus.Submitted)
                throw new InvalidOperationException("Only submitted report can be requested for revision.");

            if (report.RevisionCount >= MaxRevisionCount)
            {
                report.Status = ConsultingReportStatus.EscalatedToStaff;
                report.RevisionRequestReason = reason.Trim();
                report.StartupReviewedAt = DateTime.UtcNow;
                report.StartupReviewDueAt = null;
                report.AdvisorRevisionDueAt = null;
                _unitOfWork.ConsultingReports.Update(report);
                await _unitOfWork.SaveChangesAsync();
                return _mapper.Map<ConsultingReportResponse>(report);
            }

            var now = DateTime.UtcNow;
            report.RevisionCount += 1;
            report.Status = ConsultingReportStatus.RevisionRequested;
            report.RevisionRequestReason = reason.Trim();
            report.StartupReviewedAt = now;
            report.StartupReviewDueAt = null;
            report.AdvisorRevisionDueAt = now.Add(AdvisorSubmitWindow);

            _unitOfWork.ConsultingReports.Update(report);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ConsultingReportResponse>(report);
        }

        public async Task<int> ProcessReportDeadlinesAsync()
        {
            var now = DateTime.UtcNow;
            var changed = 0;

            var startupTimedOutReports = await _unitOfWork.ConsultingReports.GetQuery()
                .Where(r => r.Status == ConsultingReportStatus.Submitted
                            && r.StartupReviewDueAt.HasValue
                            && r.StartupReviewDueAt.Value <= now)
                .ToListAsync();

            foreach (var report in startupTimedOutReports)
            {
                report.Status = ConsultingReportStatus.ApprovedByStartup;
                report.StartupReviewedAt = now;
                report.StartupReviewDueAt = null;
                report.AdvisorRevisionDueAt = null;
                MarkBookingCompletedAndCloseChat(report.Booking);
                await DisburseAdvisorAsync(report);
                _unitOfWork.ConsultingReports.Update(report);
                changed += 1;
            }

            var advisorTimedOutReports = await _unitOfWork.ConsultingReports.GetQuery()
                .Where(r => r.Status == ConsultingReportStatus.RevisionRequested
                            && r.AdvisorRevisionDueAt.HasValue
                            && r.AdvisorRevisionDueAt.Value <= now)
                .ToListAsync();

            foreach (var report in advisorTimedOutReports)
            {
                report.Status = ConsultingReportStatus.EscalatedToStaff;
                report.AdvisorRevisionDueAt = null;
                report.StartupReviewDueAt = null;
                _unitOfWork.ConsultingReports.Update(report);
                changed += 1;
            }

            if (changed > 0)
            {
                await _unitOfWork.SaveChangesAsync();
            }

            return changed;
        }

        private async Task DisburseAdvisorAsync(ConsultingReport report)
        {
            if (report.IsPayoutProcessed)
            {
                return;
            }

            var bookingWithWallet = await _unitOfWork.Bookings.GetByIdWithAdvisorWalletAsync(report.BookingId)
                ?? throw new KeyNotFoundException("Booking not found for payout.");

            if (bookingWithWallet.Advisor?.Wallet is null)
                throw new InvalidOperationException("Advisor wallet not found.");

            var payoutAmount = Math.Round(bookingWithWallet.Price * AdvisorPayoutRate, 2, MidpointRounding.AwayFromZero);
            if (payoutAmount > 0)
            {
                bookingWithWallet.Advisor.Wallet.Balance += payoutAmount;

                await _unitOfWork.WalletTransactions.AddAsync(new WalletTransaction
                {
                    WalletId = bookingWithWallet.Advisor.Wallet.WalletId,
                    Amount = payoutAmount,
                    Type = WalletTransactionType.Deposit,
                    Status = WalletTransactionStatus.Completed,
                    CreatedAt = DateTime.UtcNow
                });
            }

            report.IsPayoutProcessed = true;
            report.AdvisorPayoutAmount = payoutAmount;
            report.PayoutProcessedAt = DateTime.UtcNow;
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

        private void EnsureCurrentUserIsBookingCustomer(Booking booking)
        {
            var userId = _userService.GetUserId();
            if (booking.CustomerId != userId)
                throw new ForbiddenAccessException("Only booking customer can review this report.");
        }

        private static void MarkBookingCompletedAndCloseChat(Booking booking)
        {
            booking.Status = BookingStatus.Completed;

            if (booking.ChatSession is not null && booking.ChatSession.IsOpen)
            {
                booking.ChatSession.IsOpen = false;
                booking.ChatSession.EndTime = DateTime.UtcNow;
            }
        }
    }
}
