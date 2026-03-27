using AISEP.DAL.Entities;

namespace AISEP.DAL.Repositories.Chats
{
    public interface IChatMessageRepository
    {
        Task<ChatMessage?> GetByIdAsync(int messageId);
        Task<IEnumerable<ChatMessage>> GetBySessionIdAsync(int sessionId);
        Task AddAsync(ChatMessage message);
    }
}
