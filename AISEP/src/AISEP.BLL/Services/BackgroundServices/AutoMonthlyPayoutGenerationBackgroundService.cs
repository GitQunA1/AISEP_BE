using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.Services.MonthlyPayouts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AISEP.BLL.Services.BackgroundServices
{
    public class AutoMonthlyPayoutGenerationBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<AutoMonthlyPayoutGenerationBackgroundService> _logger;
        private static readonly TimeSpan Interval = TimeSpan.FromHours(12);

        public AutoMonthlyPayoutGenerationBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<AutoMonthlyPayoutGenerationBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("AutoMonthlyPayoutGenerationBackgroundService started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                await TryGenerateAsync();
                await Task.Delay(Interval, stoppingToken);
            }
        }

        private async Task TryGenerateAsync()
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var monthlyPayoutBatchService = scope.ServiceProvider.GetRequiredService<IMonthlyPayoutBatchService>();

                var nowLocal = GetVietnamNow();
                var previousMonth = new DateTime(nowLocal.Year, nowLocal.Month, 1).AddMonths(-1);
                var fromDate = new DateTime(previousMonth.Year, previousMonth.Month, 1);
                var toDate = fromDate.AddMonths(1).AddDays(-1);

                await monthlyPayoutBatchService.GenerateAsync(new GenerateMonthlyPayoutRequest
                {
                    FromDate = fromDate,
                    ToDate = toDate
                });

                _logger.LogInformation(
                    "Auto payout sweep executed for closed-month range {FromDate:yyyy-MM-dd} - {ToDate:yyyy-MM-dd}.",
                    fromDate,
                    toDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while auto-generating monthly payout batch.");
            }
        }

        private static DateTime GetVietnamNow()
        {
            try
            {
                var vietnamTz = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTz);
            }
            catch
            {
                return DateTime.UtcNow.AddHours(7);
            }
        }
    }
}
