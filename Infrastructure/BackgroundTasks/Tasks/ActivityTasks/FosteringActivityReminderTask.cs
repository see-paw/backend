using Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Infrastructure.BackgroundTasks.Tasks.ActivityTasks
{
    /// <summary>
    /// Task responsible for sending reminders a few hours before fostering activities start or end.
    /// </summary>
    public class FosteringActivityReminderTask(ILogger<FosteringActivityReminderTask> logger)
        : BaseActivityReminderTask(logger)
    {
        /// <inheritdoc/>
        protected override ActivityType GetActivityType() => ActivityType.Fostering;

        /// <inheritdoc/>
        protected override string GetActivityDisplayName() => "fostering";

        /// <inheritdoc/>
        protected override NotificationType GetUserStartReminderType()
            => NotificationType.FOSTERING_ACTIVITY_START_REMINDER_USER;

        /// <inheritdoc/>
        protected override NotificationType GetAdminStartReminderType()
            => NotificationType.FOSTERING_ACTIVITY_START_REMINDER_SHELTER_ADMIN;

        /// <inheritdoc/>
        protected override NotificationType GetUserEndReminderType()
            => NotificationType.FOSTERING_ACTIVITY_END_REMINDER_USER;

        /// <inheritdoc/>
        protected override NotificationType GetAdminEndReminderType()
            => NotificationType.FOSTERING_ACTIVITY_END_REMINDER_SHELTER_ADMIN;
    }
}