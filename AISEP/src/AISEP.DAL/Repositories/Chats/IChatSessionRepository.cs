using AISEP.DAL.Entities;

namespace AISEP.DAL.Repositories.Chats
{
    public interface IChatSessionRepository
    {
        Task<ChatSession?> GetByIdAsync(int sessionId);
        Task<ChatSession?> GetByBookingIdAsync(int bookingId);
        Task<ChatSession?> GetByConnectionRequestIdAsync(int connectionRequestId);
        Task<IEnumerable<ChatSession>> GetByUserIdAsync(int userId);
        Task AddAsync(ChatSession session);
        void Update(ChatSession session);
    }
}
