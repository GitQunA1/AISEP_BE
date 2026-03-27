using AISEP.DAL.Data;
using AISEP.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace AISEP.DAL.Repositories.Chats
{
    public class ChatMessageRepository : IChatMessageRepository
    {
        private readonly ApplicationDbContext _context;

        public ChatMessageRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ChatMessage?> GetByIdAsync(int messageId)
            => await _context.ChatMessages
                .Include(m => m.Sender)
                .FirstOrDefaultAsync(m => m.ChatMessageId == messageId);

        public async Task<IEnumerable<ChatMessage>> GetBySessionIdAsync(int sessionId)
            => await _context.ChatMessages
                .Include(m => m.Sender)
                .Where(m => m.ChatSessionId == sessionId)
                .OrderBy(m => m.SentAt)
                .ToListAsync();

        public async Task AddAsync(ChatMessage message)
            => await _context.ChatMessages.AddAsync(message);
    }
}
