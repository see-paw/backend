using Application.Interfaces;
using Domain;
using Domain.Enums;
using Infrastructure.BackgroundTasks.Tasks;
using Infrastructure.BackgroundTasks.Tasks.ActivityTasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Persistence;
using Xunit;

namespace Tests.BackgroundTasks
{
    /// <summary>
    /// Unit tests for the BaseActivityReminderTask class.
    /// </summary>
    public class BaseActivityReminderTaskTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly Mock<ILogger<BaseActivityReminderTask>> _mockLogger;
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly Mock<IServiceProvider> _mockServiceProvider;
        private readonly Mock<IServiceScope> _mockScope;
        private readonly Mock<IServiceScopeFactory> _mockScopeFactory;
        private readonly TestActivityReminderTask _task;

        public BaseActivityReminderTaskTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _context = new AppDbContext(options);
            _mockLogger = new Mock<ILogger<BaseActivityReminderTask>>();
            _mockNotificationService = new Mock<INotificationService>();

            _mockScope = new Mock<IServiceScope>();
            _mockScopeFactory = new Mock<IServiceScopeFactory>();
            _mockServiceProvider = new Mock<IServiceProvider>();

            _mockScope.Setup(s => s.ServiceProvider).Returns(_mockServiceProvider.Object);
            _mockScopeFactory.Setup(f => f.CreateScope()).Returns(_mockScope.Object);

            _mockServiceProvider.Setup(sp => sp.GetService(typeof(IServiceScopeFactory))).Returns(_mockScopeFactory.Object);
            _mockServiceProvider.Setup(sp => sp.GetService(typeof(AppDbContext))).Returns(_context);
            _mockServiceProvider.Setup(sp => sp.GetService(typeof(INotificationService))).Returns(_mockNotificationService.Object);

            _task = new TestActivityReminderTask(_mockLogger.Object);
        }

        public void Dispose() => _context.Database.EnsureDeleted();

        /// <summary>
        /// Gets the current time in Lisbon timezone, which is what the task uses for calculations.
        /// </summary>
        private DateTime GetLisbonNow()
        {
            var lisbonTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Lisbon");
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, lisbonTimeZone);
        }

        /// <summary>
        /// Concrete subclass of BaseActivityReminderTask used for testing.
        /// </summary>
        private class TestActivityReminderTask : BaseActivityReminderTask
        {
            public TestActivityReminderTask(ILogger logger) : base(logger) { }

            protected override ActivityType GetActivityType() => ActivityType.Ownership;
            protected override NotificationType GetUserStartReminderType() => NotificationType.OWNERSHIP_ACTIVITY_START_REMINDER_USER;
            protected override NotificationType GetAdminStartReminderType() => NotificationType.OWNERSHIP_ACTIVITY_START_REMINDER_SHELTER_ADMIN;
            protected override NotificationType GetUserEndReminderType() => NotificationType.OWNERSHIP_ACTIVITY_END_REMINDER_USER;
            protected override NotificationType GetAdminEndReminderType() => NotificationType.OWNERSHIP_ACTIVITY_END_REMINDER_SHELTER_ADMIN;
            protected override string GetActivityDisplayName() => "test";
        }

        // ---------- TESTS ----------

        [Fact]
        public async Task ExecuteAsync_ShouldLogInformation_WhenNoActivities()
        {
            await _task.ExecuteAsync(_mockServiceProvider.Object);

            _mockLogger.Verify(
                l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("No test activities")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldNotSendReminder_WhenActivityOutsideWindow()
        {
            var lisbonNow = GetLisbonNow();
            var activity = new Activity
            {
                Id = Guid.NewGuid().ToString(),
                Type = ActivityType.Ownership,
                Status = ActivityStatus.Active,
                StartDate = lisbonNow.AddHours(30),
                EndDate = lisbonNow.AddHours(40),
                User = new User { Id = Guid.NewGuid().ToString(), Name = "User" },
                Animal = new Animal { Id = Guid.NewGuid().ToString(), Name = "Dog", ShelterId = "1" }
            };
            _context.Activities.Add(activity);
            _context.SaveChanges();

            await _task.ExecuteAsync(_mockServiceProvider.Object);

            _mockNotificationService.Verify(n => n.CreateAndSendToUserAsync(
                It.IsAny<string>(), It.IsAny<NotificationType>(), It.IsAny<string>(),
                It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldIgnoreWrongActivityType()
        {
            var lisbonNow = GetLisbonNow();
            var activity = new Activity
            {
                Id = Guid.NewGuid().ToString(),
                Type = ActivityType.Fostering,
                Status = ActivityStatus.Active,
                StartDate = lisbonNow.AddHours(24).AddMinutes(10),
                EndDate = lisbonNow.AddHours(25),
                User = new User { Id = Guid.NewGuid().ToString() },
                Animal = new Animal { Id = Guid.NewGuid().ToString(), ShelterId = "1" }
            };
            _context.Activities.Add(activity);
            _context.SaveChanges();

            await _task.ExecuteAsync(_mockServiceProvider.Object);

            _mockNotificationService.Verify(n => n.CreateAndSendToUserAsync(
                It.IsAny<string>(), It.IsAny<NotificationType>(), It.IsAny<string>(),
                It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldSendStartReminder_WhenStartDateInWindow()
        {
            var lisbonNow = GetLisbonNow();
            var activity = new Activity
            {
                Id = Guid.NewGuid().ToString(),
                Type = ActivityType.Ownership,
                Status = ActivityStatus.Active,
                StartDate = lisbonNow.AddHours(24).AddMinutes(5),
                EndDate = lisbonNow.AddHours(48),
                User = new User { Id = Guid.NewGuid().ToString(), Name = "John" },
                Animal = new Animal { Id = Guid.NewGuid().ToString(), Name = "Doggo", ShelterId = "1" }
            };
            _context.Activities.Add(activity);
            _context.SaveChanges();

            await _task.ExecuteAsync(_mockServiceProvider.Object);

            _mockNotificationService.Verify(n => n.CreateAndSendToUserAsync(
                activity.User.Id,
                NotificationType.OWNERSHIP_ACTIVITY_START_REMINDER_USER,
                It.Is<string>(msg => msg.Contains("começa")),
                It.IsAny<string?>(), null, activity.Id, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldSendEndReminder_WhenEndDateInWindow()
        {
            var lisbonNow = GetLisbonNow();
            var activity = new Activity
            {
                Id = Guid.NewGuid().ToString(),
                Type = ActivityType.Ownership,
                Status = ActivityStatus.Active,
                StartDate = lisbonNow.AddHours(10),
                EndDate = lisbonNow.AddHours(24).AddMinutes(5),
                User = new User { Id = Guid.NewGuid().ToString(), Name = "Jane" },
                Animal = new Animal { Id = Guid.NewGuid().ToString(), Name = "Cat", ShelterId = "1" }
            };
            _context.Activities.Add(activity);
            _context.SaveChanges();

            await _task.ExecuteAsync(_mockServiceProvider.Object);

            _mockNotificationService.Verify(n => n.CreateAndSendToUserAsync(
                activity.User.Id,
                NotificationType.OWNERSHIP_ACTIVITY_END_REMINDER_USER,
                It.Is<string>(msg => msg.Contains("acaba")),
                It.IsAny<string?>(), null, activity.Id, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldNotSendReminder_WhenActivityIsCompleted()
        {
            var lisbonNow = GetLisbonNow();
            var activity = new Activity
            {
                Id = Guid.NewGuid().ToString(),
                Type = ActivityType.Ownership,
                Status = ActivityStatus.Completed,
                StartDate = lisbonNow.AddHours(23),
                EndDate = lisbonNow.AddHours(24),
                User = new User { Id = "1" },
                Animal = new Animal { Id = "2", ShelterId = "1" }
            };
            _context.Activities.Add(activity);
            _context.SaveChanges();

            await _task.ExecuteAsync(_mockServiceProvider.Object);

            _mockNotificationService.Verify(n => n.CreateAndSendToUserAsync(
                It.IsAny<string>(), It.IsAny<NotificationType>(), It.IsAny<string>(),
                It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldNotDuplicateExistingStartNotification()
        {
            var lisbonNow = GetLisbonNow();
            var activity = new Activity
            {
                Id = Guid.NewGuid().ToString(),
                Type = ActivityType.Ownership,
                Status = ActivityStatus.Active,
                StartDate = lisbonNow.AddHours(24).AddMinutes(10),
                EndDate = lisbonNow.AddHours(48),
                User = new User { Id = "1" },
                Animal = new Animal { Id = "2", ShelterId = "1" }
            };
            _context.Activities.Add(activity);
            _context.Notifications.Add(new Notification
            {
                ActivityId = activity.Id,
                Type = NotificationType.OWNERSHIP_ACTIVITY_START_REMINDER_USER
            });
            _context.SaveChanges();

            await _task.ExecuteAsync(_mockServiceProvider.Object);

            _mockNotificationService.Verify(n => n.CreateAndSendToUserAsync(
                It.IsAny<string>(), It.IsAny<NotificationType>(), It.IsAny<string>(),
                It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldContinue_WhenNotificationServiceThrowsException()
        {
            var lisbonNow = GetLisbonNow();
            var activity = new Activity
            {
                Id = Guid.NewGuid().ToString(),
                Type = ActivityType.Ownership,
                Status = ActivityStatus.Active,
                StartDate = lisbonNow.AddHours(24).AddMinutes(10),
                EndDate = lisbonNow.AddHours(48),
                User = new User { Id = "1" },
                Animal = new Animal { Id = "2", ShelterId = "1" }
            };
            _context.Activities.Add(activity);
            _context.SaveChanges();

            _mockNotificationService
                .Setup(n => n.CreateAndSendToUserAsync(
                    It.IsAny<string>(), It.IsAny<NotificationType>(), It.IsAny<string>(),
                    It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Send failure"));

            await _task.ExecuteAsync(_mockServiceProvider.Object);

            _mockLogger.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("Failed to send reminder")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldSendRemindersToAdmin_WhenAdminExists()
        {
            var lisbonNow = GetLisbonNow();
            var shelterId = Guid.NewGuid().ToString();
            var admin = new User { Id = Guid.NewGuid().ToString(), ShelterId = shelterId };
            var activity = new Activity
            {
                Id = Guid.NewGuid().ToString(),
                Type = ActivityType.Ownership,
                Status = ActivityStatus.Active,
                StartDate = lisbonNow.AddHours(24).AddMinutes(10),
                EndDate = lisbonNow.AddHours(48),
                User = new User { Id = Guid.NewGuid().ToString(), Name = "User" },
                Animal = new Animal { Id = Guid.NewGuid().ToString(), ShelterId = shelterId, Name = "Dog" }
            };
            _context.Users.Add(admin);
            _context.Activities.Add(activity);
            _context.SaveChanges();

            await _task.ExecuteAsync(_mockServiceProvider.Object);

            _mockNotificationService.Verify(n => n.CreateAndSendToUserAsync(
                admin.Id,
                NotificationType.OWNERSHIP_ACTIVITY_START_REMINDER_SHELTER_ADMIN,
                It.Is<string>(msg => msg.Contains("começa")),
                It.IsAny<string?>(), null, activity.Id, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldOnlyNotifyUser_WhenNoAdminFound()
        {
            var lisbonNow = GetLisbonNow();
            var activity = new Activity
            {
                Id = Guid.NewGuid().ToString(),
                Type = ActivityType.Ownership,
                Status = ActivityStatus.Active,
                StartDate = lisbonNow.AddHours(24).AddMinutes(10),
                EndDate = lisbonNow.AddHours(48),
                User = new User { Id = Guid.NewGuid().ToString(), Name = "User" },
                Animal = new Animal { Id = Guid.NewGuid().ToString(), ShelterId = Guid.NewGuid().ToString(), Name = "Cat" }
            };
            _context.Activities.Add(activity);
            _context.SaveChanges();

            await _task.ExecuteAsync(_mockServiceProvider.Object);

            _mockNotificationService.Verify(n => n.CreateAndSendToUserAsync(
                activity.User.Id,
                NotificationType.OWNERSHIP_ACTIVITY_START_REMINDER_USER,
                It.IsAny<string>(),
                It.IsAny<string?>(), It.IsAny<string?>(), activity.Id, It.IsAny<CancellationToken>()),
                Times.Once);

            _mockNotificationService.Verify(n => n.CreateAndSendToUserAsync(
                It.Is<string>(id => id != activity.User.Id),
                NotificationType.OWNERSHIP_ACTIVITY_START_REMINDER_SHELTER_ADMIN,
                It.IsAny<string>(),
                It.IsAny<string?>(), It.IsAny<string?>(), activity.Id, It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldSendAllReminders_WhenStartAndEndInWindow()
        {
            var lisbonNow = GetLisbonNow();
            var shelterId = Guid.NewGuid().ToString();
            var admin = new User { Id = Guid.NewGuid().ToString(), ShelterId = shelterId };
            var activity = new Activity
            {
                Id = Guid.NewGuid().ToString(),
                Type = ActivityType.Ownership,
                Status = ActivityStatus.Active,
                StartDate = lisbonNow.AddHours(24).AddMinutes(5),
                EndDate = lisbonNow.AddHours(24).AddMinutes(8),
                User = new User { Id = Guid.NewGuid().ToString(), Name = "User" },
                Animal = new Animal { Id = Guid.NewGuid().ToString(), ShelterId = shelterId, Name = "Buddy" }
            };
            _context.Users.Add(admin);
            _context.Activities.Add(activity);
            _context.SaveChanges();

            await _task.ExecuteAsync(_mockServiceProvider.Object);

            _mockNotificationService.Verify(n => n.CreateAndSendToUserAsync(
                It.IsAny<string>(), It.IsAny<NotificationType>(), It.IsAny<string>(),
                It.IsAny<string?>(), It.IsAny<string?>(), activity.Id, It.IsAny<CancellationToken>()),
                Times.Exactly(4));
        }
    }
}