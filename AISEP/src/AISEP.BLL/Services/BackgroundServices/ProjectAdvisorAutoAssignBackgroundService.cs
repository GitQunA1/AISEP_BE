using AISEP.BLL.Services.ProjectAdvisorAssignments;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AISEP.BLL.Services.BackgroundServices
{
    public class ProjectAdvisorAutoAssignBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ProjectAdvisorAutoAssignBackgroundService> _logger;
        private static readonly TimeSpan Interval = TimeSpan.FromMinutes(1);

        public ProjectAdvisorAutoAssignBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<ProjectAdvisorAutoAssignBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ProjectAdvisorAutoAssignBackgroundService started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                await ProcessAssignmentsAsync(stoppingToken);
                await Task.Delay(Interval, stoppingToken);
            }
        }

        private async Task ProcessAssignmentsAsync(CancellationToken cancellationToken)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var autoAssignService = scope.ServiceProvider.GetRequiredService<IProjectAdvisorAutoAssignService>();

                var assignedCount = await autoAssignService.AutoAssignUnassignedApprovedProjectsAsync(cancellationToken);
                if (assignedCount > 0)
                {
                    _logger.LogInformation(
                        "Auto-assigned advisor for {Count} approved project(s) that previously had no assignment.",
                        assignedCount);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while auto-assigning advisors for approved projects.");
            }
        }
    }
}
