using AISEP.BLL.Services.Chats;
using AISEP.BLL.Services.Users;
using AISEP.DAL.Common;
using AISEP.DAL.Entities;
using AISEP.DAL.Enums;
using AISEP.DAL.Repositories.Bookings;
using AISEP.DAL.Repositories.Chats;
using AISEP.DAL.Repositories.ConnectionRequests;
using Moq;
using Xunit;

namespace AISEP.BLL.Tests.ChatSessions;

public class ChatSessionServiceGroupedTests
{
    [Fact]
    public async Task UT196_OpenSessionAsync_ShouldReturnNull_WhenBookingNotFound()
    {
        var (service, _, bookings, chatSessions, _, _) = CreateSut();
        bookings.Setup(x => x.GetByIdAsync(1960)).ReturnsAsync((Booking?)null);

        var result = await service.OpenSessionAsync(1960);

        Assert.Null(result);
        chatSessions.Verify(x => x.GetByBookingIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task UT197_OpenSessionAsync_ShouldReturnNull_WhenUserNotBookingParticipant()
    {
        const int currentUserId = 7197;
        var booking = BuildBooking(bookingId: 1970, customerId: 1197, advisorUserId: 2197, BookingStatus.Confirmed);

        var (service, _, bookings, chatSessions, _, _) = CreateSut(currentUserId);
        bookings.Setup(x => x.GetByIdAsync(booking.BookingId)).ReturnsAsync(booking);

        var result = await service.OpenSessionAsync(booking.BookingId);

        Assert.Null(result);
        chatSessions.Verify(x => x.GetByBookingIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task UT198_OpenSessionAsync_ShouldReturnNull_WhenBookingStatusIsNotConfirmed()
    {
        const int currentUserId = 7198;
        var booking = BuildBooking(bookingId: 1980, customerId: currentUserId, advisorUserId: 2198, BookingStatus.Pending);

        var (service, _, bookings, chatSessions, _, _) = CreateSut(currentUserId);
        bookings.Setup(x => x.GetByIdAsync(booking.BookingId)).ReturnsAsync(booking);

        var result = await service.OpenSessionAsync(booking.BookingId);

        Assert.Null(result);
        chatSessions.Verify(x => x.GetByBookingIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task UT199_OpenSessionAsync_ShouldReturnExistingSession_WhenAlreadyExists()
    {
        const int currentUserId = 7199;
        var booking = BuildBooking(bookingId: 1990, customerId: currentUserId, advisorUserId: 2199, BookingStatus.Confirmed);
        var existingSession = BuildBookingSession(sessionId: 1999, booking: booking, isOpen: true);

        var (service, unitOfWork, bookings, chatSessions, _, _) = CreateSut(currentUserId);
        bookings.Setup(x => x.GetByIdAsync(booking.BookingId)).ReturnsAsync(booking);
        chatSessions.Setup(x => x.GetByBookingIdAsync(booking.BookingId)).ReturnsAsync(existingSession);

        var result = await service.OpenSessionAsync(booking.BookingId);

        Assert.NotNull(result);
        Assert.Equal(existingSession.ChatSessionId, result!.ChatSessionId);
        Assert.Equal("Booking", result.SessionType);

        chatSessions.Verify(x => x.AddAsync(It.IsAny<ChatSession>()), Times.Never);
        unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UT200_OpenSessionAsync_ShouldCreateSession_WhenNotExistsAndConfirmed()
    {
        const int currentUserId = 7200;
        var booking = BuildBooking(bookingId: 2000, customerId: currentUserId, advisorUserId: 2200, BookingStatus.Confirmed);
        var createdSession = BuildBookingSession(sessionId: 2205, booking: booking, isOpen: true);

        var (service, unitOfWork, bookings, chatSessions, _, _) = CreateSut(currentUserId);
        bookings.Setup(x => x.GetByIdAsync(booking.BookingId)).ReturnsAsync(booking);
        chatSessions.Setup(x => x.GetByBookingIdAsync(booking.BookingId)).ReturnsAsync((ChatSession?)null);
        chatSessions
            .Setup(x => x.AddAsync(It.IsAny<ChatSession>()))
            .Callback<ChatSession>(session => session.ChatSessionId = createdSession.ChatSessionId)
            .Returns(Task.CompletedTask);
        chatSessions.Setup(x => x.GetByIdAsync(createdSession.ChatSessionId)).ReturnsAsync(createdSession);

        var result = await service.OpenSessionAsync(booking.BookingId);

        Assert.NotNull(result);
        Assert.Equal(createdSession.ChatSessionId, result!.ChatSessionId);
        Assert.Equal(booking.BookingId, result.BookingId);
        Assert.True(result.IsOpen);

        chatSessions.Verify(x => x.AddAsync(It.IsAny<ChatSession>()), Times.Once);
        unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UT201_OpenSessionByConnectionRequestAsync_ShouldReturnNull_WhenRequestInvalidOrNotAccepted()
    {
        var pendingRequest = BuildConnectionRequest(
            requestId: 2012,
            investorUserId: 3201,
            startupUserId: 4201,
            status: ConnectionRequestStatus.Pending);

        var (service, _, _, chatSessions, connectionRequests, _) = CreateSut();
        connectionRequests.Setup(x => x.GetByIdAsync(2011)).ReturnsAsync((ConnectionRequest?)null);
        connectionRequests.Setup(x => x.GetByIdAsync(pendingRequest.ConnectionRequestId)).ReturnsAsync(pendingRequest);

        var missingResult = await service.OpenSessionByConnectionRequestAsync(2011, userId: 3201);
        var nonAcceptedResult = await service.OpenSessionByConnectionRequestAsync(pendingRequest.ConnectionRequestId, userId: 3201);

        Assert.Null(missingResult);
        Assert.Null(nonAcceptedResult);
        chatSessions.Verify(x => x.GetByConnectionRequestIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task UT202_OpenSessionByConnectionRequestAsync_ShouldReturnExistingSession_WhenAlreadyExists()
    {
        const int investorUserId = 3202;
        var request = BuildConnectionRequest(
            requestId: 2020,
            investorUserId: investorUserId,
            startupUserId: 4202,
            status: ConnectionRequestStatus.Accepted);
        var existingSession = BuildConnectionSession(sessionId: 2022, request: request, isOpen: true);

        var (service, unitOfWork, _, chatSessions, connectionRequests, _) = CreateSut();
        connectionRequests.Setup(x => x.GetByIdAsync(request.ConnectionRequestId)).ReturnsAsync(request);
        chatSessions.Setup(x => x.GetByConnectionRequestIdAsync(request.ConnectionRequestId)).ReturnsAsync(existingSession);

        var result = await service.OpenSessionByConnectionRequestAsync(request.ConnectionRequestId, investorUserId);

        Assert.NotNull(result);
        Assert.Equal(existingSession.ChatSessionId, result!.ChatSessionId);
        Assert.Equal("ConnectionRequest", result.SessionType);

        chatSessions.Verify(x => x.AddAsync(It.IsAny<ChatSession>()), Times.Never);
        unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UT203_GetSessionAsync_ShouldReturnNull_WhenUserNotParticipant()
    {
        const int currentUserId = 7203;
        var foreignBooking = BuildBooking(bookingId: 2030, customerId: 1203, advisorUserId: 2203, BookingStatus.Confirmed);
        var session = BuildBookingSession(sessionId: 2033, booking: foreignBooking, isOpen: true);

        var (service, _, _, chatSessions, _, _) = CreateSut(currentUserId);
        chatSessions.Setup(x => x.GetByIdAsync(session.ChatSessionId)).ReturnsAsync(session);

        var result = await service.GetSessionAsync(session.ChatSessionId);

        Assert.Null(result);
    }

    [Fact]
    public async Task UT204_CloseSessionAsync_ShouldReturnFalse_WhenSessionMissingClosedOrUnauthorized()
    {
        const int currentUserId = 7204;
        var closedBooking = BuildBooking(bookingId: 2040, customerId: currentUserId, advisorUserId: 2204, BookingStatus.Confirmed);
        var closedSession = BuildBookingSession(sessionId: 2044, booking: closedBooking, isOpen: false);

        var (service, unitOfWork, _, chatSessions, _, _) = CreateSut(currentUserId);
        chatSessions.Setup(x => x.GetByIdAsync(closedSession.ChatSessionId)).ReturnsAsync(closedSession);

        var result = await service.CloseSessionAsync(closedSession.ChatSessionId);

        Assert.False(result);
        chatSessions.Verify(x => x.Update(It.IsAny<ChatSession>()), Times.Never);
        unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UT205_CloseSessionAsync_ShouldCloseSessionAndSetEndTime_WhenAuthorized()
    {
        const int currentUserId = 7205;
        var booking = BuildBooking(bookingId: 2050, customerId: currentUserId, advisorUserId: 2205, BookingStatus.Confirmed);
        var session = BuildBookingSession(sessionId: 2055, booking: booking, isOpen: true);

        var (service, unitOfWork, _, chatSessions, _, _) = CreateSut(currentUserId);
        chatSessions.Setup(x => x.GetByIdAsync(session.ChatSessionId)).ReturnsAsync(session);

        var result = await service.CloseSessionAsync(session.ChatSessionId);

        Assert.True(result);
        Assert.False(session.IsOpen);
        Assert.NotNull(session.EndTime);

        chatSessions.Verify(x => x.Update(session), Times.Once);
        unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    private static (
        ChatSessionService Service,
        Mock<IUnitOfWork> UnitOfWork,
        Mock<IBookingRepository> Bookings,
        Mock<IChatSessionRepository> ChatSessions,
        Mock<IConnectionRequestRepository> ConnectionRequests,
        Mock<IUserService> UserService) CreateSut(int userId = 7001)
    {
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var bookingRepositoryMock = new Mock<IBookingRepository>();
        var chatSessionRepositoryMock = new Mock<IChatSessionRepository>();
        var connectionRequestRepositoryMock = new Mock<IConnectionRequestRepository>();
        var userServiceMock = new Mock<IUserService>();

        var defaultBooking = BuildBooking(bookingId: 9001, customerId: userId, advisorUserId: 8001, BookingStatus.Confirmed);
        var defaultConnectionRequest = BuildConnectionRequest(
            requestId: 9101,
            investorUserId: userId,
            startupUserId: 8101,
            status: ConnectionRequestStatus.Accepted);
        var defaultSession = BuildBookingSession(sessionId: 9201, booking: defaultBooking, isOpen: true);

        unitOfWorkMock.SetupGet(x => x.Bookings).Returns(bookingRepositoryMock.Object);
        unitOfWorkMock.SetupGet(x => x.ChatSessions).Returns(chatSessionRepositoryMock.Object);
        unitOfWorkMock.SetupGet(x => x.ConnectionRequests).Returns(connectionRequestRepositoryMock.Object);
        unitOfWorkMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        bookingRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(defaultBooking);

        connectionRequestRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(defaultConnectionRequest);

        chatSessionRepositoryMock
            .Setup(x => x.GetByBookingIdAsync(It.IsAny<int>()))
            .ReturnsAsync((ChatSession?)null);
        chatSessionRepositoryMock
            .Setup(x => x.GetByConnectionRequestIdAsync(It.IsAny<int>()))
            .ReturnsAsync((ChatSession?)null);
        chatSessionRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(defaultSession);
        chatSessionRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<ChatSession>()))
            .Returns(Task.CompletedTask);

        userServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        var service = new ChatSessionService(unitOfWorkMock.Object, userServiceMock.Object);

        return (
            service,
            unitOfWorkMock,
            bookingRepositoryMock,
            chatSessionRepositoryMock,
            connectionRequestRepositoryMock,
            userServiceMock);
    }

    private static Booking BuildBooking(int bookingId, int customerId, int advisorUserId, BookingStatus status)
    {
        return new Booking
        {
            BookingId = bookingId,
            AdvisorId = bookingId + 100,
            CustomerId = customerId,
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(2),
            Price = 100,
            Status = status,
            Advisor = new Advisor
            {
                AdvisorId = bookingId + 100,
                UserId = advisorUserId,
                User = BuildUser(advisorUserId, UserRole.Advisor)
            },
            Customer = BuildUser(customerId, UserRole.Investor)
        };
    }

    private static ConnectionRequest BuildConnectionRequest(
        int requestId,
        int investorUserId,
        int startupUserId,
        ConnectionRequestStatus status)
    {
        var startup = new Startup
        {
            StartupId = requestId + 10,
            UserId = startupUserId,
            CompanyName = $"Startup-{requestId}",
            User = BuildUser(startupUserId, UserRole.Startup)
        };

        var project = new Project
        {
            ProjectId = requestId + 100,
            StartupId = startup.StartupId,
            ProjectName = $"Project-{requestId}",
            Startup = startup,
            IndustryOptionId = 1,
            IndustryOption = new IndustryOption { Id = 1, Value = "SaaS", IsActive = true },
            Status = ProjectStatus.Approved,
            CreatedAt = DateTime.UtcNow.AddDays(-2)
        };

        var investor = new Investor
        {
            InvestorId = requestId + 20,
            UserId = investorUserId,
            OrganizationName = $"Investor-{requestId}",
            User = BuildUser(investorUserId, UserRole.Investor)
        };

        return new ConnectionRequest
        {
            ConnectionRequestId = requestId,
            InvestorId = investor.InvestorId,
            ProjectId = project.ProjectId,
            Status = status,
            Investor = investor,
            Project = project
        };
    }

    private static ChatSession BuildBookingSession(int sessionId, Booking booking, bool isOpen)
    {
        return new ChatSession
        {
            ChatSessionId = sessionId,
            BookingId = booking.BookingId,
            Booking = booking,
            IsOpen = isOpen,
            StartTime = DateTime.UtcNow.AddMinutes(-30),
            EndTime = isOpen ? null : DateTime.UtcNow.AddMinutes(-5),
            ChatMessages = []
        };
    }

    private static ChatSession BuildConnectionSession(int sessionId, ConnectionRequest request, bool isOpen)
    {
        return new ChatSession
        {
            ChatSessionId = sessionId,
            ConnectionRequestId = request.ConnectionRequestId,
            ConnectionRequest = request,
            IsOpen = isOpen,
            StartTime = DateTime.UtcNow.AddMinutes(-20),
            EndTime = isOpen ? null : DateTime.UtcNow.AddMinutes(-2),
            ChatMessages = []
        };
    }

    private static User BuildUser(int userId, UserRole role)
    {
        return new User
        {
            Id = userId,
            UserName = $"user-{userId}",
            Email = $"user{userId}@test.local",
            Role = role,
            Status = UserStatus.Active,
            CreatedAt = DateTime.UtcNow
        };
    }
}
