using Application.Interfaces;
using Domain;
using Domain.Enums;
using Infrastructure.BackgroundTasks.Tasks.ActivityTasks;
using Infrastructure.Hubs;
using Infrastructure.Notifications;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Persistence;
using Xunit;

namespace Tests.IntegrationTests.BackgroundTasks;

/// <summary>
/// Integration tests for background tasks using a real test database.
/// These tests verify the complete flow with actual database operations.
/// 
/// Requirements:
/// - PostgreSQL test database configured in appsettings.Test.json
/// - Run DbInitializer to populate test data before tests
/// </summary>
[Collection("Database")]
public class ActivityReminderIntegrationTests : IAsyncLifetime
{
    private readonly IServiceProvider _serviceProvider;
    private readonly AppDbContext _context;
    private readonly string _testDatabaseName = $"TestDb_{Guid.NewGuid():N}";

    public ActivityReminderIntegrationTests()
    {
        var services = new ServiceCollection();

        // Configure real PostgreSQL test database
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql($"Host=localhost;Database={_testDatabaseName};Username=seepaw;Password=seepawpwd"));

        // Register background tasks
        services.AddScoped<OwnershipActivityReminderTask>();
        services.AddScoped<FosteringActivityReminderTask>();
        services.AddScoped<OwnershipActivityCompletionTask>();

        // Register logger
        services.AddLogging(builder => builder.AddConsole());

        // Register Identity UserManager (required by NotificationService)
        services.AddIdentityCore<User>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>();

        // Mock SignalR HubContext (not needed for these tests, but required by NotificationService)
        var mockHubContext = new Mock<IHubContext<NotificationHub>>();
        services.AddScoped(_ => mockHubContext.Object);

        // Register REAL NotificationService (it will create notifications in the database)
        services.AddScoped<INotificationService, NotificationService>();

        _serviceProvider = services.BuildServiceProvider();
        _context = _serviceProvider.GetRequiredService<AppDbContext>();
    }

    public async Task InitializeAsync()
    {
        await _context.Database.EnsureCreatedAsync();
        await SeedTestDataAsync();
    }

    public async Task DisposeAsync()
    {
        await _context.Database.EnsureDeletedAsync();
        await _context.DisposeAsync();
    }

    private async Task SeedTestDataAsync()
    {
        var shelter = new Shelter
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test Shelter",
            Street = "Test Street",
            City = "Lisbon",
            PostalCode = "1000-000",
            Phone = "912345678",
            NIF = "123456789",
            OpeningTime = new TimeOnly(9, 0),
            ClosingTime = new TimeOnly(18, 0)
        };

        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            UserName = "testuser",
            Email = "test@example.com",
            Name = "Test User",
            BirthDate = DateTime.UtcNow.AddYears(-25),
            Street = "User Street",
            City = "Lisbon",
            PostalCode = "1000-001"
        };

        var breed = new Breed
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test Breed",
            Description = "Breed for testing"
        };

        var animal = new Animal
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test Dog",
            ShelterId = shelter.Id,
            BreedId = breed.Id,
            Species = Species.Dog,
            Size = SizeType.Medium,
            Sex = SexType.Male,
            Colour = "Brown",
            BirthDate = new DateOnly(2020, 1, 1),
            Cost = 50,
            Sterilized = true
        };

        _context.Shelters.Add(shelter);
        _context.Users.Add(user);
        _context.Breeds.Add(breed);
        _context.Animals.Add(animal);
        await _context.SaveChangesAsync();
    }

    [Fact]
    public async Task OwnershipActivityCompletionTask_ShouldMarkAsCompleted_WhenEndDatePassed()
    {
        var user = await _context.Users.FirstAsync();
        var animal = await _context.Animals.FirstAsync();

        var activity = new Activity
        {
            Id = Guid.NewGuid().ToString(),
            UserId = user.Id,
            AnimalId = animal.Id,
            Type = ActivityType.Ownership,
            Status = ActivityStatus.Active,
            StartDate = DateTime.SpecifyKind(DateTime.UtcNow.AddDays(-7), DateTimeKind.Utc),
            EndDate = DateTime.SpecifyKind(DateTime.UtcNow.AddDays(-1), DateTimeKind.Utc)
        };

        _context.Activities.Add(activity);
        await _context.SaveChangesAsync();

        var task = _serviceProvider.GetRequiredService<OwnershipActivityCompletionTask>();
        await task.ExecuteAsync(_serviceProvider);

        _context.Entry(activity).Reload();
        Assert.Equal(ActivityStatus.Completed, activity.Status);
    }

    [Fact]
    public async Task OwnershipActivityReminderTask_ShouldCreateNotification_WhenActivityStartsIn24Hours()
    {
        var user = await _context.Users.FirstAsync();
        var animal = await _context.Animals.FirstAsync();

        var now = DateTime.UtcNow;

        var activity = new Activity
        {
            Id = Guid.NewGuid().ToString(),
            UserId = user.Id,
            AnimalId = animal.Id,
            Type = ActivityType.Ownership,
            Status = ActivityStatus.Active,
            StartDate = DateTime.SpecifyKind(now.AddHours(24).AddMinutes(10), DateTimeKind.Utc),
            EndDate = DateTime.SpecifyKind(now.AddHours(48), DateTimeKind.Utc)
        };

        _context.Activities.Add(activity);
        await _context.SaveChangesAsync();

        var notificationCountBefore = await _context.Notifications.CountAsync();

        var task = _serviceProvider.GetRequiredService<OwnershipActivityReminderTask>();
        await task.ExecuteAsync(_serviceProvider);

        var notificationCountAfter = await _context.Notifications.CountAsync();
        Assert.True(notificationCountAfter > notificationCountBefore);
    }

    [Fact]
    public async Task FosteringActivityReminderTask_ShouldCreateNotification_WhenActivityEndsIn24Hours()
    {
        var user = await _context.Users.FirstAsync();
        var animal = await _context.Animals.FirstAsync();

        var now = DateTime.UtcNow;

        var activity = new Activity
        {
            Id = Guid.NewGuid().ToString(),
            UserId = user.Id,
            AnimalId = animal.Id,
            Type = ActivityType.Fostering,
            Status = ActivityStatus.Active,
            StartDate = DateTime.SpecifyKind(now.AddHours(-48), DateTimeKind.Utc),
            EndDate = DateTime.SpecifyKind(now.AddHours(24).AddMinutes(10), DateTimeKind.Utc)
        };

        _context.Activities.Add(activity);
        await _context.SaveChangesAsync();

        var notificationCountBefore = await _context.Notifications.CountAsync();

        var task = _serviceProvider.GetRequiredService<FosteringActivityReminderTask>();
        await task.ExecuteAsync(_serviceProvider);

        var notificationCountAfter = await _context.Notifications.CountAsync();
        Assert.True(notificationCountAfter > notificationCountBefore);
    }

    [Fact]
    public async Task ReminderTasks_ShouldNotDuplicateNotifications_WhenRunMultipleTimes()
    {
        var user = await _context.Users.FirstAsync();
        var animal = await _context.Animals.FirstAsync();

        var now = DateTime.UtcNow;

        var activity = new Activity
        {
            Id = Guid.NewGuid().ToString(),
            UserId = user.Id,
            AnimalId = animal.Id,
            Type = ActivityType.Ownership,
            Status = ActivityStatus.Active,
            StartDate = DateTime.SpecifyKind(now.AddHours(24).AddMinutes(10), DateTimeKind.Utc),
            EndDate = DateTime.SpecifyKind(now.AddHours(48), DateTimeKind.Utc)
        };

        _context.Activities.Add(activity);
        await _context.SaveChangesAsync();

        var task = _serviceProvider.GetRequiredService<OwnershipActivityReminderTask>();

        await task.ExecuteAsync(_serviceProvider);
        var notificationsAfterFirstRun = await _context.Notifications
            .CountAsync(n => n.ActivityId == activity.Id);

        await task.ExecuteAsync(_serviceProvider);
        var notificationsAfterSecondRun = await _context.Notifications
            .CountAsync(n => n.ActivityId == activity.Id);

        Assert.Equal(notificationsAfterFirstRun, notificationsAfterSecondRun);
    }
}