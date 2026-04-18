using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;
using AISEP.BLL.Services.Bookings;
using AISEP.BLL.Services.Notifications;
using AISEP.BLL.Services.Users;
using AISEP.DAL.Common;
using AISEP.DAL.Entities;
using AISEP.DAL.Enums;
using AISEP.DAL.Repositories.AdvisorAvailabilities;
using AISEP.DAL.Repositories.Advisors;
using AISEP.DAL.Repositories.Bookings;
using AISEP.DAL.Repositories.BookingSlots;
using AISEP.DAL.Repositories.PremiumFreeBookingUsageLogs;
using AISEP.DAL.Repositories.Projects;
using AISEP.DAL.Repositories.ProjectAdvisorAssignments;
using AISEP.DAL.Repositories.Subscriptions;
using AISEP.DAL.Repositories.SystemCommissionConfigs;
using AutoMapper;
using Moq;
using Sieve.Services;
using Xunit;

namespace AISEP.BLL.Tests.Bookings;

public class BookingServiceGroupedTests
{
    [Fact]
    public async Task UT108_CreateBookingAsync_ShouldThrow_WhenNoSlotSelected()
    {
        var (service, _, _, _, _, _, _, _, _, _, _, _, _, _, _) = CreateSut();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateBookingAsync(new CreateBookingRequest
            {
                AdvisorId = 10,
                ProjectId = 20,
                AdvisorAvailabilitySlotIds = []
            }));

        Assert.Contains("At least one slot must be selected.", ex.Message);
    }

    [Fact]
    public async Task UT109_CreateBookingAsync_ShouldThrow_WhenAdvisorNotFound()
    {
        var (service, _, _, advisorRepo, _, _, _, _, _, _, _, _, _, _, _) = CreateSut();
        advisorRepo.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((Advisor?)null);

        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.CreateBookingAsync(BuildCreateRequest(advisorId: 999, projectId: 20, slotIds: [1])));

        Assert.Contains("Advisor not found.", ex.Message);
    }

    [Fact]
    public async Task UT110_CreateBookingAsync_ShouldThrow_WhenProjectNotFound()
    {
        var (service, _, _, advisorRepo, projectRepo, _, _, _, _, _, _, _, _, _, _) = CreateSut();
        advisorRepo.Setup(x => x.GetByIdAsync(10)).ReturnsAsync(BuildAdvisor(10, 9000));
        projectRepo.Setup(x => x.GetByIdAsync(404)).ReturnsAsync((Project?)null);

        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.CreateBookingAsync(BuildCreateRequest(advisorId: 10, projectId: 404, slotIds: [1])));

        Assert.Contains("Project not found.", ex.Message);
    }

    [Fact]
    public async Task UT111_CreateBookingAsync_ShouldThrow_WhenProjectHasNoAdvisorAssignment()
    {
        var (service, _, _, advisorRepo, projectRepo, assignmentRepo, _, _, _, _, _, _, _, _, _) = CreateSut();
        advisorRepo.Setup(x => x.GetByIdAsync(10)).ReturnsAsync(BuildAdvisor(10, 9000));
        projectRepo.Setup(x => x.GetByIdAsync(20)).ReturnsAsync(BuildProject(20, ProjectStatus.Approved));
        assignmentRepo.Setup(x => x.GetByProjectIdAsync(20)).ReturnsAsync([]);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateBookingAsync(BuildCreateRequest(advisorId: 10, projectId: 20, slotIds: [1])));

        Assert.Contains("Project has not been assigned to any advisor yet.", ex.Message);
    }

    [Fact]
    public async Task UT112_CreateBookingAsync_ShouldThrow_WhenSelectedSlotIdsContainMissingItems()
    {
        var advisor = BuildAdvisor(10, 9000);
        var project = BuildProject(20, ProjectStatus.Approved);
        var availableSlot = BuildSlot(1, 10, DateTime.UtcNow.Date.AddDays(2), 9, 10);

        var (service, _, _, advisorRepo, projectRepo, assignmentRepo, availabilityRepo, _, _, _, _, _, _, _, _) = CreateSut();
        advisorRepo.Setup(x => x.GetByIdAsync(10)).ReturnsAsync(advisor);
        projectRepo.Setup(x => x.GetByIdAsync(20)).ReturnsAsync(project);
        assignmentRepo.Setup(x => x.GetByProjectIdAsync(20)).ReturnsAsync([BuildAssignment(project.ProjectId, advisor.AdvisorId, advisor)]);
        availabilityRepo.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<int>>())).ReturnsAsync([availableSlot]);

        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.CreateBookingAsync(BuildCreateRequest(advisorId: 10, projectId: 20, slotIds: [1, 2])));

        Assert.Contains("One or more selected slots were not found.", ex.Message);
    }

    [Fact]
    public async Task UT113_CreateBookingAsync_ShouldThrow_WhenSelectedSlotsBelongToDifferentAdvisor()
    {
        var advisor = BuildAdvisor(10, 9000);
        var project = BuildProject(20, ProjectStatus.Approved);
        var slots = new List<AdvisorAvailability>
        {
            BuildSlot(1, 10, DateTime.UtcNow.Date.AddDays(2), 9, 10),
            BuildSlot(2, 11, DateTime.UtcNow.Date.AddDays(2), 10, 11)
        };

        var (service, _, _, advisorRepo, projectRepo, assignmentRepo, availabilityRepo, _, _, _, _, _, _, _, _) = CreateSut();
        advisorRepo.Setup(x => x.GetByIdAsync(10)).ReturnsAsync(advisor);
        projectRepo.Setup(x => x.GetByIdAsync(20)).ReturnsAsync(project);
        assignmentRepo.Setup(x => x.GetByProjectIdAsync(20)).ReturnsAsync([BuildAssignment(project.ProjectId, advisor.AdvisorId, advisor)]);
        availabilityRepo.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<int>>())).ReturnsAsync(slots);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateBookingAsync(BuildCreateRequest(advisorId: 10, projectId: 20, slotIds: [1, 2])));

        Assert.Contains("All selected slots must belong to the same advisor.", ex.Message);
    }

    [Fact]
    public async Task UT114_CreateBookingAsync_ShouldThrow_WhenAnySelectedSlotNotAvailable()
    {
        var advisor = BuildAdvisor(10, 9000);
        var project = BuildProject(20, ProjectStatus.Approved);
        var slot = BuildSlot(1, 10, DateTime.UtcNow.Date.AddDays(2), 9, 10, AdvisorAvailabilityStatus.Booked);

        var (service, _, _, advisorRepo, projectRepo, assignmentRepo, availabilityRepo, _, _, _, _, _, _, _, _) = CreateSut();
        advisorRepo.Setup(x => x.GetByIdAsync(10)).ReturnsAsync(advisor);
        projectRepo.Setup(x => x.GetByIdAsync(20)).ReturnsAsync(project);
        assignmentRepo.Setup(x => x.GetByProjectIdAsync(20)).ReturnsAsync([BuildAssignment(project.ProjectId, advisor.AdvisorId, advisor)]);
        availabilityRepo.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<int>>())).ReturnsAsync([slot]);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateBookingAsync(BuildCreateRequest(advisorId: 10, projectId: 20, slotIds: [1])));

        Assert.Contains("One or more selected slots are no longer available.", ex.Message);
    }

    [Fact]
    public async Task UT115_CreateBookingAsync_ShouldThrow_WhenAnySelectedSlotInPast()
    {
        var advisor = BuildAdvisor(10, 9000);
        var project = BuildProject(20, ProjectStatus.Approved);
        var oldSlot = BuildSlot(1, 10, DateTime.UtcNow.Date.AddDays(-1), 9, 10);

        var (service, _, _, advisorRepo, projectRepo, assignmentRepo, availabilityRepo, _, _, _, _, _, _, _, _) = CreateSut();
        advisorRepo.Setup(x => x.GetByIdAsync(10)).ReturnsAsync(advisor);
        projectRepo.Setup(x => x.GetByIdAsync(20)).ReturnsAsync(project);
        assignmentRepo.Setup(x => x.GetByProjectIdAsync(20)).ReturnsAsync([BuildAssignment(project.ProjectId, advisor.AdvisorId, advisor)]);
        availabilityRepo.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<int>>())).ReturnsAsync([oldSlot]);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateBookingAsync(BuildCreateRequest(advisorId: 10, projectId: 20, slotIds: [1])));

        Assert.Contains("Selected slots must be in the future.", ex.Message);
    }

    [Fact]
    public async Task UT116_CreateBookingAsync_ShouldThrow_WhenSelectedSlotsAreNotConsecutive()
    {
        var advisor = BuildAdvisor(10, 9000);
        var project = BuildProject(20, ProjectStatus.Approved);
        var slots = new List<AdvisorAvailability>
        {
            BuildSlot(1, 10, DateTime.UtcNow.Date.AddDays(2), 9, 10),
            BuildSlot(2, 10, DateTime.UtcNow.Date.AddDays(2), 11, 12)
        };

        var (service, _, _, advisorRepo, projectRepo, assignmentRepo, availabilityRepo, _, _, _, _, _, _, _, _) = CreateSut();
        advisorRepo.Setup(x => x.GetByIdAsync(10)).ReturnsAsync(advisor);
        projectRepo.Setup(x => x.GetByIdAsync(20)).ReturnsAsync(project);
        assignmentRepo.Setup(x => x.GetByProjectIdAsync(20)).ReturnsAsync([BuildAssignment(project.ProjectId, advisor.AdvisorId, advisor)]);
        availabilityRepo.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<int>>())).ReturnsAsync(slots);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateBookingAsync(BuildCreateRequest(advisorId: 10, projectId: 20, slotIds: [1, 2])));

        Assert.Contains("Selected slots must be consecutive in time.", ex.Message);
    }

    [Fact]
    public async Task UT117_CreateBookingAsync_ShouldThrow_WhenBookingLessThan12HoursInAdvance()
    {
        var advisor = BuildAdvisor(10, 9000);
        var project = BuildProject(20, ProjectStatus.Approved);
        var nearStart = DateTime.UtcNow.AddHours(2);
        var nearSlot = new AdvisorAvailability
        {
            AdvisorAvailabilityId = 1,
            AdvisorId = 10,
            SlotDate = nearStart.Date,
            StartTime = TimeOnly.FromDateTime(nearStart),
            EndTime = TimeOnly.FromDateTime(nearStart.AddHours(1)),
            Status = AdvisorAvailabilityStatus.Available
        };

        var (service, _, _, advisorRepo, projectRepo, assignmentRepo, availabilityRepo, _, _, _, _, _, _, _, _) = CreateSut();
        advisorRepo.Setup(x => x.GetByIdAsync(10)).ReturnsAsync(advisor);
        projectRepo.Setup(x => x.GetByIdAsync(20)).ReturnsAsync(project);
        assignmentRepo.Setup(x => x.GetByProjectIdAsync(20)).ReturnsAsync([BuildAssignment(project.ProjectId, advisor.AdvisorId, advisor)]);
        availabilityRepo.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<int>>())).ReturnsAsync([nearSlot]);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateBookingAsync(BuildCreateRequest(advisorId: 10, projectId: 20, slotIds: [1])));

        Assert.Contains("Bookings must be made at least 12 hours in advance.", ex.Message);
    }

    [Fact]
    public async Task UT118_CreateBookingAsync_ShouldThrow_WhenFreeRebookFromComplaintAlreadyUsed()
    {
        var advisor = BuildAdvisor(10, 9000);
        var project = BuildProject(20, ProjectStatus.Approved);
        var slots = BuildConsecutiveSlots(advisor.AdvisorId, DateTime.UtcNow.Date.AddDays(2), 9, 2);
        var sourceBooking = new Booking
        {
            BookingId = 501,
            CustomerId = 5000,
            ProjectId = 20,
            AdvisorId = 10,
            Status = BookingStatus.ComplaintAccepted
        };

        var (service, _, bookingRepo, advisorRepo, projectRepo, assignmentRepo, availabilityRepo, _, _, _, _, _, _, _, _) = CreateSut();
        advisorRepo.Setup(x => x.GetByIdAsync(10)).ReturnsAsync(advisor);
        projectRepo.Setup(x => x.GetByIdAsync(20)).ReturnsAsync(project);
        assignmentRepo.Setup(x => x.GetByProjectIdAsync(20)).ReturnsAsync([BuildAssignment(project.ProjectId, advisor.AdvisorId, advisor)]);
        availabilityRepo.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<int>>())).ReturnsAsync(slots);
        bookingRepo.Setup(x => x.GetByIdAsync(501)).ReturnsAsync(sourceBooking);
        bookingRepo.Setup(x => x.ExistsFreeRebookFromComplaintByOldBookingIdAsync(501)).ReturnsAsync(true);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateBookingAsync(new CreateBookingRequest
            {
                AdvisorId = 10,
                ProjectId = 20,
                OldBookingId = 501,
                AdvisorAvailabilitySlotIds = slots.Select(x => x.AdvisorAvailabilityId).ToList()
            }));

        Assert.Contains("already been used", ex.Message);
    }

    [Fact]
    public async Task UT119_CreateBookingAsync_ShouldThrow_WhenPremiumFreeBookingDurationExceeds3Hours()
    {
        var advisor = BuildAdvisor(10, 9000);
        var project = BuildProject(20, ProjectStatus.Approved);
        var slots = BuildConsecutiveSlots(advisor.AdvisorId, DateTime.UtcNow.Date.AddDays(2), 9, 4);

        var (service, _, _, advisorRepo, projectRepo, assignmentRepo, availabilityRepo, _, _, _, _, _, _, _, _) = CreateSut();
        advisorRepo.Setup(x => x.GetByIdAsync(10)).ReturnsAsync(advisor);
        projectRepo.Setup(x => x.GetByIdAsync(20)).ReturnsAsync(project);
        assignmentRepo.Setup(x => x.GetByProjectIdAsync(20)).ReturnsAsync([BuildAssignment(project.ProjectId, advisor.AdvisorId, advisor)]);
        availabilityRepo.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<int>>())).ReturnsAsync(slots);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateBookingAsync(new CreateBookingRequest
            {
                AdvisorId = 10,
                ProjectId = 20,
                IsFreeBooking = true,
                AdvisorAvailabilitySlotIds = slots.Select(x => x.AdvisorAvailabilityId).ToList()
            }));

        Assert.Contains("less than or equal to 3 hours", ex.Message);
    }

    [Fact]
    public async Task UT120_CreateBookingAsync_ShouldThrow_WhenPremiumFreeQuotaNotAvailable()
    {
        var advisor = BuildAdvisor(10, 9000);
        var project = BuildProject(20, ProjectStatus.Approved);
        var slots = BuildConsecutiveSlots(advisor.AdvisorId, DateTime.UtcNow.Date.AddDays(2), 9, 2);

        var (service, _, _, advisorRepo, projectRepo, assignmentRepo, availabilityRepo, _, subscriptionRepo, _, _, _, _, _, _) = CreateSut();
        advisorRepo.Setup(x => x.GetByIdAsync(10)).ReturnsAsync(advisor);
        projectRepo.Setup(x => x.GetByIdAsync(20)).ReturnsAsync(project);
        assignmentRepo.Setup(x => x.GetByProjectIdAsync(20)).ReturnsAsync([BuildAssignment(project.ProjectId, advisor.AdvisorId, advisor)]);
        availabilityRepo.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<int>>())).ReturnsAsync(slots);
        subscriptionRepo.Setup(x => x.GetLatestActiveAsync(5000)).ReturnsAsync((Subscription?)null);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateBookingAsync(new CreateBookingRequest
            {
                AdvisorId = 10,
                ProjectId = 20,
                IsFreeBooking = true,
                AdvisorAvailabilitySlotIds = slots.Select(x => x.AdvisorAvailabilityId).ToList()
            }));

        Assert.Contains("do not have any free premium booking quota left", ex.Message);
    }

    [Fact]
    public async Task UT121_CreateBookingAsync_ShouldCreateBookingAndReserveSlots_WhenValid()
    {
        var advisor = BuildAdvisor(10, 9000, hourlyRate: 120);
        var project = BuildProject(20, ProjectStatus.Approved);
        var slots = BuildConsecutiveSlots(advisor.AdvisorId, DateTime.UtcNow.Date.AddDays(2), 9, 2);

        Booking? addedBooking = null;

        var (service, unitOfWork, bookingRepo, advisorRepo, projectRepo, assignmentRepo, availabilityRepo, bookingSlotRepo, _, commissionRepo, _, _, _, _, _) = CreateSut();

        advisorRepo.Setup(x => x.GetByIdAsync(10)).ReturnsAsync(advisor);
        projectRepo.Setup(x => x.GetByIdAsync(20)).ReturnsAsync(project);
        assignmentRepo.Setup(x => x.GetByProjectIdAsync(20)).ReturnsAsync([BuildAssignment(project.ProjectId, advisor.AdvisorId, advisor)]);
        availabilityRepo.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<int>>())).ReturnsAsync(slots);
        commissionRepo.Setup(x => x.GetCurrentAsync(It.IsAny<DateTime>())).ReturnsAsync(new SystemCommissionConfig { SystemCommissionConfigId = 1, Percent = 10m });

        bookingRepo
            .Setup(x => x.AddAsync(It.IsAny<Booking>()))
            .Callback<Booking>(b =>
            {
                addedBooking = b;
                b.BookingId = 777;
            })
            .Returns(Task.CompletedTask);

        bookingRepo
            .Setup(x => x.GetByIdAsync(777))
            .ReturnsAsync(() => addedBooking);

        var result = await service.CreateBookingAsync(new CreateBookingRequest
        {
            AdvisorId = 10,
            ProjectId = 20,
            AdvisorAvailabilitySlotIds = slots.Select(x => x.AdvisorAvailabilityId).ToList(),
            IsFreeBooking = false,
            Note = "Need startup strategy"
        });

        Assert.NotNull(addedBooking);
        Assert.Equal(777, addedBooking!.BookingId);
        Assert.Equal(BookingStatus.Pending, addedBooking.Status);
        Assert.Equal(240m, addedBooking.Price);
        Assert.Equal(24m, addedBooking.SystemCommissionAmount);
        Assert.All(slots, x => Assert.Equal(AdvisorAvailabilityStatus.Booked, x.Status));

        Assert.NotNull(result);
        Assert.Equal(777, result!.Id);

        bookingSlotRepo.Verify(
            x => x.AddRangeAsync(It.Is<IEnumerable<BookingSlot>>(items => items.Count() == 2)),
            Times.Once);
        availabilityRepo.Verify(x => x.Update(It.IsAny<AdvisorAvailability>()), Times.Exactly(2));
        unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Exactly(2));
    }

    [Fact]
    public async Task UT122_CreateBookingAsync_ShouldNotifyAdvisor_WhenBookingCreated()
    {
        var advisor = BuildAdvisor(10, 9000, hourlyRate: 100);
        var project = BuildProject(20, ProjectStatus.Approved);
        var slots = BuildConsecutiveSlots(advisor.AdvisorId, DateTime.UtcNow.Date.AddDays(2), 9, 1);

        Booking? addedBooking = null;

        var (service, _, bookingRepo, advisorRepo, projectRepo, assignmentRepo, availabilityRepo, _, _, commissionRepo, _, _, notificationService, _, _) = CreateSut();
        advisorRepo.Setup(x => x.GetByIdAsync(10)).ReturnsAsync(advisor);
        projectRepo.Setup(x => x.GetByIdAsync(20)).ReturnsAsync(project);
        assignmentRepo.Setup(x => x.GetByProjectIdAsync(20)).ReturnsAsync([BuildAssignment(project.ProjectId, advisor.AdvisorId, advisor)]);
        availabilityRepo.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<int>>())).ReturnsAsync(slots);
        commissionRepo.Setup(x => x.GetCurrentAsync(It.IsAny<DateTime>())).ReturnsAsync(new SystemCommissionConfig { SystemCommissionConfigId = 2, Percent = 5m });

        bookingRepo
            .Setup(x => x.AddAsync(It.IsAny<Booking>()))
            .Callback<Booking>(b =>
            {
                addedBooking = b;
                b.BookingId = 778;
            })
            .Returns(Task.CompletedTask);
        bookingRepo.Setup(x => x.GetByIdAsync(778)).ReturnsAsync(() => addedBooking);

        await service.CreateBookingAsync(new CreateBookingRequest
        {
            AdvisorId = 10,
            ProjectId = 20,
            AdvisorAvailabilitySlotIds = [1]
        });

        notificationService.Verify(
            x => x.SendNotificationAsync(
                advisor.UserId,
                It.IsAny<string>(),
                It.IsAny<string>(),
                NotificationType.General,
                778,
                "Booking"),
            Times.Once);
    }

    [Fact]
    public async Task UT123_ApproveBookingAsync_ShouldThrow_WhenBookingNotFound()
    {
        var (service, _, bookingRepo, _, _, _, _, _, _, _, _, _, _, _, _) = CreateSut();
        bookingRepo.Setup(x => x.GetByIdForAdvisorActionAsync(999)).ReturnsAsync((Booking?)null);

        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => service.ApproveBookingAsync(999));

        Assert.Contains("Booking not found.", ex.Message);
    }

    [Fact]
    public async Task UT124_ApproveBookingAsync_ShouldThrow_WhenAdvisorResponseWindowExpired()
    {
        var booking = BuildAdvisorActionBooking(
            bookingId: 801,
            advisorId: 10,
            customerId: 5000,
            status: BookingStatus.Pending,
            createdAt: DateTime.UtcNow.AddMinutes(-30),
            price: 100,
            isPaymentWaived: false,
            includeProject: false);

        var (service, unitOfWork, bookingRepo, advisorRepo, _, _, availabilityRepo, _, _, _, userService, _, notificationService, _, _) = CreateSut();
        bookingRepo.Setup(x => x.GetByIdForAdvisorActionAsync(801)).ReturnsAsync(booking);
        advisorRepo.Setup(x => x.GetByUserIdAsync(7000)).ReturnsAsync(new Advisor { AdvisorId = 10, UserId = 7000, ApprovalStatus = ApprovalStatus.Approved });
        userService.Setup(x => x.GetUserId()).Returns(7000);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.ApproveBookingAsync(801));

        Assert.Contains("Booking response window has expired.", ex.Message);
        Assert.Equal(BookingStatus.NoResponse, booking.Status);
        Assert.All(booking.BookingSlots, slot => Assert.Equal(AdvisorAvailabilityStatus.Available, slot.AdvisorAvailability.Status));
        availabilityRepo.Verify(x => x.Update(It.IsAny<AdvisorAvailability>()), Times.Exactly(booking.BookingSlots.Count));
        unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
        notificationService.Verify(
            x => x.SendNotificationAsync(
                booking.CustomerId,
                It.IsAny<string>(),
                It.IsAny<string>(),
                NotificationType.General,
                null,
                null),
            Times.Once);
    }

    [Fact]
    public async Task UT125_ApproveBookingAsync_ShouldSetConfirmed_WhenPaymentWaivedOrPriceZero()
    {
        var booking = BuildAdvisorActionBooking(
            bookingId: 802,
            advisorId: 10,
            customerId: 5000,
            status: BookingStatus.Pending,
            createdAt: DateTime.UtcNow.AddMinutes(-1),
            price: 100,
            isPaymentWaived: true,
            includeProject: true);

        var (service, unitOfWork, bookingRepo, advisorRepo, _, _, _, _, _, _, userService, _, notificationService, _, _) = CreateSut();
        bookingRepo.Setup(x => x.GetByIdForAdvisorActionAsync(802)).ReturnsAsync(booking);
        advisorRepo.Setup(x => x.GetByUserIdAsync(7001)).ReturnsAsync(new Advisor { AdvisorId = 10, UserId = 7001, ApprovalStatus = ApprovalStatus.Approved });
        userService.Setup(x => x.GetUserId()).Returns(7001);

        var result = await service.ApproveBookingAsync(802);

        Assert.NotNull(result);
        Assert.Equal(BookingStatus.Confirmed, booking.Status);
        unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
        notificationService.Verify(
            x => x.SendNotificationAsync(
                booking.CustomerId,
                It.IsAny<string>(),
                It.IsAny<string>(),
                NotificationType.General,
                booking.BookingId,
                "Booking"),
            Times.Once);
    }

    [Fact]
    public async Task UT126_ApproveBookingAsync_ShouldSetApprovedAwaitingPayment_WhenPaymentRequired()
    {
        var booking = BuildAdvisorActionBooking(
            bookingId: 803,
            advisorId: 10,
            customerId: 5000,
            status: BookingStatus.Pending,
            createdAt: DateTime.UtcNow.AddMinutes(-1),
            price: 120,
            isPaymentWaived: false,
            includeProject: true);

        var (service, unitOfWork, bookingRepo, advisorRepo, _, _, _, _, _, _, userService, _, notificationService, _, _) = CreateSut();
        bookingRepo.Setup(x => x.GetByIdForAdvisorActionAsync(803)).ReturnsAsync(booking);
        advisorRepo.Setup(x => x.GetByUserIdAsync(7002)).ReturnsAsync(new Advisor { AdvisorId = 10, UserId = 7002, ApprovalStatus = ApprovalStatus.Approved });
        userService.Setup(x => x.GetUserId()).Returns(7002);

        var result = await service.ApproveBookingAsync(803);

        Assert.NotNull(result);
        Assert.Equal(BookingStatus.ApprovedAwaitingPayment, booking.Status);
        unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
        notificationService.Verify(
            x => x.SendNotificationAsync(
                booking.CustomerId,
                It.IsAny<string>(),
                It.IsAny<string>(),
                NotificationType.General,
                booking.BookingId,
                "Booking"),
            Times.Once);
    }

    [Fact]
    public async Task UT127_RejectBookingAsync_ShouldThrow_WhenStatusCannotBeRejected()
    {
        var booking = BuildAdvisorActionBooking(
            bookingId: 804,
            advisorId: 10,
            customerId: 5000,
            status: BookingStatus.Completed,
            createdAt: DateTime.UtcNow.AddMinutes(-1),
            price: 120,
            isPaymentWaived: false,
            includeProject: true);

        var (service, _, bookingRepo, advisorRepo, _, _, _, _, _, _, userService, _, _, _, _) = CreateSut();
        bookingRepo.Setup(x => x.GetByIdForAdvisorActionAsync(804)).ReturnsAsync(booking);
        advisorRepo.Setup(x => x.GetByUserIdAsync(7003)).ReturnsAsync(new Advisor { AdvisorId = 10, UserId = 7003, ApprovalStatus = ApprovalStatus.Approved });
        userService.Setup(x => x.GetUserId()).Returns(7003);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.RejectBookingAsync(804, "cannot proceed"));

        Assert.Contains("Only pending/approved-awaiting-payment bookings can be rejected.", ex.Message);
    }

    [Fact]
    public async Task UT128_RejectBookingAsync_ShouldReleaseSlotsAndNotifyCustomer_WhenRejected()
    {
        var booking = BuildAdvisorActionBooking(
            bookingId: 805,
            advisorId: 10,
            customerId: 5000,
            status: BookingStatus.Pending,
            createdAt: DateTime.UtcNow.AddMinutes(-1),
            price: 120,
            isPaymentWaived: false,
            includeProject: true);

        var (service, unitOfWork, bookingRepo, advisorRepo, _, _, availabilityRepo, _, _, _, userService, _, notificationService, _, _) = CreateSut();
        bookingRepo.Setup(x => x.GetByIdForAdvisorActionAsync(805)).ReturnsAsync(booking);
        advisorRepo.Setup(x => x.GetByUserIdAsync(7004)).ReturnsAsync(new Advisor { AdvisorId = 10, UserId = 7004, ApprovalStatus = ApprovalStatus.Approved });
        userService.Setup(x => x.GetUserId()).Returns(7004);

        var result = await service.RejectBookingAsync(805, "No fit now");

        Assert.NotNull(result);
        Assert.Equal(BookingStatus.Cancel, booking.Status);
        Assert.Contains("[Advisor Reject] No fit now", booking.Note);
        Assert.All(booking.BookingSlots, slot => Assert.Equal(AdvisorAvailabilityStatus.Available, slot.AdvisorAvailability.Status));

        availabilityRepo.Verify(x => x.Update(It.IsAny<AdvisorAvailability>()), Times.Exactly(booking.BookingSlots.Count));
        unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
        notificationService.Verify(
            x => x.SendNotificationAsync(
                booking.CustomerId,
                It.IsAny<string>(),
                It.IsAny<string>(),
                NotificationType.General,
                null,
                null),
            Times.Once);
    }

    private static (
        BookingService Service,
        Mock<IUnitOfWork> UnitOfWork,
        Mock<IBookingRepository> BookingRepository,
        Mock<IAdvisorsRepository> AdvisorRepository,
        Mock<IProjectRepository> ProjectRepository,
        Mock<IProjectAdvisorAssignmentRepository> AssignmentRepository,
        Mock<IAdvisorAvailabilityRepository> AvailabilityRepository,
        Mock<IBookingSlotRepository> BookingSlotRepository,
        Mock<ISubscriptionRepository> SubscriptionRepository,
        Mock<ISystemCommissionConfigRepository> CommissionRepository,
        Mock<IUserService> UserService,
        Mock<IMapper> Mapper,
        Mock<INotificationService> NotificationService,
        Mock<IPremiumFreeBookingUsageLogRepository> UsageLogRepository,
        Mock<ISieveProcessor> SieveProcessor) CreateSut()
    {
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var bookingRepositoryMock = new Mock<IBookingRepository>();
        var advisorRepositoryMock = new Mock<IAdvisorsRepository>();
        var projectRepositoryMock = new Mock<IProjectRepository>();
        var assignmentRepositoryMock = new Mock<IProjectAdvisorAssignmentRepository>();
        var availabilityRepositoryMock = new Mock<IAdvisorAvailabilityRepository>();
        var bookingSlotRepositoryMock = new Mock<IBookingSlotRepository>();
        var subscriptionRepositoryMock = new Mock<ISubscriptionRepository>();
        var commissionRepositoryMock = new Mock<ISystemCommissionConfigRepository>();
        var userServiceMock = new Mock<IUserService>();
        var mapperMock = new Mock<IMapper>();
        var notificationServiceMock = new Mock<INotificationService>();
        var usageLogRepositoryMock = new Mock<IPremiumFreeBookingUsageLogRepository>();
        var sieveProcessorMock = new Mock<ISieveProcessor>();

        var defaultAdvisor = BuildAdvisor(10, 9000, 100);
        var defaultProject = BuildProject(20, ProjectStatus.Approved);
        var defaultSlots = BuildConsecutiveSlots(defaultAdvisor.AdvisorId, DateTime.UtcNow.Date.AddDays(2), 9, 2);

        unitOfWorkMock.SetupGet(x => x.Bookings).Returns(bookingRepositoryMock.Object);
        unitOfWorkMock.SetupGet(x => x.Advisors).Returns(advisorRepositoryMock.Object);
        unitOfWorkMock.SetupGet(x => x.Projects).Returns(projectRepositoryMock.Object);
        unitOfWorkMock.SetupGet(x => x.ProjectAdvisorAssignments).Returns(assignmentRepositoryMock.Object);
        unitOfWorkMock.SetupGet(x => x.AdvisorAvailabilities).Returns(availabilityRepositoryMock.Object);
        unitOfWorkMock.SetupGet(x => x.BookingSlots).Returns(bookingSlotRepositoryMock.Object);
        unitOfWorkMock.SetupGet(x => x.Subscriptions).Returns(subscriptionRepositoryMock.Object);
        unitOfWorkMock.SetupGet(x => x.SystemCommissionConfigs).Returns(commissionRepositoryMock.Object);
        unitOfWorkMock.SetupGet(x => x.PremiumFreeBookingUsageLogs).Returns(usageLogRepositoryMock.Object);
        unitOfWorkMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        advisorRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(defaultAdvisor);
        advisorRepositoryMock.Setup(x => x.GetByUserIdAsync(It.IsAny<int>())).ReturnsAsync(new Advisor
        {
            AdvisorId = 10,
            UserId = 7000,
            ApprovalStatus = ApprovalStatus.Approved,
            User = new User { Id = 7000, UserName = "advisor-user", Role = UserRole.Advisor, Status = UserStatus.Active, CreatedAt = DateTime.UtcNow }
        });

        projectRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(defaultProject);
        assignmentRepositoryMock
            .Setup(x => x.GetByProjectIdAsync(It.IsAny<int>()))
            .ReturnsAsync([BuildAssignment(defaultProject.ProjectId, defaultAdvisor.AdvisorId, defaultAdvisor)]);

        availabilityRepositoryMock
            .Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync((IEnumerable<int> ids) => defaultSlots.Where(x => ids.Contains(x.AdvisorAvailabilityId)).ToList());

        bookingRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Booking?)null);
        bookingRepositoryMock.Setup(x => x.GetByIdForAdvisorActionAsync(It.IsAny<int>())).ReturnsAsync((Booking?)null);
        bookingRepositoryMock.Setup(x => x.ExistsFreeRebookFromComplaintByOldBookingIdAsync(It.IsAny<int>())).ReturnsAsync(false);
        bookingRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Booking>())).Returns(Task.CompletedTask);

        bookingSlotRepositoryMock.Setup(x => x.AddRangeAsync(It.IsAny<IEnumerable<BookingSlot>>())).Returns(Task.CompletedTask);
        commissionRepositoryMock
            .Setup(x => x.GetCurrentAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(new SystemCommissionConfig { SystemCommissionConfigId = 1, Percent = 10m });
        subscriptionRepositoryMock
            .Setup(x => x.GetLatestActiveAsync(It.IsAny<int>()))
            .ReturnsAsync(new Subscription
            {
                SubscriptionId = 1,
                UserId = 5000,
                RemainingFreeBookings = 1,
                Status = SubscriptionStatus.Active,
                StartDate = DateTime.UtcNow.AddDays(-10),
                EndDate = DateTime.UtcNow.AddDays(20)
            });

        userServiceMock.Setup(x => x.GetUserId()).Returns(5000);
        userServiceMock.Setup(x => x.GetUserRole()).Returns("Investor");

        mapperMock
            .Setup(x => x.Map<BookingResponse>(It.IsAny<Booking>()))
            .Returns<Booking>(b => new BookingResponse
            {
                Id = b.BookingId,
                AdvisorId = b.AdvisorId,
                ProjectId = b.ProjectId,
                CustomerId = b.CustomerId,
                Status = b.Status,
                Price = b.Price,
                Note = b.Note,
                IsPaymentWaived = b.IsPaymentWaived,
                IsFreeRebookFromComplaint = b.IsFreeRebookFromComplaint,
                UsedPremiumFreeQuota = b.UsedPremiumFreeQuota,
                AdvisorAvailabilitySlotIds = b.BookingSlots.Select(x => x.AdvisorAvailabilityId).ToList(),
                SlotCount = b.BookingSlots.Count
            });

        var service = new BookingService(
            unitOfWorkMock.Object,
            sieveProcessorMock.Object,
            userServiceMock.Object,
            mapperMock.Object,
            notificationServiceMock.Object);

        return (
            service,
            unitOfWorkMock,
            bookingRepositoryMock,
            advisorRepositoryMock,
            projectRepositoryMock,
            assignmentRepositoryMock,
            availabilityRepositoryMock,
            bookingSlotRepositoryMock,
            subscriptionRepositoryMock,
            commissionRepositoryMock,
            userServiceMock,
            mapperMock,
            notificationServiceMock,
            usageLogRepositoryMock,
            sieveProcessorMock);
    }

    private static CreateBookingRequest BuildCreateRequest(int advisorId, int projectId, List<int> slotIds)
    {
        return new CreateBookingRequest
        {
            AdvisorId = advisorId,
            ProjectId = projectId,
            AdvisorAvailabilitySlotIds = slotIds
        };
    }

    private static Advisor BuildAdvisor(int advisorId, int userId, decimal? hourlyRate = 100)
    {
        return new Advisor
        {
            AdvisorId = advisorId,
            UserId = userId,
            HourlyRate = hourlyRate,
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
    }

    private static Project BuildProject(int projectId, ProjectStatus status)
    {
        var startupUser = new User
        {
            Id = 9900,
            UserName = "startup-owner",
            Email = "startup@test.local",
            Role = UserRole.Startup,
            Status = UserStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        var startup = new Startup
        {
            StartupId = 300,
            UserId = startupUser.Id,
            User = startupUser,
            CompanyName = "AISEP Startup",
            Email = "company@test.local"
        };

        return new Project
        {
            ProjectId = projectId,
            StartupId = startup.StartupId,
            Startup = startup,
            ProjectName = "AISEP Booking Project",
            Status = status,
            Industry = Industry.Fintech
        };
    }

    private static ProjectAdvisorAssignment BuildAssignment(int projectId, int advisorId, Advisor advisor)
    {
        return new ProjectAdvisorAssignment
        {
            ProjectId = projectId,
            AdvisorId = advisorId,
            Advisor = advisor,
            AssignedAt = DateTime.UtcNow
        };
    }

    private static AdvisorAvailability BuildSlot(
        int slotId,
        int advisorId,
        DateTime date,
        int startHour,
        int endHour,
        AdvisorAvailabilityStatus status = AdvisorAvailabilityStatus.Available)
    {
        return new AdvisorAvailability
        {
            AdvisorAvailabilityId = slotId,
            AdvisorId = advisorId,
            SlotDate = date.Date,
            StartTime = new TimeOnly(startHour, 0),
            EndTime = new TimeOnly(endHour, 0),
            Status = status,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private static List<AdvisorAvailability> BuildConsecutiveSlots(int advisorId, DateTime date, int startHour, int count)
    {
        var slots = new List<AdvisorAvailability>();
        for (var i = 0; i < count; i++)
        {
            slots.Add(BuildSlot(i + 1, advisorId, date, startHour + i, startHour + i + 1));
        }

        return slots;
    }

    private static Booking BuildAdvisorActionBooking(
        int bookingId,
        int advisorId,
        int customerId,
        BookingStatus status,
        DateTime createdAt,
        decimal price,
        bool isPaymentWaived,
        bool includeProject)
    {
        var booking = new Booking
        {
            BookingId = bookingId,
            AdvisorId = advisorId,
            CustomerId = customerId,
            Status = status,
            CreatedAt = createdAt,
            Price = price,
            IsPaymentWaived = isPaymentWaived,
            BookingSlots =
            [
                new BookingSlot
                {
                    BookingId = bookingId,
                    AdvisorAvailabilityId = 10001,
                    AdvisorAvailability = new AdvisorAvailability
                    {
                        AdvisorAvailabilityId = 10001,
                        AdvisorId = advisorId,
                        SlotDate = DateTime.UtcNow.Date.AddDays(2),
                        StartTime = new TimeOnly(9, 0),
                        EndTime = new TimeOnly(10, 0),
                        Status = AdvisorAvailabilityStatus.Booked,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    }
                }
            ]
        };

        if (includeProject)
        {
            booking.Project = BuildProject(20, ProjectStatus.Approved);
            booking.ProjectId = booking.Project.ProjectId;
        }

        return booking;
    }
}
