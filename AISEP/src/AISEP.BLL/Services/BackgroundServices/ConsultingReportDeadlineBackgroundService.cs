using AISEP.BLL.Services.ConsultingReports;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AISEP.BLL.Services.BackgroundServices
{
    public class ConsultingReportDeadlineBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ConsultingReportDeadlineBackgroundService> _logger;
        private static readonly TimeSpan Interval = TimeSpan.FromSeconds(30);

        public ConsultingReportDeadlineBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<ConsultingReportDeadlineBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ConsultingReportDeadlineBackgroundService started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                await ProcessDeadlinesAsync();
                await Task.Delay(Interval, stoppingToken);
            }
        }

        private async Task ProcessDeadlinesAsync()
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<IConsultingReportService>();
                var affectedCount = await service.ProcessReportDeadlinesAsync();
                if (affectedCount > 0)
                {
                    _logger.LogInformation(
                        "Processed {Count} consulting report deadline action(s).",
                        affectedCount);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while processing consulting report deadlines.");
            }
        }
    }
}
