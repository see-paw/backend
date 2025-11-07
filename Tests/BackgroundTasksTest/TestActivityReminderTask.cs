using Domain.Enums;
using Infrastructure.BackgroundTasks.Tasks.ActivityTasks;
using Microsoft.Extensions.Logging;

namespace Tests.BackgroundTasksTest;

class TestActivityReminderTask : BaseActivityReminderTask
{
    public TestActivityReminderTask(ILogger logger) : base(logger) { }

    protected override ActivityType GetActivityType() => ActivityType.Ownership;
    protected override NotificationType GetUserStartReminderType() => NotificationType.OWNERSHIP_ACTIVITY_START_REMINDER_USER;
    protected override NotificationType GetAdminStartReminderType() => NotificationType.OWNERSHIP_ACTIVITY_START_REMINDER_SHELTER_ADMIN;
    protected override NotificationType GetUserEndReminderType() => NotificationType.OWNERSHIP_ACTIVITY_END_REMINDER_USER;
    protected override NotificationType GetAdminEndReminderType() => NotificationType.OWNERSHIP_ACTIVITY_END_REMINDER_SHELTER_ADMIN;
    protected override string GetActivityDisplayName() => "test";
}