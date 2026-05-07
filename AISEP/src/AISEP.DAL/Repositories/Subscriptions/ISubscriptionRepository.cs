using AISEP.DAL.Entities;

namespace AISEP.DAL.Repositories.Subscriptions
{
    public interface ISubscriptionRepository
    {
        IQueryable<Subscription> GetQuery();
        Task<Subscription?> GetByIdAsync(int subscriptionId);
        Task AddAsync(Subscription subscription);
        void Update(Subscription subscription);
        Task<IEnumerable<Subscription>> GetExpiredActiveAsync();
        Task<bool> HasActiveAsync(int userId);
        Task<Subscription?> GetLatestActiveAsync(int userId);
    }
}
