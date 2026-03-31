using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;
using AISEP.DAL.Common;
using AISEP.DAL.Entities;
using AISEP.DAL.Enums;

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
            if (session is null || !IsParticipant(session.Booking, userId)) return [];

            var messages = await _unitOfWork.ChatMessages.GetBySessionIdAsync(sessionId);
            return messages.Select(MapMessage);
        }

        public async Task<ChatMessageResponse?> SendMessageAsync(int sessionId, int userId, SendMessageRequest request)
        {
            var session = await _unitOfWork.ChatSessions.GetByIdAsync(sessionId);
            if (session is null || !session.IsOpen || !IsParticipant(session.Booking, userId))
                return null;
            if (session.Booking.Status == BookingStatus.Completed)
            {
                session.IsOpen = false;
                session.EndTime = DateTime.UtcNow;
                _unitOfWork.ChatSessions.Update(session);
                await _unitOfWork.SaveChangesAsync();
                return null;
            }

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

        

        private static bool IsParticipant(Booking booking, int userId)
            => booking.CustomerId == userId || booking.Advisor.UserId == userId;

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
