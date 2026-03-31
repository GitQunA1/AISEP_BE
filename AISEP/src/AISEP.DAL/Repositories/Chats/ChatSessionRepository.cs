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
            => await BuildSessionQuery()
                .FirstOrDefaultAsync(s => s.ChatSessionId == sessionId);

        public async Task<ChatSession?> GetByBookingIdAsync(int bookingId)
            => await BuildSessionQuery()
                .FirstOrDefaultAsync(s => s.BookingId == bookingId);

        public async Task<ChatSession?> GetByConnectionRequestIdAsync(int connectionRequestId)
            => await BuildSessionQuery()
                .FirstOrDefaultAsync(s => s.ConnectionRequestId == connectionRequestId);

        public async Task<IEnumerable<ChatSession>> GetByUserIdAsync(int userId)
            => await BuildSessionQuery()
                .Where(s =>
                    (s.BookingId.HasValue
                     && s.Booking != null
                     && (s.Booking.CustomerId == userId || s.Booking.Advisor!.UserId == userId))
                    ||
                    (s.ConnectionRequestId.HasValue
                     && s.ConnectionRequest != null
                     && (s.ConnectionRequest.Investor!.UserId == userId
                         || s.ConnectionRequest.Project!.Startup!.UserId == userId)))
                .OrderByDescending(s => s.StartTime)
                .ToListAsync();

        public async Task AddAsync(ChatSession session)
            => await _context.ChatSessions.AddAsync(session);

        public void Update(ChatSession session)
            => _context.ChatSessions.Update(session);

        private IQueryable<ChatSession> BuildSessionQuery()
            => _context.ChatSessions
                .Include(s => s.Booking!)
                    .ThenInclude(b => b.Advisor)
                        .ThenInclude(a => a.User)
                .Include(s => s.Booking!)
                    .ThenInclude(b => b.Customer)
                .Include(s => s.ConnectionRequest!)
                    .ThenInclude(cr => cr.Investor)
                        .ThenInclude(i => i.User)
                .Include(s => s.ConnectionRequest!)
                    .ThenInclude(cr => cr.Project)
                        .ThenInclude(p => p.Startup)
                            .ThenInclude(st => st.User)
                .Include(s => s.ChatMessages)
                    .ThenInclude(m => m.Sender);
    }
}
