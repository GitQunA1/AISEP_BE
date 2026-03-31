using AISEP.BLL.DTOs.Responses;
using AISEP.DAL.Common;
using AISEP.DAL.Entities;
using AISEP.DAL.Enums;

namespace AISEP.BLL.Services.Chats
{
    public class ChatSessionService : IChatSessionService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ChatSessionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ChatSessionResponse?> OpenSessionAsync(int bookingId, int userId)
        {
            var booking = await _unitOfWork.Bookings.GetByIdAsync(bookingId);
            if (booking is null || !IsParticipant(booking, userId)) return null;
            if (booking.Status != BookingStatus.Confirmed)
                return null;

            var existing = await _unitOfWork.ChatSessions.GetByBookingIdAsync(bookingId);
            if (existing is not null)
                return MapSession(existing);

            var session = new ChatSession
            {
                BookingId = bookingId,
                IsOpen    = true,
                StartTime = DateTime.UtcNow
            };

            await _unitOfWork.ChatSessions.AddAsync(session);
            await _unitOfWork.SaveChangesAsync();

            var created = await _unitOfWork.ChatSessions.GetByIdAsync(session.ChatSessionId);
            return MapSession(created!);
        }

        public async Task<ChatSessionResponse?> GetSessionAsync(int sessionId, int userId)
        {
            var session = await _unitOfWork.ChatSessions.GetByIdAsync(sessionId);
            if (session is null || !IsParticipant(session.Booking, userId)) return null;
            return MapSession(session);
        }

        public async Task<IEnumerable<ChatSessionResponse>> GetMySessionsAsync(int userId)
        {
            var sessions = await _unitOfWork.ChatSessions.GetByUserIdAsync(userId);
            return sessions.Select(MapSession);
        }

        public async Task<bool> CloseSessionAsync(int sessionId, int userId)
        {
            var session = await _unitOfWork.ChatSessions.GetByIdAsync(sessionId);
            if (session is null || !session.IsOpen || !IsParticipant(session.Booking, userId))
                return false;

            session.IsOpen  = false;
            session.EndTime = DateTime.UtcNow;
            _unitOfWork.ChatSessions.Update(session);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        // ── Helpers ──────────────────────────────────────────────────────

        private static bool IsParticipant(Booking booking, int userId)
            => booking.CustomerId == userId || booking.Advisor.UserId == userId;

        private static ChatSessionResponse MapSession(ChatSession s) => new()
        {
            ChatSessionId = s.ChatSessionId,
            BookingId     = s.BookingId,
            IsOpen        = s.IsOpen,
            StartTime     = s.StartTime,
            EndTime       = s.EndTime,
            AdvisorName   = s.Booking?.Advisor?.User?.UserName ?? string.Empty,
            CustomerName  = s.Booking?.Customer?.UserName      ?? string.Empty,
            Messages      = s.ChatMessages?.Select(m => new ChatMessageResponse
            {
                ChatMessageId = m.ChatMessageId,
                ChatSessionId = m.ChatSessionId,
                SenderId      = m.SenderId,
                SenderName    = m.Sender?.UserName ?? string.Empty,
                Content       = m.Content,
                SentAt        = m.SentAt
            }) ?? []
        };
    }
}
