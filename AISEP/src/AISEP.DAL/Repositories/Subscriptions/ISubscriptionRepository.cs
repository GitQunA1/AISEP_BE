using AISEP.DAL.Entities;

namespace AISEP.DAL.Repositories.Subscriptions
{
    public interface ISubscriptionRepository
    {
        Task AddAsync(Subscription subscription);
    }
}
