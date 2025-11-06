using Domain.Enums;
using Infrastructure.BackgroundTasks.Tasks.ActivityTasks;
using Microsoft.Extensions.Logging;

namespace Infrastructure.BackgroundTasks.Tasks.ActivityTasks
{
    /// <summary>
    /// Task responsible for sending reminders a few hours before ownership activities start or end.
    /// </summary>
    public class OwnershipActivityReminderTask(ILogger<OwnershipActivityReminderTask> logger)
        : BaseActivityReminderTask(logger)
    {
        /// <inheritdoc/>
        protected override ActivityType GetActivityType() => ActivityType.Ownership;

        /// <inheritdoc/>
        protected override string GetActivityDisplayName() => "adoption";

        /// <inheritdoc/>
        protected override NotificationType GetUserStartReminderType()
            => NotificationType.OWNERSHIP_ACTIVITY_START_REMINDER_USER;

        /// <inheritdoc/>
        protected override NotificationType GetAdminStartReminderType()
            => NotificationType.OWNERSHIP_ACTIVITY_START_REMINDER_SHELTER_ADMIN;

        /// <inheritdoc/>
        protected override NotificationType GetUserEndReminderType()
            => NotificationType.OWNERSHIP_ACTIVITY_END_REMINDER_USER;

        /// <inheritdoc/>
        protected override NotificationType GetAdminEndReminderType()
            => NotificationType.OWNERSHIP_ACTIVITY_END_REMINDER_SHELTER_ADMIN;
    }
}