using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;
using AISEP.DAL.Common;
using AISEP.DAL.Entities;

namespace AISEP.BLL.Services.Chats
{
    public class ChatMessageService : IChatMessageService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ChatMessageService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<ChatMessageResponse>> GetMessagesAsync(int sessionId, int userId)
        {
            var session = await _unitOfWork.ChatSessions.GetByIdAsync(sessionId);
            if (session is null || !IsParticipant(session, userId)) return [];

            var messages = await _unitOfWork.ChatMessages.GetBySessionIdAsync(sessionId);
            return messages.Select(MapMessage);
        }

        public async Task<ChatMessageResponse?> SendMessageAsync(int sessionId, int userId, SendMessageRequest request)
        {
            var session = await _unitOfWork.ChatSessions.GetByIdAsync(sessionId);
            if (session is null || !session.IsOpen || !IsParticipant(session, userId))
                return null;

            var message = new ChatMessage
            {
                ChatSessionId = sessionId,
                SenderId      = userId,
                Content       = request.Content,
                SentAt        = DateTime.UtcNow
            };

            await _unitOfWork.ChatMessages.AddAsync(message);
            await _unitOfWork.SaveChangesAsync();

            var created = await _unitOfWork.ChatMessages.GetByIdAsync(message.ChatMessageId);
            return created is null ? null : MapMessage(created);
        }

        private static bool IsBookingParticipant(Booking booking, int userId)
            => booking.CustomerId == userId || booking.Advisor.UserId == userId;

        private static bool IsConnectionParticipant(ConnectionRequest connectionRequest, int userId)
            => connectionRequest.Investor.UserId == userId
               || connectionRequest.Project.Startup.UserId == userId;

        private static bool IsParticipant(ChatSession session, int userId)
        {
            if (session.BookingId.HasValue && session.Booking is not null)
            {
                return IsBookingParticipant(session.Booking, userId);
            }

            if (session.ConnectionRequestId.HasValue && session.ConnectionRequest is not null)
            {
                return IsConnectionParticipant(session.ConnectionRequest, userId);
            }

            return false;
        }

        private static ChatMessageResponse MapMessage(ChatMessage m) => new()
        {
            ChatMessageId = m.ChatMessageId,
            ChatSessionId = m.ChatSessionId,
            SenderId      = m.SenderId,
            SenderName    = m.Sender?.UserName ?? string.Empty,
            Content       = m.Content,
            SentAt        = m.SentAt
        };
    }
}
