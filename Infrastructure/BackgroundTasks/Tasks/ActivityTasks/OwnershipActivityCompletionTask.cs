using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Infrastructure.BackgroundTasks.Tasks.ActivityTasks
{
    /// <summary>
    /// Task responsible for automatically marking ownership activities as Completed
    /// when their EndDate has passed.
    /// </summary>
    public class OwnershipActivityCompletionTask(ILogger<OwnershipActivityCompletionTask> logger) : IReminderTask
    {
        /// <summary>
        /// Executes the task by updating all active ownership activities whose end date has passed.
        /// </summary>
        /// <param name="services">Scoped service provider for resolving dependencies.</param>
        public async Task ExecuteAsync(IServiceProvider services)
        {
            // Task is scoped and needs to create scoped services to avoid concurrency problems
            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Convert to Portugal's current DST (UTC = winter time, UTC + 1 = summer time)
            var lisbonTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Lisbon");
            var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, lisbonTimeZone);

            // Select all active ownership activities whose EndDate has passed
            var activitiesToComplete = await context.Activities
                .Where(a => a.Type == ActivityType.Ownership &&
                            a.Status == ActivityStatus.Active &&
                            a.EndDate <= now)
                .ToListAsync();

            if (activitiesToComplete.Count == 0)
            {
                logger.LogInformation("No ownership activities to mark as completed at {Time}.", now);
                return;
            }

            // Mark each activity as completed
            foreach (var activity in activitiesToComplete)
            {
                activity.Status = ActivityStatus.Completed;
                logger.LogInformation("Activity {ActivityId} marked as Completed.", activity.Id);
            }

            await context.SaveChangesAsync();
            logger.LogInformation("{Count} ownership activities marked as Completed at {Time}.",
                activitiesToComplete.Count, now);
        }
    }
}
