using Application.Interfaces;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Infrastructure.BackgroundTasks.Tasks
{
    /// <summary>
    /// Task responsible for sending reminders a few hours before ownership activities start or end.
    /// </summary>
    public class OwnershipActivityReminderTask(ILogger<OwnershipActivityReminderTask> logger) : IReminderTask
    {
        private static readonly int HOURS_BEFORE_REMINDER = 24;
        private static readonly int WINDOW_MINUTES = 10;

        /// <summary>
        /// Executes the reminder logic by querying upcoming or ending ownership activities
        /// and creating notifications for the user and the shelter admin if not already sent.
        /// </summary>
        public async Task ExecuteAsync(IServiceProvider services)
        {
            // Task is scoped and needs to create scoped services to avoid concurrency problems
            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            // Define time window where activities are scanned
            // Any activity starting in this time window is a trigger for this task
            var now = DateTime.UtcNow;
            var windowStart = now.AddHours(HOURS_BEFORE_REMINDER);
            var windowEnd = windowStart.AddMinutes(WINDOW_MINUTES);

            var activities = (await context.Activities
                .Include(a => a.Animal)
                .Include(a => a.User)
                .Where(a => a.Type == ActivityType.Ownership && a.Status == ActivityStatus.Active)
                .Where(a =>
                    (a.StartDate >= windowStart && a.StartDate <= windowEnd) ||
                    (a.EndDate >= windowStart && a.EndDate <= windowEnd))
                .ToListAsync())
                .DistinctBy(a => a.Id)
                .ToList();

            if (activities.Count == 0)
            {
                logger.LogInformation("No ownership activities to remind at {Time}", now);
                return;
            }

            logger.LogInformation("Found {Count} ownership activities to process for reminders.", activities.Count);

            foreach (var activity in activities)
            {
                try
                {
                    if (activity.StartDate >= windowStart && activity.StartDate <= windowEnd)
                        await SendStartRemindersAsync(notificationService, context, activity);

                    if (activity.EndDate >= windowStart && activity.EndDate <= windowEnd)
                        await SendEndRemindersAsync(notificationService, context, activity);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to send reminder for activity {ActivityId}", activity.Id);
                }
            }
        }

        /// <summary>
        /// Sends start reminder notifications to the user and the shelter admin if not already sent.
        /// </summary>
        private static async Task SendStartRemindersAsync(
            INotificationService notificationService,
            AppDbContext context,
            Domain.Activity activity)
        {
            bool userReminderExists = await context.Notifications
                .AnyAsync(n => n.ActivityId == activity.Id &&
                               n.Type == NotificationType.OWNERSHIP_ACTIVITY_START_REMINDER_USER);

            bool adminReminderExists = await context.Notifications
                .AnyAsync(n => n.ActivityId == activity.Id &&
                               n.Type == NotificationType.OWNERSHIP_ACTIVITY_START_REMINDER_SHELTER_ADMIN);

            if (!userReminderExists)
            {
                await notificationService.CreateAndSendToUserAsync(
                    userId: activity.User.Id,
                    type: NotificationType.OWNERSHIP_ACTIVITY_START_REMINDER_USER,
                    message: $"Reminder: your adoption activity with {activity.Animal.Name} starts at {activity.StartDate:t}.",
                    animalId: activity.Animal.Id,
                    activityId: activity.Id
                );
            }

            var adminId = await context.Users
                .Where(u => u.ShelterId == activity.Animal.ShelterId)
                .Select(u => u.Id)
                .FirstOrDefaultAsync();

            if (!adminReminderExists && !string.IsNullOrEmpty(adminId))
            {
                await notificationService.CreateAndSendToUserAsync(
                    userId: adminId,
                    type: NotificationType.OWNERSHIP_ACTIVITY_START_REMINDER_SHELTER_ADMIN,
                    message: $"Reminder: adoption activity for {activity.Animal.Name} with {activity.User.Name} starts at {activity.StartDate:t}.",
                    animalId: activity.Animal.Id,
                    activityId: activity.Id
                );
            }
        }

        /// <summary>
        /// Sends end reminder notifications to the user and the shelter admin if not already sent.
        /// </summary>
        private static async Task SendEndRemindersAsync(
            INotificationService notificationService,
            AppDbContext context,
            Domain.Activity activity)
        {
            bool userReminderExists = await context.Notifications
                .AnyAsync(n => n.ActivityId == activity.Id &&
                               n.Type == NotificationType.OWNERSHIP_ACTIVITY_END_REMINDER_USER);

            bool adminReminderExists = await context.Notifications
                .AnyAsync(n => n.ActivityId == activity.Id &&
                               n.Type == NotificationType.OWNERSHIP_ACTIVITY_END_REMINDER_SHELTER_ADMIN);

            if (!userReminderExists)
            {
                await notificationService.CreateAndSendToUserAsync(
                    userId: activity.User.Id,
                    type: NotificationType.OWNERSHIP_ACTIVITY_END_REMINDER_USER,
                    message: $"Reminder: your adoption activity with {activity.Animal.Name} will end at {activity.EndDate:t}.",
                    animalId: activity.Animal.Id,
                    activityId: activity.Id
                );
            }

            var adminId = await context.Users
                .Where(u => u.ShelterId == activity.Animal.ShelterId)
                .Select(u => u.Id)
                .FirstOrDefaultAsync();

            if (!adminReminderExists && !string.IsNullOrEmpty(adminId))
            {
                await notificationService.CreateAndSendToUserAsync(
                    userId: adminId,
                    type: NotificationType.OWNERSHIP_ACTIVITY_END_REMINDER_SHELTER_ADMIN,
                    message: $"Reminder: adoption activity for {activity.Animal.Name} with {activity.User.Name} will end at {activity.EndDate:t}.",
                    animalId: activity.Animal.Id,
                    activityId: activity.Id
                );
            }
        }
    }
}
