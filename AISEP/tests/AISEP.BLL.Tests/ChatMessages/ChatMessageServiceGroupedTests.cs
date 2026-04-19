using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.Services.Chats;
using AISEP.DAL.Common;
using AISEP.DAL.Entities;
using AISEP.DAL.Enums;
using AISEP.DAL.Repositories.Chats;
using Moq;
using Xunit;

namespace AISEP.BLL.Tests.ChatMessages;

public class ChatMessageServiceGroupedTests
{
    [Fact]
    public async Task UT206_GetMessagesAsync_ShouldReturnEmpty_WhenSessionNotFound()
    {
        var (service, _, chatSessions, chatMessages) = CreateSut();
        chatSessions.Setup(x => x.GetByIdAsync(2060)).ReturnsAsync((ChatSession?)null);

        var result = await service.GetMessagesAsync(2060, userId: 7206);

        Assert.Empty(result);
        chatMessages.Verify(x => x.GetBySessionIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task UT207_GetMessagesAsync_ShouldReturnEmpty_WhenUserNotParticipant()
    {
        var session = BuildBookingSession(
            sessionId: 2070,
            customerId: 1207,
            advisorUserId: 2207,
            bookingStatus: BookingStatus.Confirmed,
            isOpen: true);

        var (service, _, chatSessions, chatMessages) = CreateSut();
        chatSessions.Setup(x => x.GetByIdAsync(session.ChatSessionId)).ReturnsAsync(session);

        var result = await service.GetMessagesAsync(session.ChatSessionId, userId: 7207);

        Assert.Empty(result);
        chatMessages.Verify(x => x.GetBySessionIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task UT208_GetMessagesAsync_ShouldReturnMappedMessages_WhenUserIsParticipant()
    {
        const int currentUserId = 7208;
        var session = BuildBookingSession(
            sessionId: 2080,
            customerId: currentUserId,
            advisorUserId: 2208,
            bookingStatus: BookingStatus.Confirmed,
            isOpen: true);

        var messages = new List<ChatMessage>
        {
            new()
            {
                ChatMessageId = 2081,
                ChatSessionId = session.ChatSessionId,
                SenderId = currentUserId,
                Sender = BuildUser(currentUserId, UserRole.Investor),
                Content = "Hello advisor",
                SentAt = DateTime.UtcNow.AddMinutes(-5)
            },
            new()
            {
                ChatMessageId = 2082,
                ChatSessionId = session.ChatSessionId,
                SenderId = 2208,
                Sender = BuildUser(2208, UserRole.Advisor),
                Content = "Hello investor",
                SentAt = DateTime.UtcNow.AddMinutes(-3)
            }
        };

        var (service, _, chatSessions, chatMessages) = CreateSut();
        chatSessions.Setup(x => x.GetByIdAsync(session.ChatSessionId)).ReturnsAsync(session);
        chatMessages.Setup(x => x.GetBySessionIdAsync(session.ChatSessionId)).ReturnsAsync(messages);

        var result = (await service.GetMessagesAsync(session.ChatSessionId, currentUserId)).ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal(2081, result[0].ChatMessageId);
        Assert.Equal("Hello advisor", result[0].Content);
        Assert.Equal($"user-{currentUserId}", result[0].SenderName);
        Assert.Equal(2082, result[1].ChatMessageId);
        Assert.Equal("Hello investor", result[1].Content);
        Assert.Equal("user-2208", result[1].SenderName);
    }

    [Fact]
    public async Task UT209_SendMessageAsync_ShouldReturnNull_WhenSessionNotFound()
    {
        var request = new SendMessageRequest { Content = "test message" };
        var (service, unitOfWork, chatSessions, chatMessages) = CreateSut();
        chatSessions.Setup(x => x.GetByIdAsync(2090)).ReturnsAsync((ChatSession?)null);

        var result = await service.SendMessageAsync(2090, userId: 7209, request);

        Assert.Null(result);
        chatMessages.Verify(x => x.AddAsync(It.IsAny<ChatMessage>()), Times.Never);
        unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UT210_SendMessageAsync_ShouldReturnNull_WhenSessionClosed()
    {
        const int currentUserId = 7210;
        var request = new SendMessageRequest { Content = "test message" };
        var closedSession = BuildBookingSession(
            sessionId: 2100,
            customerId: currentUserId,
            advisorUserId: 2210,
            bookingStatus: BookingStatus.Confirmed,
            isOpen: false);

        var (service, unitOfWork, chatSessions, chatMessages) = CreateSut();
        chatSessions.Setup(x => x.GetByIdAsync(closedSession.ChatSessionId)).ReturnsAsync(closedSession);

        var result = await service.SendMessageAsync(closedSession.ChatSessionId, currentUserId, request);

        Assert.Null(result);
        chatMessages.Verify(x => x.AddAsync(It.IsAny<ChatMessage>()), Times.Never);
        unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UT211_SendMessageAsync_ShouldReturnNull_WhenUserNotParticipant()
    {
        var request = new SendMessageRequest { Content = "test message" };
        var session = BuildBookingSession(
            sessionId: 2110,
            customerId: 1211,
            advisorUserId: 2211,
            bookingStatus: BookingStatus.Confirmed,
            isOpen: true);

        var (service, unitOfWork, chatSessions, chatMessages) = CreateSut();
        chatSessions.Setup(x => x.GetByIdAsync(session.ChatSessionId)).ReturnsAsync(session);

        var result = await service.SendMessageAsync(session.ChatSessionId, userId: 7211, request);

        Assert.Null(result);
        chatMessages.Verify(x => x.AddAsync(It.IsAny<ChatMessage>()), Times.Never);
        unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UT212_SendMessageAsync_ShouldAutoCloseSessionAndReturnNull_WhenBookingCompleted()
    {
        const int currentUserId = 7212;
        var request = new SendMessageRequest { Content = "test message" };
        var completedSession = BuildBookingSession(
            sessionId: 2120,
            customerId: currentUserId,
            advisorUserId: 2212,
            bookingStatus: BookingStatus.Completed,
            isOpen: true);

        var (service, unitOfWork, chatSessions, chatMessages) = CreateSut();
        chatSessions.Setup(x => x.GetByIdAsync(completedSession.ChatSessionId)).ReturnsAsync(completedSession);

        var result = await service.SendMessageAsync(completedSession.ChatSessionId, currentUserId, request);

        Assert.Null(result);
        Assert.False(completedSession.IsOpen);
        Assert.NotNull(completedSession.EndTime);

        chatSessions.Verify(x => x.Update(completedSession), Times.Once);
        chatMessages.Verify(x => x.AddAsync(It.IsAny<ChatMessage>()), Times.Never);
        unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UT213_SendMessageAsync_ShouldPersistAndReturnMessage_WhenValid()
    {
        const int currentUserId = 7213;
        var request = new SendMessageRequest { Content = "persist message" };
        var session = BuildBookingSession(
            sessionId: 2130,
            customerId: currentUserId,
            advisorUserId: 2213,
            bookingStatus: BookingStatus.Confirmed,
            isOpen: true);

        var persisted = new ChatMessage
        {
            ChatMessageId = 2139,
            ChatSessionId = session.ChatSessionId,
            SenderId = currentUserId,
            Sender = BuildUser(currentUserId, UserRole.Investor),
            Content = request.Content,
            SentAt = DateTime.UtcNow
        };

        ChatMessage? addedMessage = null;
        var (service, unitOfWork, chatSessions, chatMessages) = CreateSut();
        chatSessions.Setup(x => x.GetByIdAsync(session.ChatSessionId)).ReturnsAsync(session);
        chatMessages
            .Setup(x => x.AddAsync(It.IsAny<ChatMessage>()))
            .Callback<ChatMessage>(m =>
            {
                addedMessage = m;
                m.ChatMessageId = persisted.ChatMessageId;
            })
            .Returns(Task.CompletedTask);
        chatMessages.Setup(x => x.GetByIdAsync(persisted.ChatMessageId)).ReturnsAsync(persisted);

        var result = await service.SendMessageAsync(session.ChatSessionId, currentUserId, request);

        Assert.NotNull(addedMessage);
        Assert.Equal(request.Content, addedMessage!.Content);
        Assert.NotNull(result);
        Assert.Equal(persisted.ChatMessageId, result!.ChatMessageId);
        Assert.Equal(session.ChatSessionId, result.ChatSessionId);
        Assert.Equal(currentUserId, result.SenderId);
        Assert.Equal($"user-{currentUserId}", result.SenderName);

        chatMessages.Verify(x => x.AddAsync(It.IsAny<ChatMessage>()), Times.Once);
        chatSessions.Verify(x => x.Update(It.IsAny<ChatSession>()), Times.Never);
        unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    private static (
        ChatMessageService Service,
        Mock<IUnitOfWork> UnitOfWork,
        Mock<IChatSessionRepository> ChatSessions,
        Mock<IChatMessageRepository> ChatMessages) CreateSut()
    {
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var chatSessionRepositoryMock = new Mock<IChatSessionRepository>();
        var chatMessageRepositoryMock = new Mock<IChatMessageRepository>();

        var defaultSession = BuildBookingSession(
            sessionId: 9901,
            customerId: 7001,
            advisorUserId: 8001,
            bookingStatus: BookingStatus.Confirmed,
            isOpen: true);

        unitOfWorkMock.SetupGet(x => x.ChatSessions).Returns(chatSessionRepositoryMock.Object);
        unitOfWorkMock.SetupGet(x => x.ChatMessages).Returns(chatMessageRepositoryMock.Object);
        unitOfWorkMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        chatSessionRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(defaultSession);

        chatMessageRepositoryMock
            .Setup(x => x.GetBySessionIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<ChatMessage>());
        chatMessageRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<ChatMessage>()))
            .Returns(Task.CompletedTask);
        chatMessageRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((ChatMessage?)null);

        var service = new ChatMessageService(unitOfWorkMock.Object);

        return (service, unitOfWorkMock, chatSessionRepositoryMock, chatMessageRepositoryMock);
    }

    private static ChatSession BuildBookingSession(
        int sessionId,
        int customerId,
        int advisorUserId,
        BookingStatus bookingStatus,
        bool isOpen)
    {
        var bookingId = sessionId + 100;
        return new ChatSession
        {
            ChatSessionId = sessionId,
            BookingId = bookingId,
            Booking = new Booking
            {
                BookingId = bookingId,
                AdvisorId = bookingId + 100,
                CustomerId = customerId,
                StartTime = DateTime.UtcNow.AddHours(-2),
                EndTime = DateTime.UtcNow.AddHours(-1),
                Price = 100,
                Status = bookingStatus,
                Advisor = new Advisor
                {
                    AdvisorId = bookingId + 100,
                    UserId = advisorUserId,
                    User = BuildUser(advisorUserId, UserRole.Advisor)
                },
                Customer = BuildUser(customerId, UserRole.Investor)
            },
            IsOpen = isOpen,
            StartTime = DateTime.UtcNow.AddMinutes(-30),
            EndTime = isOpen ? null : DateTime.UtcNow.AddMinutes(-5),
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
