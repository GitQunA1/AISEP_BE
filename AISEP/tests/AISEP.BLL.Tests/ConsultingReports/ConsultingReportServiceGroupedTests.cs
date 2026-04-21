using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;
using AISEP.BLL.Exceptions;
using AISEP.BLL.Services.ConsultingReports;
using AISEP.BLL.Services.Notifications;
using AISEP.BLL.Services.Users;
using AISEP.DAL.Common;
using AISEP.DAL.Entities;
using AISEP.DAL.Enums;
using AISEP.DAL.Repositories.Advisors;
using AISEP.DAL.Repositories.Bookings;
using AISEP.DAL.Repositories.ConsultingReports;
using AISEP.DAL.Repositories.Subscriptions;
using AISEP.DAL.Repositories.WalletTransactions;
using AutoMapper;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Sieve.Services;
using System.Linq.Expressions;
using Xunit;

namespace AISEP.BLL.Tests.ConsultingReports;

public class ConsultingReportServiceGroupedTests
{
    [Fact]
    public async Task UT130_CreateAsync_ShouldThrowForbidden_WhenCurrentUserIsNotAdvisor()
    {
        var (service, _, _, advisorRepository, _, _, _, userService, _, _, _) = CreateSut();
        userService.Setup(x => x.GetUserId()).Returns(5000);
        advisorRepository.Setup(x => x.GetByUserIdAsync(5000)).ReturnsAsync((Advisor?)null);

        var ex = await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            service.CreateAsync(BuildCreateRequest(200)));

        Assert.Contains("Only advisor can submit consulting report.", ex.Message);
    }

    [Fact]
    public async Task UT131_CreateAsync_ShouldThrow_WhenBookingNotFound()
    {
        var advisor = BuildAdvisor(advisorId: 10, userId: 5000);

        var (service, _, bookingRepository, advisorRepository, _, _, _, userService, _, _, _) = CreateSut();
        userService.Setup(x => x.GetUserId()).Returns(5000);
        advisorRepository.Setup(x => x.GetByUserIdAsync(5000)).ReturnsAsync(advisor);
        bookingRepository.Setup(x => x.GetByIdAsync(404)).ReturnsAsync((Booking?)null);

        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.CreateAsync(BuildCreateRequest(404)));

        Assert.Contains("Booking not found.", ex.Message);
    }

    [Fact]
    public async Task UT132_CreateAsync_ShouldThrowForbidden_WhenAdvisorNotAssignedToBooking()
    {
        var advisor = BuildAdvisor(advisorId: 10, userId: 5000);
        var booking = BuildBooking(bookingId: 201, advisorId: 11, customerId: 5000, status: BookingStatus.Confirmed, endTime: DateTime.UtcNow.AddHours(-1));

        var (service, _, bookingRepository, advisorRepository, _, _, _, userService, _, _, _) = CreateSut();
        userService.Setup(x => x.GetUserId()).Returns(5000);
        advisorRepository.Setup(x => x.GetByUserIdAsync(5000)).ReturnsAsync(advisor);
        bookingRepository.Setup(x => x.GetByIdAsync(201)).ReturnsAsync(booking);

        var ex = await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            service.CreateAsync(BuildCreateRequest(201)));

        Assert.Contains("You are not assigned to this booking.", ex.Message);
    }

    [Fact]
    public async Task UT133_CreateAsync_ShouldThrow_WhenBookingStatusIsNotConfirmed()
    {
        var advisor = BuildAdvisor(advisorId: 10, userId: 5000);
        var booking = BuildBooking(bookingId: 202, advisorId: 10, customerId: 5000, status: BookingStatus.Pending, endTime: DateTime.UtcNow.AddHours(-1));

        var (service, _, bookingRepository, advisorRepository, _, _, _, userService, _, _, _) = CreateSut();
        userService.Setup(x => x.GetUserId()).Returns(5000);
        advisorRepository.Setup(x => x.GetByUserIdAsync(5000)).ReturnsAsync(advisor);
        bookingRepository.Setup(x => x.GetByIdAsync(202)).ReturnsAsync(booking);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateAsync(BuildCreateRequest(202)));

        Assert.Contains("confirmed booking", ex.Message);
    }

    [Fact]
    public async Task UT134_CreateAsync_ShouldThrow_WhenSubmissionWindowExpired()
    {
        var advisor = BuildAdvisor(advisorId: 10, userId: 5000);
        var booking = BuildBooking(bookingId: 203, advisorId: 10, customerId: 5000, status: BookingStatus.Confirmed, endTime: DateTime.UtcNow.AddHours(-26));

        var (service, _, bookingRepository, advisorRepository, reportRepository, _, _, userService, _, _, _) = CreateSut();
        userService.Setup(x => x.GetUserId()).Returns(5000);
        advisorRepository.Setup(x => x.GetByUserIdAsync(5000)).ReturnsAsync(advisor);
        bookingRepository.Setup(x => x.GetByIdAsync(203)).ReturnsAsync(booking);
        reportRepository.Setup(x => x.GetByBookingIdAsync(203)).ReturnsAsync((ConsultingReport?)null);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateAsync(BuildCreateRequest(203)));

        Assert.Contains("Submission window", ex.Message);
    }

    [Fact]
    public async Task UT135_CreateAsync_ShouldCreateSubmittedReport_WhenNoExistingReport()
    {
        var advisor = BuildAdvisor(advisorId: 10, userId: 5000);
        var booking = BuildBooking(bookingId: 204, advisorId: 10, customerId: 5000, status: BookingStatus.Confirmed, endTime: DateTime.UtcNow.AddHours(-2));
        ConsultingReport? added = null;

        var (service, unitOfWork, bookingRepository, advisorRepository, reportRepository, _, _, userService, _, _, _) = CreateSut();
        userService.Setup(x => x.GetUserId()).Returns(5000);
        advisorRepository.Setup(x => x.GetByUserIdAsync(5000)).ReturnsAsync(advisor);
        bookingRepository.Setup(x => x.GetByIdAsync(204)).ReturnsAsync(booking);
        reportRepository.Setup(x => x.GetByBookingIdAsync(204)).ReturnsAsync((ConsultingReport?)null);
        reportRepository
            .Setup(x => x.AddAsync(It.IsAny<ConsultingReport>()))
            .Callback<ConsultingReport>(r =>
            {
                added = r;
                r.ConsultingReportId = 513;
            })
            .Returns(Task.CompletedTask);
        reportRepository.Setup(x => x.GetByIdAsync(513)).ReturnsAsync(() => added);

        var result = await service.CreateAsync(BuildCreateRequest(204));

        Assert.NotNull(added);
        Assert.Equal(ConsultingReportStatus.Submitted, added!.Status);
        Assert.NotNull(added.StartupReviewDueAt);
        Assert.Equal(513, result.ConsultingReportId);

        reportRepository.Verify(x => x.AddAsync(It.IsAny<ConsultingReport>()), Times.Once);
        reportRepository.Verify(x => x.Update(It.IsAny<ConsultingReport>()), Times.Never);
        unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UT136_CreateAsync_ShouldThrow_WhenExistingReportIsNotRevisionRequested()
    {
        var advisor = BuildAdvisor(advisorId: 10, userId: 5000);
        var booking = BuildBooking(bookingId: 205, advisorId: 10, customerId: 5000, status: BookingStatus.Confirmed, endTime: DateTime.UtcNow.AddHours(-1));
        var existingReport = BuildReport(614, booking, ConsultingReportStatus.Submitted);

        var (service, _, bookingRepository, advisorRepository, reportRepository, _, _, userService, _, _, _) = CreateSut([existingReport]);
        userService.Setup(x => x.GetUserId()).Returns(5000);
        advisorRepository.Setup(x => x.GetByUserIdAsync(5000)).ReturnsAsync(advisor);
        bookingRepository.Setup(x => x.GetByIdAsync(205)).ReturnsAsync(booking);
        reportRepository.Setup(x => x.GetByBookingIdAsync(205)).ReturnsAsync(existingReport);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateAsync(BuildCreateRequest(205)));

        Assert.Contains("already exists", ex.Message);
    }

    [Fact]
    public async Task UT137_CreateAsync_ShouldUpdateReport_WhenRevisionRequestedAndWithinDeadline()
    {
        var advisor = BuildAdvisor(advisorId: 10, userId: 5000);
        var booking = BuildBooking(bookingId: 206, advisorId: 10, customerId: 5000, status: BookingStatus.Confirmed, endTime: DateTime.UtcNow.AddHours(-1));
        var existingReport = BuildReport(615, booking, ConsultingReportStatus.RevisionRequested, revisionCount: 1);
        existingReport.AdvisorRevisionDueAt = DateTime.UtcNow.AddHours(12);
        existingReport.RevisionRequestReason = "Need more details";

        var (service, unitOfWork, bookingRepository, advisorRepository, reportRepository, _, _, userService, _, _, _) = CreateSut([existingReport]);
        userService.Setup(x => x.GetUserId()).Returns(5000);
        advisorRepository.Setup(x => x.GetByUserIdAsync(5000)).ReturnsAsync(advisor);
        bookingRepository.Setup(x => x.GetByIdAsync(206)).ReturnsAsync(booking);
        reportRepository.Setup(x => x.GetByBookingIdAsync(206)).ReturnsAsync(existingReport);
        reportRepository.Setup(x => x.GetByIdAsync(615)).ReturnsAsync(existingReport);

        var request = new CreateConsultingReportRequest
        {
            BookingId = 206,
            MeetingTitle = "Revision Meeting",
            Location = "Online",
            MeetingTime = DateTime.UtcNow,
            MeetingPurpose = "Update plan",
            Content = "Updated content",
            DecisionsMade = "Updated decisions"
        };

        var result = await service.CreateAsync(request);

        Assert.Equal("Revision Meeting", existingReport.MeetingTitle);
        Assert.Equal("Online", existingReport.Location);
        Assert.Equal(ConsultingReportStatus.Submitted, existingReport.Status);
        Assert.NotNull(existingReport.StartupReviewDueAt);
        Assert.Null(existingReport.AdvisorRevisionDueAt);
        Assert.Null(existingReport.RevisionRequestReason);
        Assert.Equal(615, result.ConsultingReportId);

        reportRepository.Verify(x => x.Update(existingReport), Times.Once);
        unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UT138_ApproveAsync_ShouldThrow_WhenReportStatusIsNotSubmitted()
    {
        var booking = BuildBooking(bookingId: 300, advisorId: 10, customerId: 5000, status: BookingStatus.Confirmed, endTime: DateTime.UtcNow.AddHours(-2));
        var report = BuildReport(700, booking, ConsultingReportStatus.RevisionRequested);

        var (service, unitOfWork, _, _, reportRepository, _, _, userService, _, _, _) = CreateSut([report]);
        userService.Setup(x => x.GetUserId()).Returns(5000);
        reportRepository.Setup(x => x.GetByIdAsync(700)).ReturnsAsync(report);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.ApproveAsync(700));

        Assert.Contains("Only submitted report can be approved.", ex.Message);
        unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UT139_ApproveAsync_ShouldCompleteBookingAndCloseChat_WhenApproved()
    {
        var booking = BuildBooking(
            bookingId: 301,
            advisorId: 10,
            customerId: 5000,
            status: BookingStatus.Confirmed,
            endTime: DateTime.UtcNow.AddHours(-2),
            chatOpen: true,
            price: 180m,
            advisorWalletBalance: 100m);
        var report = BuildReport(701, booking, ConsultingReportStatus.Submitted);

        var (service, unitOfWork, bookingRepository, _, reportRepository, _, _, userService, _, _, _) = CreateSut([report]);
        userService.Setup(x => x.GetUserId()).Returns(5000);
        reportRepository.Setup(x => x.GetByIdAsync(701)).ReturnsAsync(report);
        bookingRepository.Setup(x => x.GetByIdWithAdvisorWalletAsync(301)).ReturnsAsync(booking);

        _ = await service.ApproveAsync(701);

        Assert.Equal(ConsultingReportStatus.ApprovedByStartup, report.Status);
        Assert.Equal(BookingStatus.Completed, booking.Status);
        Assert.NotNull(booking.ChatSession);
        Assert.False(booking.ChatSession!.IsOpen);
        Assert.NotNull(booking.ChatSession.EndTime);

        unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UT140_ApproveAsync_ShouldDisburseAdvisorPayout_WhenApproved()
    {
        var booking = BuildBooking(
            bookingId: 302,
            advisorId: 10,
            customerId: 5000,
            status: BookingStatus.Confirmed,
            endTime: DateTime.UtcNow.AddHours(-2),
            chatOpen: false,
            price: 250m,
            advisorWalletBalance: 100m);
        var report = BuildReport(702, booking, ConsultingReportStatus.Submitted);
        WalletTransaction? addedTransaction = null;

        var (service, _, bookingRepository, _, reportRepository, _, walletTransactionRepository, userService, _, notificationService, _) = CreateSut([report]);
        userService.Setup(x => x.GetUserId()).Returns(5000);
        reportRepository.Setup(x => x.GetByIdAsync(702)).ReturnsAsync(report);
        bookingRepository.Setup(x => x.GetByIdWithAdvisorWalletAsync(302)).ReturnsAsync(booking);
        walletTransactionRepository
            .Setup(x => x.AddAsync(It.IsAny<WalletTransaction>()))
            .Callback<WalletTransaction>(tx => addedTransaction = tx)
            .Returns(Task.CompletedTask);

        var result = await service.ApproveAsync(702);

        Assert.True(report.IsPayoutProcessed);
        Assert.Equal(200m, report.AdvisorPayoutAmount);
        Assert.Equal(300m, booking.Advisor.Wallet!.Balance);
        Assert.NotNull(addedTransaction);
        Assert.Equal(200m, addedTransaction!.Amount);
        Assert.Equal(WalletTransactionType.Deposit, addedTransaction.Type);
        Assert.Equal(WalletTransactionStatus.Completed, addedTransaction.Status);
        Assert.True(result.IsPayoutProcessed);

        walletTransactionRepository.Verify(x => x.AddAsync(It.IsAny<WalletTransaction>()), Times.Once);
        notificationService.Verify(
            x => x.SendNotificationAsync(
                booking.Advisor.UserId,
                It.IsAny<string>(),
                It.IsAny<string>(),
                NotificationType.General,
                booking.BookingId,
                "Booking"),
            Times.Once);
    }

    [Fact]
    public async Task UT141_RequestRevisionAsync_ShouldThrow_WhenReportStatusIsNotSubmitted()
    {
        var booking = BuildBooking(bookingId: 401, advisorId: 10, customerId: 5000, status: BookingStatus.Confirmed, endTime: DateTime.UtcNow.AddHours(-2));
        var report = BuildReport(801, booking, ConsultingReportStatus.ApprovedByStartup);

        var (service, unitOfWork, _, _, reportRepository, _, _, userService, _, _, _) = CreateSut([report]);
        userService.Setup(x => x.GetUserId()).Returns(5000);
        reportRepository.Setup(x => x.GetByIdAsync(801)).ReturnsAsync(report);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.RequestRevisionAsync(801, "Need correction"));

        Assert.Contains("Only submitted report can be requested for revision.", ex.Message);
        unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UT142_RequestRevisionAsync_ShouldThrow_WhenRevisionCountReachedMax()
    {
        var booking = BuildBooking(bookingId: 402, advisorId: 10, customerId: 5000, status: BookingStatus.Confirmed, endTime: DateTime.UtcNow.AddHours(-2));
        var report = BuildReport(802, booking, ConsultingReportStatus.Submitted, revisionCount: 3);

        var (service, unitOfWork, _, _, reportRepository, _, _, userService, _, _, _) = CreateSut([report]);
        userService.Setup(x => x.GetUserId()).Returns(5000);
        reportRepository.Setup(x => x.GetByIdAsync(802)).ReturnsAsync(report);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.RequestRevisionAsync(802, "  Need clearer action items  "));

        Assert.Contains("Maximum revision requests reached", ex.Message);
        Assert.Equal(3, report.RevisionCount);
        unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Never);
        reportRepository.Verify(x => x.Update(report), Times.Never);
    }

    [Fact]
    public async Task UT143_RequestRevisionAsync_ShouldSetRevisionRequestedAndAdvisorDue_WhenUnderLimit()
    {
        var booking = BuildBooking(bookingId: 403, advisorId: 10, customerId: 5000, status: BookingStatus.Confirmed, endTime: DateTime.UtcNow.AddHours(-2));
        var report = BuildReport(803, booking, ConsultingReportStatus.Submitted, revisionCount: 1);

        var (service, unitOfWork, _, _, reportRepository, _, _, userService, _, _, _) = CreateSut([report]);
        userService.Setup(x => x.GetUserId()).Returns(5000);
        reportRepository.Setup(x => x.GetByIdAsync(803)).ReturnsAsync(report);

        var result = await service.RequestRevisionAsync(803, "  revise details  ");

        Assert.Equal(ConsultingReportStatus.RevisionRequested, report.Status);
        Assert.Equal(2, report.RevisionCount);
        Assert.Equal("revise details", report.RevisionRequestReason);
        Assert.NotNull(report.StartupReviewedAt);
        Assert.NotNull(report.AdvisorRevisionDueAt);
        Assert.InRange(report.AdvisorRevisionDueAt!.Value, DateTime.UtcNow.AddHours(23), DateTime.UtcNow.AddHours(25));
        Assert.Equal("RevisionRequested", result.Status);

        unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
        reportRepository.Verify(x => x.Update(report), Times.Once);
    }

    [Fact]
    public async Task UT147_ProcessReportDeadlinesAsync_ShouldAutoApprove_WhenStartupReviewTimesOut()
    {
        var booking = BuildBooking(
            bookingId: 501,
            advisorId: 10,
            customerId: 5000,
            status: BookingStatus.Confirmed,
            endTime: DateTime.UtcNow.AddHours(-5),
            chatOpen: true,
            price: 220m,
            advisorWalletBalance: 50m);
        var report = BuildReport(901, booking, ConsultingReportStatus.Submitted);
        report.StartupReviewDueAt = DateTime.UtcNow.AddMinutes(-1);

        var (service, unitOfWork, bookingRepository, _, reportRepository, _, _, _, _, _, _) = CreateSut([report]);
        bookingRepository.Setup(x => x.GetByIdWithAdvisorWalletAsync(501)).ReturnsAsync(booking);

        var affected = await service.ProcessReportDeadlinesAsync();

        Assert.Equal(1, affected);
        Assert.Equal(ConsultingReportStatus.ApprovedByStartup, report.Status);
        Assert.Equal(BookingStatus.Completed, booking.Status);
        Assert.NotNull(booking.ChatSession);
        Assert.False(booking.ChatSession!.IsOpen);
        Assert.True(report.IsPayoutProcessed);

        reportRepository.Verify(x => x.Update(report), Times.Once);
        unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UT148_ProcessReportDeadlinesAsync_ShouldKeepRevisionRequested_WhenAdvisorRevisionTimesOut()
    {
        var booking = BuildBooking(
            bookingId: 502,
            advisorId: 10,
            customerId: 5000,
            status: BookingStatus.Confirmed,
            endTime: DateTime.UtcNow.AddHours(-5),
            chatOpen: false,
            price: 220m,
            advisorWalletBalance: 50m);
        var report = BuildReport(902, booking, ConsultingReportStatus.RevisionRequested, revisionCount: 1);
        report.StartupReviewDueAt = null;
        report.AdvisorRevisionDueAt = DateTime.UtcNow.AddMinutes(-1);

        var (service, unitOfWork, bookingRepository, _, reportRepository, _, _, _, _, _, _) = CreateSut([report]);

        var affected = await service.ProcessReportDeadlinesAsync();

        Assert.Equal(1, affected);
        Assert.Equal(ConsultingReportStatus.RevisionRequested, report.Status);
        Assert.Null(report.AdvisorRevisionDueAt);
        Assert.Null(report.StartupReviewDueAt);

        reportRepository.Verify(x => x.Update(report), Times.Once);
        bookingRepository.Verify(x => x.GetByIdWithAdvisorWalletAsync(It.IsAny<int>()), Times.Never);
        unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UT149_ProcessReportDeadlinesAsync_ShouldMarkBookingOverdue_WhenInitialSubmissionTimesOut()
    {
        var overdueBooking = BuildBooking(
            bookingId: 503,
            advisorId: 10,
            customerId: 5000,
            status: BookingStatus.Confirmed,
            endTime: DateTime.UtcNow.AddHours(-25),
            chatOpen: true);

        var (service, unitOfWork, bookingRepository, _, reportRepository, _, _, _, _, _, _) = CreateSut();
        bookingRepository
            .Setup(x => x.GetConfirmedWithoutConsultingReportPastDueAsync(It.IsAny<DateTime>()))
            .ReturnsAsync([overdueBooking]);

        var affected = await service.ProcessReportDeadlinesAsync();

        Assert.Equal(1, affected);
        Assert.Equal(BookingStatus.ConsultingReportOverdue, overdueBooking.Status);
        reportRepository.Verify(x => x.Update(It.IsAny<ConsultingReport>()), Times.Never);
        unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    private static (
        ConsultingReportService Service,
        Mock<IUnitOfWork> UnitOfWork,
        Mock<IBookingRepository> BookingRepository,
        Mock<IAdvisorsRepository> AdvisorRepository,
        Mock<IConsultingReportRepository> ReportRepository,
        Mock<ISubscriptionRepository> SubscriptionRepository,
        Mock<IWalletTransactionRepository> WalletTransactionRepository,
        Mock<IUserService> UserService,
        Mock<IMapper> Mapper,
        Mock<INotificationService> NotificationService,
        Mock<ISieveProcessor> SieveProcessor) CreateSut(
        IEnumerable<ConsultingReport>? seededReports = null,
        IEnumerable<Subscription>? seededSubscriptions = null)
    {
        var reports = seededReports?.ToList() ?? [];
        var subscriptions = seededSubscriptions?.ToList() ?? [];

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var bookingRepositoryMock = new Mock<IBookingRepository>();
        var advisorRepositoryMock = new Mock<IAdvisorsRepository>();
        var reportRepositoryMock = new Mock<IConsultingReportRepository>();
        var subscriptionRepositoryMock = new Mock<ISubscriptionRepository>();
        var walletTransactionRepositoryMock = new Mock<IWalletTransactionRepository>();
        var userServiceMock = new Mock<IUserService>();
        var mapperMock = new Mock<IMapper>();
        var notificationServiceMock = new Mock<INotificationService>();
        var sieveProcessorMock = new Mock<ISieveProcessor>();

        unitOfWorkMock.SetupGet(x => x.Bookings).Returns(bookingRepositoryMock.Object);
        unitOfWorkMock.SetupGet(x => x.Advisors).Returns(advisorRepositoryMock.Object);
        unitOfWorkMock.SetupGet(x => x.ConsultingReports).Returns(reportRepositoryMock.Object);
        unitOfWorkMock.SetupGet(x => x.Subscriptions).Returns(subscriptionRepositoryMock.Object);
        unitOfWorkMock.SetupGet(x => x.WalletTransactions).Returns(walletTransactionRepositoryMock.Object);
        unitOfWorkMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        reportRepositoryMock
            .Setup(x => x.GetQuery())
            .Returns(() => new TestAsyncEnumerable<ConsultingReport>(reports.AsQueryable()));
        reportRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((int id) => reports.FirstOrDefault(r => r.ConsultingReportId == id));
        reportRepositoryMock
            .Setup(x => x.GetByBookingIdAsync(It.IsAny<int>()))
            .ReturnsAsync((int bookingId) => reports.FirstOrDefault(r => r.BookingId == bookingId));
        reportRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<ConsultingReport>()))
            .Callback<ConsultingReport>(report =>
            {
                if (report.ConsultingReportId <= 0)
                {
                    report.ConsultingReportId = (reports.LastOrDefault()?.ConsultingReportId ?? 1000) + 1;
                }

                reports.Add(report);
            })
            .Returns(Task.CompletedTask);
        reportRepositoryMock
            .Setup(x => x.Update(It.IsAny<ConsultingReport>()))
            .Callback<ConsultingReport>(updated =>
            {
                var index = reports.FindIndex(r => r.ConsultingReportId == updated.ConsultingReportId);
                if (index >= 0)
                {
                    reports[index] = updated;
                }
            });

        subscriptionRepositoryMock
            .Setup(x => x.GetQuery())
            .Returns(() => new TestAsyncEnumerable<Subscription>(subscriptions.AsQueryable()));

        bookingRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Booking?)null);
        bookingRepositoryMock.Setup(x => x.GetByIdWithAdvisorWalletAsync(It.IsAny<int>())).ReturnsAsync((Booking?)null);
        bookingRepositoryMock.Setup(x => x.GetConfirmedWithoutConsultingReportPastDueAsync(It.IsAny<DateTime>())).ReturnsAsync([]);
        walletTransactionRepositoryMock.Setup(x => x.AddAsync(It.IsAny<WalletTransaction>())).Returns(Task.CompletedTask);

        advisorRepositoryMock
            .Setup(x => x.GetByUserIdAsync(It.IsAny<int>()))
            .ReturnsAsync((int userId) => BuildAdvisor(advisorId: 10, userId: userId));

        userServiceMock.Setup(x => x.GetUserId()).Returns(5000);
        userServiceMock.Setup(x => x.GetUserRole()).Returns("Startup");

        mapperMock
            .Setup(x => x.Map<ConsultingReport>(It.IsAny<CreateConsultingReportRequest>()))
            .Returns<CreateConsultingReportRequest>(request => new ConsultingReport
            {
                BookingId = request.BookingId,
                MeetingTitle = request.MeetingTitle,
                Location = request.Location,
                MeetingTime = request.MeetingTime,
                MeetingPurpose = request.MeetingPurpose,
                Content = request.Content,
                DecisionsMade = request.DecisionsMade
            });

        mapperMock
            .Setup(x => x.Map<ConsultingReportResponse>(It.IsAny<ConsultingReport>()))
            .Returns<ConsultingReport>(report => new ConsultingReportResponse
            {
                ConsultingReportId = report.ConsultingReportId,
                BookingId = report.BookingId,
                MeetingTitle = report.MeetingTitle,
                Location = report.Location,
                MeetingTime = report.MeetingTime,
                MeetingPurpose = report.MeetingPurpose,
                Content = report.Content,
                DecisionsMade = report.DecisionsMade,
                Status = report.Status.ToString(),
                RevisionCount = report.RevisionCount,
                RevisionRequestReason = report.RevisionRequestReason,
                LastSubmittedAt = report.LastSubmittedAt,
                StartupReviewDueAt = report.StartupReviewDueAt,
                AdvisorRevisionDueAt = report.AdvisorRevisionDueAt,
                StartupReviewedAt = report.StartupReviewedAt,
                IsPayoutProcessed = report.IsPayoutProcessed,
                AdvisorPayoutAmount = report.AdvisorPayoutAmount,
                PayoutProcessedAt = report.PayoutProcessedAt,
                CreatedAt = report.CreatedAt,
                AdvisorId = report.Booking?.AdvisorId ?? 0,
                CustomerId = report.Booking?.CustomerId ?? 0
            });

        var service = new ConsultingReportService(
            unitOfWorkMock.Object,
            userServiceMock.Object,
            mapperMock.Object,
            sieveProcessorMock.Object,
            notificationServiceMock.Object);

        return (
            service,
            unitOfWorkMock,
            bookingRepositoryMock,
            advisorRepositoryMock,
            reportRepositoryMock,
            subscriptionRepositoryMock,
            walletTransactionRepositoryMock,
            userServiceMock,
            mapperMock,
            notificationServiceMock,
            sieveProcessorMock);
    }

    private static CreateConsultingReportRequest BuildCreateRequest(int bookingId)
    {
        return new CreateConsultingReportRequest
        {
            BookingId = bookingId,
            MeetingTitle = "Consulting Session",
            Location = "Online",
            MeetingTime = DateTime.UtcNow,
            MeetingPurpose = "Product strategy",
            Content = "Discuss roadmap",
            DecisionsMade = "Defined milestones"
        };
    }

    private static ConsultingReport BuildReport(
        int reportId,
        Booking booking,
        ConsultingReportStatus status,
        int revisionCount = 0)
    {
        return new ConsultingReport
        {
            ConsultingReportId = reportId,
            BookingId = booking.BookingId,
            Booking = booking,
            MeetingTitle = "Weekly review",
            Location = "Online",
            MeetingTime = DateTime.UtcNow.AddHours(-1),
            MeetingPurpose = "Track progress",
            Content = "Progress details",
            DecisionsMade = "Follow-up tasks",
            Status = status,
            RevisionCount = revisionCount,
            LastSubmittedAt = DateTime.UtcNow.AddHours(-1),
            StartupReviewDueAt = DateTime.UtcNow.AddHours(2),
            CreatedAt = DateTime.UtcNow.AddHours(-2)
        };
    }

    private static Booking BuildBooking(
        int bookingId,
        int advisorId,
        int customerId,
        BookingStatus status,
        DateTime endTime,
        bool chatOpen = false,
        decimal price = 100m,
        decimal advisorWalletBalance = 0m)
    {
        var advisorUserId = 9000 + advisorId;
        var advisor = BuildAdvisor(advisorId, advisorUserId, advisorWalletBalance);

        var booking = new Booking
        {
            BookingId = bookingId,
            AdvisorId = advisorId,
            CustomerId = customerId,
            Advisor = advisor,
            Customer = new User
            {
                Id = customerId,
                UserName = $"customer-{customerId}",
                Email = $"customer{customerId}@test.local",
                Role = UserRole.Startup,
                Status = UserStatus.Active,
                CreatedAt = DateTime.UtcNow
            },
            StartTime = endTime.AddHours(-1),
            EndTime = endTime,
            Price = price,
            Status = status,
            CreatedAt = DateTime.UtcNow.AddDays(-2)
        };

        if (chatOpen)
        {
            booking.ChatSession = new ChatSession
            {
                ChatSessionId = 3000 + bookingId,
                BookingId = bookingId,
                IsOpen = true,
                StartTime = DateTime.UtcNow.AddHours(-5)
            };
        }

        return booking;
    }

    private static Advisor BuildAdvisor(int advisorId, int userId, decimal walletBalance = 0m)
    {
        var advisor = new Advisor
        {
            AdvisorId = advisorId,
            UserId = userId,
            ApprovalStatus = ApprovalStatus.Approved,
            User = new User
            {
                Id = userId,
                UserName = $"advisor-{advisorId}",
                Email = $"advisor{advisorId}@test.local",
                Role = UserRole.Advisor,
                Status = UserStatus.Active,
                CreatedAt = DateTime.UtcNow
            }
        };

        advisor.Wallet = new Wallet
        {
            WalletId = 1000 + advisorId,
            AdvisorId = advisorId,
            Balance = walletBalance,
            Currency = "VND",
            IsActive = true,
            Advisor = advisor
        };

        return advisor;
    }

    private sealed class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
    {
        private readonly IQueryProvider _inner;

        public TestAsyncQueryProvider(IQueryProvider inner)
        {
            _inner = inner;
        }

        public IQueryable CreateQuery(Expression expression)
        {
            return new TestAsyncEnumerable<TEntity>(expression);
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new TestAsyncEnumerable<TElement>(expression);
        }

        public object? Execute(Expression expression)
        {
            return _inner.Execute(expression);
        }

        public TResult Execute<TResult>(Expression expression)
        {
            return _inner.Execute<TResult>(expression);
        }

        public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
        {
            var expectedResultType = typeof(TResult).GetGenericArguments()[0];
            var executionResult = typeof(IQueryProvider)
                .GetMethod(nameof(IQueryProvider.Execute), 1, new[] { typeof(Expression) })!
                .MakeGenericMethod(expectedResultType)
                .Invoke(_inner, new[] { expression });

            return (TResult)typeof(Task)
                .GetMethod(nameof(Task.FromResult))!
                .MakeGenericMethod(expectedResultType)
                .Invoke(null, new[] { executionResult })!;
        }
    }

    private sealed class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
    {
        public TestAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable)
        {
        }

        public TestAsyncEnumerable(Expression expression) : base(expression)
        {
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new TestAsyncEnumerator<T>(this.ToList().GetEnumerator());
        }

        IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
    }

    private sealed class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;

        public TestAsyncEnumerator(IEnumerator<T> inner)
        {
            _inner = inner;
        }

        public T Current => _inner.Current;

        public ValueTask DisposeAsync()
        {
            _inner.Dispose();
            return ValueTask.CompletedTask;
        }

        public ValueTask<bool> MoveNextAsync()
        {
            return new ValueTask<bool>(_inner.MoveNext());
        }
    }
}
