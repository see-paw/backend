using Application.Interfaces;
using Domain;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Infrastructure.BackgroundTasks.Tasks.ActivityTasks
{
    /// <summary>
    /// Base class for activity reminder tasks (Fostering and Ownership).
    /// Handles common logic for sending reminders before activities start or end.
    /// </summary>
    public abstract class BaseActivityReminderTask(ILogger logger) : IReminderTask
    {
        /// <summary>
        /// Number of hours before the activity starts/ends when reminders should be sent.
        /// </summary>
        protected static readonly int HOURS_BEFORE_REMINDER = 24;

        /// <summary>
        /// Time window in minutes for scanning activities that need reminders.
        /// Activities within this window from the reminder trigger time will be processed.
        /// </summary>
        protected static readonly int WINDOW_MINUTES = 60;

        /// <summary>
        /// Gets the type of activity this task handles (Fostering or Ownership).
        /// </summary>
        protected abstract ActivityType GetActivityType();

        /// <summary>
        /// Gets the notification type to send to the user when the activity starts.
        /// </summary>
        protected abstract NotificationType GetUserStartReminderType();

        /// <summary>
        /// Gets the notification type to send to the shelter admin when the activity starts.
        /// </summary>
        protected abstract NotificationType GetAdminStartReminderType();

        /// <summary>
        /// Gets the notification type to send to the user when the activity ends.
        /// </summary>
        protected abstract NotificationType GetUserEndReminderType();

        /// <summary>
        /// Gets the notification type to send to the shelter admin when the activity ends.
        /// </summary>
        protected abstract NotificationType GetAdminEndReminderType();

        /// <summary>
        /// Gets the display name for the activity type (e.g., "fostering" or "adoption").
        /// Used in log messages and notifications.
        /// </summary>
        protected abstract string GetActivityDisplayName();

        /// <summary>
        /// Executes the reminder logic by querying upcoming or ending activities
        /// and creating notifications for the user and the shelter admin if not already sent.
        /// </summary>
        /// <param name="services">Scoped service provider for resolving dependencies.</param>
        public async Task ExecuteAsync(IServiceProvider services)
        {
            // Task is scoped and needs to create scoped services to avoid concurrency problems
            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            var now = DateTime.UtcNow;

            // Define time window where activities are scanned
            // Any activity starting in this time window is a trigger for this task
            var windowStart = now.AddHours(HOURS_BEFORE_REMINDER);
            var windowEnd = windowStart.AddMinutes(WINDOW_MINUTES);

            var activityType = GetActivityType();
            var activities = (await context.Activities
                .Include(a => a.Animal)
                .Include(a => a.User)
                .Where(a => a.Type == activityType && a.Status == ActivityStatus.Active)
                .Where(a =>
                    (a.StartDate >= windowStart && a.StartDate <= windowEnd) ||
                    (a.EndDate >= windowStart && a.EndDate <= windowEnd))
                .ToListAsync())
                .DistinctBy(a => a.Id)
                .ToList();

            if (activities.Count == 0)
            {
                logger.LogInformation("No {ActivityType} activities to remind at {Time}",
                    GetActivityDisplayName(), now);
                return;
            }

            logger.LogInformation("Found {Count} {ActivityType} activities to process for reminders.",
                activities.Count, GetActivityDisplayName());

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
                    logger.LogError(ex, "Failed to send reminder for {ActivityType} activity {ActivityId}",
                        GetActivityDisplayName(), activity.Id);
                }
            }
        }

        /// <summary>
        /// Gets the shelter admin user ID for a given shelter.
        /// Uses OrderBy to ensure consistent results in case of data inconsistencies.
        /// </summary>
        /// <param name="context">Database context.</param>
        /// <param name="shelterId">The unique identifier of the shelter.</param>
        /// <returns>The admin user ID if found, otherwise null.</returns>
        protected static async Task<string?> GetShelterAdminAsync(AppDbContext context, string shelterId)
        {
            return await context.Users
                .Where(u => u.ShelterId == shelterId)
                .OrderBy(u => u.Id)
                .Select(u => u.Id)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Sends start reminder notifications to the user and the shelter admin if not already sent.
        /// </summary>
        /// <param name="notificationService">Service for creating and sending notifications.</param>
        /// <param name="context">Database context.</param>
        /// <param name="activity">The activity for which to send start reminders.</param>
        private async Task SendStartRemindersAsync(
            INotificationService notificationService,
            AppDbContext context,
            Activity activity)
        {
            var userStartReminderType = GetUserStartReminderType();
            bool userReminderExists = await context.Notifications
                .AnyAsync(n => n.ActivityId == activity.Id &&
                               n.Type == userStartReminderType);

            var adminStartReminderType = GetAdminStartReminderType();
            bool adminReminderExists = await context.Notifications
                .AnyAsync(n => n.ActivityId == activity.Id &&
                               n.Type == adminStartReminderType);

            if (!userReminderExists)
            {
                await notificationService.CreateAndSendToUserAsync(
                    userId: activity.User.Id,
                    type: GetUserStartReminderType(),
                    message: $"Lembrete: A tua atividade de {GetActivityDisplayName()} com o/a {activity.Animal.Name} começa em {activity.StartDate:t}.",
                    animalId: activity.Animal.Id,
                    activityId: activity.Id
                );
            }

            var adminId = await GetShelterAdminAsync(context, activity.Animal.ShelterId);

            if (!adminReminderExists && !string.IsNullOrEmpty(adminId))
            {
                await notificationService.CreateAndSendToUserAsync(
                    userId: adminId,
                    type: GetAdminStartReminderType(),
                    message: $"Lembrete: A atividade de {GetActivityDisplayName()} com o/a {activity.Animal.Name} do utilizador {activity.User.Name} começa em {activity.StartDate:t}.",
                    animalId: activity.Animal.Id,
                    activityId: activity.Id
                );
            }
        }

        /// <summary>
        /// Sends end reminder notifications to the user and the shelter admin if not already sent.
        /// </summary>
        /// <param name="notificationService">Service for creating and sending notifications.</param>
        /// <param name="context">Database context.</param>
        /// <param name="activity">The activity for which to send end reminders.</param>
        private async Task SendEndRemindersAsync(
            INotificationService notificationService,
            AppDbContext context,
            Activity activity)
        {
            var userEndReminderType = GetUserEndReminderType();
            bool userReminderExists = await context.Notifications
                .AnyAsync(n => n.ActivityId == activity.Id &&
                               n.Type == userEndReminderType);

            var adminEndReminderType = GetAdminEndReminderType();
            bool adminReminderExists = await context.Notifications
                .AnyAsync(n => n.ActivityId == activity.Id &&
                               n.Type == adminEndReminderType);

            if (!userReminderExists)
            {
                await notificationService.CreateAndSendToUserAsync(
                    userId: activity.User.Id,
                    type: GetUserEndReminderType(),
                    message: $"Lembrete: A tua atividade de {GetActivityDisplayName()} com o/a {activity.Animal.Name} vai acabar em {activity.EndDate:t}.",
                    animalId: activity.Animal.Id,
                    activityId: activity.Id
                );
            }

            var adminId = await GetShelterAdminAsync(context, activity.Animal.ShelterId);

            if (!adminReminderExists && !string.IsNullOrEmpty(adminId))
            {
                await notificationService.CreateAndSendToUserAsync(
                    userId: adminId,
                    type: GetAdminEndReminderType(),
                    message: $"Lembrete: A atividade de {GetActivityDisplayName()} com o/a {activity.Animal.Name} do utilizador {activity.User.Name} acaba em {activity.EndDate:t}.",
                    animalId: activity.Animal.Id,
                    activityId: activity.Id
                );
            }
        }
    }
}