using AISEP.BLL.Services.Bookings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AISEP.BLL.Services.BackgroundServices
{
    public class BookingResponseExpiryBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<BookingResponseExpiryBackgroundService> _logger;
        private static readonly TimeSpan Interval = TimeSpan.FromMinutes(30);

        public BookingResponseExpiryBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<BookingResponseExpiryBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("BookingResponseExpiryBackgroundService started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                await ProcessExpiredBookingsAsync();
                await Task.Delay(Interval, stoppingToken);
            }
        }

        private async Task ProcessExpiredBookingsAsync()
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();

                var expiredCount = await bookingService.ExpirePendingAdvisorResponsesAsync();
                if (expiredCount > 0)
                {
                    _logger.LogInformation(
                        "Auto-cancelled {Count} booking(s) because advisor response exceeded 12h.",
                        expiredCount);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while processing booking advisor-response timeouts.");
            }
        }
    }
}
