using AISEP.DAL.Data;
using AISEP.DAL.Entities;

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
    }
}
