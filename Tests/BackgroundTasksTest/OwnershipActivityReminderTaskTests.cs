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

namespace Tests.BackgroundTasksTest;

/// <summary>
/// Unit tests for OwnershipActivityReminderTask.
/// 
/// These tests validate that the task correctly sends reminder notifications
/// before ownership activities start or end, following these rules:
/// - Sends reminders 24 hours before start/end times
/// - Checks a 60-minute window for activities
/// - Only processes Ownership type activities with Active status
/// - Sends notifications to both user and shelter admin
/// - Does not send duplicate notifications
/// - Uses Lisbon timezone for time calculations
/// </summary>
public class OwnershipActivityReminderTaskTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<ILogger<OwnershipActivityReminderTask>> _mockLogger;
    private readonly Mock<INotificationService> _mockNotificationService;
    private readonly OwnershipActivityReminderTask _task;
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly Mock<IServiceScope> _mockScope;

    public OwnershipActivityReminderTaskTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _mockLogger = new Mock<ILogger<OwnershipActivityReminderTask>>();
        _mockNotificationService = new Mock<INotificationService>();
        _task = new OwnershipActivityReminderTask(_mockLogger.Object);

        _mockScope = new Mock<IServiceScope>();
        _mockServiceProvider = new Mock<IServiceProvider>();

        _mockScope.Setup(s => s.ServiceProvider).Returns(_mockServiceProvider.Object);
        _mockServiceProvider.Setup(sp => sp.GetService(typeof(AppDbContext))).Returns(_context);
        _mockServiceProvider.Setup(sp => sp.GetService(typeof(INotificationService))).Returns(_mockNotificationService.Object);

        var mockScopeFactory = new Mock<IServiceScopeFactory>();
        mockScopeFactory.Setup(f => f.CreateScope()).Returns(_mockScope.Object);
        _mockServiceProvider.Setup(sp => sp.GetService(typeof(IServiceScopeFactory))).Returns(mockScopeFactory.Object);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    private Activity CreateActivity(
        ActivityType type = ActivityType.Ownership,
        ActivityStatus status = ActivityStatus.Active,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        var shelterId = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid().ToString();
        var animalId = Guid.NewGuid().ToString();

        var user = new User
        {
            Id = userId,
            Name = "Test User",
            Email = "test@example.com",
            BirthDate = DateTime.UtcNow.AddYears(-25),
            Street = "Test Street",
            City = "Test City",
            PostalCode = "1234-567"
        };

        var animal = new Animal
        {
            Id = animalId,
            Name = "Test Animal",
            ShelterId = shelterId,
            Colour = "Brown",
            Cost = 100,
            Species = Species.Dog,
            Size = SizeType.Medium,
            Sex = SexType.Male,
            BirthDate = new DateOnly(2020, 1, 1),
            Sterilized = true,
            BreedId = Guid.NewGuid().ToString()
        };

        var admin = new User
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Admin User",
            Email = "admin@shelter.com",
            BirthDate = DateTime.UtcNow.AddYears(-30),
            Street = "Admin Street",
            City = "Admin City",
            PostalCode = "9999-999",
            ShelterId = shelterId
        };

        _context.Users.AddRange(user, admin);
        _context.Animals.Add(animal);

        var activity = new Activity
        {
            Id = Guid.NewGuid().ToString(),
            AnimalId = animalId,
            UserId = userId,
            Type = type,
            Status = status,
            StartDate = startDate ?? DateTime.UtcNow.AddHours(24),
            EndDate = endDate ?? DateTime.UtcNow.AddHours(48),
            Animal = animal,
            User = user
        };

        _context.Activities.Add(activity);
        _context.SaveChanges();

        return activity;
    }

    [Fact]
    public async Task ExecuteAsync_ShouldSendStartReminderToUser_WhenStartDateInWindow()
    {
        var now = DateTime.UtcNow;
        var startDate = now.AddHours(24).AddMinutes(10);
        CreateActivity(startDate: startDate);

        await _task.ExecuteAsync(_mockServiceProvider.Object);

        _mockNotificationService.Verify(
            s => s.CreateAndSendToUserAsync(
                It.IsAny<string>(),
                NotificationType.OWNERSHIP_ACTIVITY_START_REMINDER_USER,
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldSendStartReminderToAdmin_WhenStartDateInWindow()
    {
        var now = DateTime.UtcNow;
        var startDate = now.AddHours(24).AddMinutes(10);
        CreateActivity(startDate: startDate);

        await _task.ExecuteAsync(_mockServiceProvider.Object);

        _mockNotificationService.Verify(
            s => s.CreateAndSendToUserAsync(
                It.IsAny<string>(),
                NotificationType.OWNERSHIP_ACTIVITY_START_REMINDER_SHELTER_ADMIN,
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldSendEndReminderToUser_WhenEndDateInWindow()
    {
        var now = DateTime.UtcNow;
        var endDate = now.AddHours(24).AddMinutes(10);
        CreateActivity(endDate: endDate);

        await _task.ExecuteAsync(_mockServiceProvider.Object);

        _mockNotificationService.Verify(
            s => s.CreateAndSendToUserAsync(
                It.IsAny<string>(),
                NotificationType.OWNERSHIP_ACTIVITY_END_REMINDER_USER,
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldSendEndReminderToAdmin_WhenEndDateInWindow()
    {
        var now = DateTime.UtcNow;
        var endDate = now.AddHours(24).AddMinutes(10);
        CreateActivity(endDate: endDate);

        await _task.ExecuteAsync(_mockServiceProvider.Object);

        _mockNotificationService.Verify(
            s => s.CreateAndSendToUserAsync(
                It.IsAny<string>(),
                NotificationType.OWNERSHIP_ACTIVITY_END_REMINDER_SHELTER_ADMIN,
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldNotSendReminder_WhenStartDateOutsideWindow()
    {
        var now = DateTime.UtcNow;
        var startDate = now.AddHours(26);
        CreateActivity(startDate: startDate);

        await _task.ExecuteAsync(_mockServiceProvider.Object);

        _mockNotificationService.Verify(
            s => s.CreateAndSendToUserAsync(
                It.IsAny<string>(),
                It.IsAny<NotificationType>(),
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldNotSendDuplicateUserStartReminder_WhenNotificationExists()
    {
        var now = DateTime.UtcNow;
        var startDate = now.AddHours(24).AddMinutes(10);
        var activity = CreateActivity(startDate: startDate);

        _context.Notifications.Add(new Notification
        {
            ActivityId = activity.Id,
            Type = NotificationType.OWNERSHIP_ACTIVITY_START_REMINDER_USER,
            Message = "Test",
            UserId = activity.UserId
        });
        _context.SaveChanges();

        await _task.ExecuteAsync(_mockServiceProvider.Object);

        _mockNotificationService.Verify(
            s => s.CreateAndSendToUserAsync(
                It.IsAny<string>(),
                NotificationType.OWNERSHIP_ACTIVITY_START_REMINDER_USER,
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldNotSendDuplicateAdminStartReminder_WhenNotificationExists()
    {
        var now = DateTime.UtcNow;
        var startDate = now.AddHours(24).AddMinutes(10);
        var activity = CreateActivity(startDate: startDate);

        var admin = await _context.Users.FirstAsync(u => u.ShelterId == activity.Animal.ShelterId);

        _context.Notifications.Add(new Notification
        {
            ActivityId = activity.Id,
            Type = NotificationType.OWNERSHIP_ACTIVITY_START_REMINDER_SHELTER_ADMIN,
            Message = "Test",
            UserId = admin.Id
        });
        _context.SaveChanges();

        await _task.ExecuteAsync(_mockServiceProvider.Object);

        _mockNotificationService.Verify(
            s => s.CreateAndSendToUserAsync(
                It.IsAny<string>(),
                NotificationType.OWNERSHIP_ACTIVITY_START_REMINDER_SHELTER_ADMIN,
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldOnlyProcessOwnershipActivities_IgnoringFostering()
    {
        var now = DateTime.UtcNow;
        var startDate = now.AddHours(24).AddMinutes(10);
        CreateActivity(type: ActivityType.Fostering, startDate: startDate);

        await _task.ExecuteAsync(_mockServiceProvider.Object);

        _mockNotificationService.Verify(
            s => s.CreateAndSendToUserAsync(
                It.IsAny<string>(),
                It.IsAny<NotificationType>(),
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldOnlyProcessActiveStatus_IgnoringCompleted()
    {
        var now = DateTime.UtcNow;
        var startDate = now.AddHours(24).AddMinutes(10);
        CreateActivity(status: ActivityStatus.Completed, startDate: startDate);

        await _task.ExecuteAsync(_mockServiceProvider.Object);

        _mockNotificationService.Verify(
            s => s.CreateAndSendToUserAsync(
                It.IsAny<string>(),
                It.IsAny<NotificationType>(),
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldOnlyProcessActiveStatus_IgnoringCancelled()
    {
        var now = DateTime.UtcNow;
        var startDate = now.AddHours(24).AddMinutes(10);
        CreateActivity(status: ActivityStatus.Cancelled, startDate: startDate);

        await _task.ExecuteAsync(_mockServiceProvider.Object);

        _mockNotificationService.Verify(
            s => s.CreateAndSendToUserAsync(
                It.IsAny<string>(),
                It.IsAny<NotificationType>(),
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldNotThrow_WhenNoActivitiesToRemind()
    {
        var exception = await Record.ExceptionAsync(
            () => _task.ExecuteAsync(_mockServiceProvider.Object));

        Assert.Null(exception);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldLogInformation_WhenNoActivitiesToRemind()
    {
        await _task.ExecuteAsync(_mockServiceProvider.Object);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("No adoption activities")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldIncludeAnimalNameInMessage_WhenSendingNotification()
    {
        var now = DateTime.UtcNow;
        var startDate = now.AddHours(24).AddMinutes(10);
        var activity = CreateActivity(startDate: startDate);

        await _task.ExecuteAsync(_mockServiceProvider.Object);

        _mockNotificationService.Verify(
            s => s.CreateAndSendToUserAsync(
                It.IsAny<string>(),
                It.IsAny<NotificationType>(),
                It.Is<string>(msg => msg.Contains(activity.Animal.Name)),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldSendBothStartAndEndReminders_WhenBothInWindow()
    {
        var now = DateTime.UtcNow;
        var startDate = now.AddHours(24).AddMinutes(10);
        var endDate = now.AddHours(24).AddMinutes(20);
        CreateActivity(startDate: startDate, endDate: endDate);

        await _task.ExecuteAsync(_mockServiceProvider.Object);

        _mockNotificationService.Verify(
            s => s.CreateAndSendToUserAsync(
                It.IsAny<string>(),
                It.IsAny<NotificationType>(),
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()),
            Times.Exactly(4));
    }
}