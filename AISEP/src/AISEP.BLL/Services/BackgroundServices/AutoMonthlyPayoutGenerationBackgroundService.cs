using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.Services.MonthlyPayouts;
using AISEP.DAL.Common;
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
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var monthlyPayoutBatchService = scope.ServiceProvider.GetRequiredService<IMonthlyPayoutBatchService>();

                var nowLocal = GetVietnamNow();
                var (year, month) = GetLatestClosedCycle(nowLocal);

                var existingBatch = await unitOfWork.MonthlyPayoutBatches.GetByPeriodAsync(year, month);
                if (existingBatch is not null)
                {
                    return;
                }

                await monthlyPayoutBatchService.GenerateAsync(new GenerateMonthlyPayoutRequest
                {
                    Year = year,
                    Month = month
                });

                _logger.LogInformation("Auto-generated monthly payout batch for {Month}/{Year}.", month, year);
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

        private static (int Year, int Month) GetLatestClosedCycle(DateTime nowLocal)
        {
            var previousMonth = new DateTime(nowLocal.Year, nowLocal.Month, 1).AddMonths(-1);
            return (previousMonth.Year, previousMonth.Month);
        }
    }
}
