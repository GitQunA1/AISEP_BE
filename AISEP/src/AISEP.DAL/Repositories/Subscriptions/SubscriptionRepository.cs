using AISEP.DAL.Data;
using AISEP.DAL.Entities;
using AISEP.DAL.Enums;
using Microsoft.EntityFrameworkCore;

namespace AISEP.DAL.Repositories.Subscriptions
{
    public class SubscriptionRepository : ISubscriptionRepository
    {
        private readonly ApplicationDbContext _context;

        public SubscriptionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Subscription subscription)
            => await _context.Subscriptions.AddAsync(subscription);

        public void Update(Subscription subscription)
            => _context.Subscriptions.Update(subscription);

        public async Task<IEnumerable<Subscription>> GetExpiredActiveAsync()
            => await _context.Subscriptions
                .Where(s => s.Status == SubscriptionStatus.Active && s.EndDate < DateTime.UtcNow)
                .ToListAsync();

        public async Task<bool> HasActiveAsync(int userId)
            => await _context.Subscriptions
                .AnyAsync(s => s.UserId == userId
                            && s.Status == SubscriptionStatus.Active
                            && s.EndDate >= DateTime.UtcNow);

        public async Task<Subscription?> GetLatestActiveAsync(int userId)
            => await _context.Subscriptions
                .Where(s => s.UserId == userId
                         && s.Status == SubscriptionStatus.Active
                         && s.EndDate >= DateTime.UtcNow)
                .OrderByDescending(s => s.EndDate)
                .FirstOrDefaultAsync();
    }
}
