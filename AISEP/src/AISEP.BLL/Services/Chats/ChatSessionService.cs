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
            if (booking is null || !IsBookingParticipant(booking, userId)) return null;

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
            return created is null ? null : MapSession(created);
        }

        public async Task<ChatSessionResponse?> OpenSessionByConnectionRequestAsync(int connectionRequestId, int userId)
        {
            var connectionRequest = await _unitOfWork.ConnectionRequests.GetByIdAsync(connectionRequestId);
            if (connectionRequest is null
                || connectionRequest.Status != ConnectionRequestStatus.Accepted
                || !IsConnectionParticipant(connectionRequest, userId))
            {
                return null;
            }

            var existing = await _unitOfWork.ChatSessions.GetByConnectionRequestIdAsync(connectionRequestId);
            if (existing is not null)
            {
                return MapSession(existing);
            }

            var session = new ChatSession
            {
                ConnectionRequestId = connectionRequestId,
                IsOpen = true,
                StartTime = DateTime.UtcNow
            };

            await _unitOfWork.ChatSessions.AddAsync(session);
            await _unitOfWork.SaveChangesAsync();

            var created = await _unitOfWork.ChatSessions.GetByIdAsync(session.ChatSessionId);
            return created is null ? null : MapSession(created);
        }

        public async Task<ChatSessionResponse?> GetSessionAsync(int sessionId, int userId)
        {
            var session = await _unitOfWork.ChatSessions.GetByIdAsync(sessionId);
            if (session is null || !IsParticipant(session, userId)) return null;
            return MapSession(session);
        }

        public async Task<ChatSessionResponse?> GetSessionByConnectionRequestAsync(int connectionRequestId, int userId)
        {
            var session = await _unitOfWork.ChatSessions.GetByConnectionRequestIdAsync(connectionRequestId);
            if (session is not null)
            {
                return IsParticipant(session, userId) ? MapSession(session) : null;
            }

            return await OpenSessionByConnectionRequestAsync(connectionRequestId, userId);
        }

        public async Task<IEnumerable<ChatSessionResponse>> GetMySessionsAsync(int userId)
        {
            var sessions = await _unitOfWork.ChatSessions.GetByUserIdAsync(userId);
            return sessions.Select(MapSession);
        }

        public async Task<bool> CloseSessionAsync(int sessionId, int userId)
        {
            var session = await _unitOfWork.ChatSessions.GetByIdAsync(sessionId);
            if (session is null || !session.IsOpen || !IsParticipant(session, userId))
                return false;

            session.IsOpen  = false;
            session.EndTime = DateTime.UtcNow;
            _unitOfWork.ChatSessions.Update(session);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        // ── Helpers ──────────────────────────────────────────────────────

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

        private static ChatSessionResponse MapSession(ChatSession s) => new()
        {
            ChatSessionId = s.ChatSessionId,
            BookingId     = s.BookingId,
            ConnectionRequestId = s.ConnectionRequestId,
            SessionType   = s.ConnectionRequestId.HasValue ? "ConnectionRequest" : "Booking",
            IsOpen        = s.IsOpen,
            StartTime     = s.StartTime,
            EndTime       = s.EndTime,
            AdvisorName   = s.Booking?.Advisor?.User?.UserName ?? string.Empty,
            CustomerName  = s.Booking?.Customer?.UserName      ?? string.Empty,
            StartupName   = s.ConnectionRequest?.Project?.Startup?.User?.UserName ?? string.Empty,
            InvestorName  = s.ConnectionRequest?.Investor?.User?.UserName ?? string.Empty,
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
