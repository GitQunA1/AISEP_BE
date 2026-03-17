using AISEP.DAL.Common;
using AISEP.DAL.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AISEP.BLL.Services.BackgroundServices
{
    /// Runs once a day. Marks expired subscriptions as Expired and revokes
    /// User.IsPremium when the user has no remaining active subscriptions.
    public class SubscriptionExpiryBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<SubscriptionExpiryBackgroundService> _logger;
        private static readonly TimeSpan Interval = TimeSpan.FromHours(24);

        public SubscriptionExpiryBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<SubscriptionExpiryBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("SubscriptionExpiryBackgroundService started.");

            // Run immediately on startup, then every 24h
            while (!stoppingToken.IsCancellationRequested)
            {
                await ProcessExpiredSubscriptionsAsync();
                await Task.Delay(Interval, stoppingToken);
            }
        }

        private async Task ProcessExpiredSubscriptionsAsync()
        {
            try
            {
                // BackgroundService is Singleton, so we must create a scope
                // to resolve Scoped services (IUnitOfWork).
                using var scope = _scopeFactory.CreateScope();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var expiredSubscriptions = await unitOfWork.Subscriptions.GetExpiredActiveAsync();
                var subscriptionList = expiredSubscriptions.ToList();

                if (subscriptionList.Count == 0)
                    return;

                _logger.LogInformation(
                    "Processing {Count} expired subscription(s).", subscriptionList.Count);

                // Collect distinct affected userIds
                var affectedUserIds = subscriptionList.Select(s => s.UserId).Distinct().ToList();

                // Mark all expired subscriptions as Expired
                foreach (var sub in subscriptionList)
                {
                    sub.Status = SubscriptionStatus.Expired;
                    unitOfWork.Subscriptions.Update(sub);
                }

                await unitOfWork.SaveChangesAsync();

                // For each affected user: revoke IsPremium if no active subscriptions remain
                foreach (var userId in affectedUserIds)
                {
                    var hasActive = await unitOfWork.Subscriptions.HasActiveAsync(userId);
                    if (!hasActive)
                    {
                        var user = await unitOfWork.Users.GetByIdAsync(userId);
                        if (user is not null && user.IsPremium)
                        {
                            user.IsPremium = false;
                            _logger.LogInformation(
                                "Revoked IsPremium for User {UserId} — no active subscriptions.", userId);
                        }
                    }
                }

                await unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while processing expired subscriptions.");
            }
        }
    }
}
