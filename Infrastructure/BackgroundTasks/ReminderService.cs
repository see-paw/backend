using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.BackgroundTasks
{
    /// <summary>
    /// Background service responsible for periodically executing 
    /// all of the registered tasks that implement <see cref="IReminderTask"/>.
    /// </summary>
    public class ReminderService(
        IServiceScopeFactory scopeFactory,
        ILogger<ReminderService> logger) : BackgroundService
    {
        private static readonly int DEFAULT_CYCLE_INTERVAL = 5; // Minutes

        /// <summary>
        /// Main method executed by the application host.
        /// The loop runs indefinitely while the application is running,
        /// invoking all of the registered tasks.
        /// </summary>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("ReminderService started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = scopeFactory.CreateScope();
                var tasks = scope.ServiceProvider.GetRequiredService<IEnumerable<IReminderTask>>();

                foreach (var task in tasks)
                {
                    try
                    {
                        logger.LogInformation("Executing {TaskName}...", task.GetType().Name);
                        await task.ExecuteAsync(scope.ServiceProvider);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error executing {TaskName} task", task.GetType().Name);
                    }
                }

                // Time between cycles
                await Task.Delay(TimeSpan.FromMinutes(DEFAULT_CYCLE_INTERVAL), stoppingToken);
            }

            logger.LogInformation("ReminderService has stopped.");
        }
    }
}
