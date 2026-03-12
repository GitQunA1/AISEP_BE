using AISEP.DAL.Data;
using AISEP.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace AISEP.DAL.Repositories.Chats
{
    public class ChatSessionRepository : IChatSessionRepository
    {
        private readonly ApplicationDbContext _context;

        public ChatSessionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ChatSession?> GetByIdAsync(int sessionId)
            => await _context.ChatSessions
                .Include(s => s.Booking)
                    .ThenInclude(b => b.Advisor)
                        .ThenInclude(a => a.User)
                .Include(s => s.Booking)
                    .ThenInclude(b => b.Customer)
                .Include(s => s.ChatMessages)
                    .ThenInclude(m => m.Sender)
                .FirstOrDefaultAsync(s => s.ChatSessionId == sessionId);

        public async Task<ChatSession?> GetByBookingIdAsync(int bookingId)
            => await _context.ChatSessions
                .FirstOrDefaultAsync(s => s.BookingId == bookingId);

        public async Task<IEnumerable<ChatSession>> GetByUserIdAsync(int userId)
            => await _context.ChatSessions
                .Include(s => s.Booking)
                    .ThenInclude(b => b.Advisor)
                        .ThenInclude(a => a.User)
                .Include(s => s.Booking)
                    .ThenInclude(b => b.Customer)
                .Where(s => s.Booking.CustomerId == userId || s.Booking.Advisor.UserId == userId)
                .OrderByDescending(s => s.StartTime)
                .ToListAsync();

        public async Task AddAsync(ChatSession session)
            => await _context.ChatSessions.AddAsync(session);

        public void Update(ChatSession session)
            => _context.ChatSessions.Update(session);
    }
}
