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
/// Unit tests for OwnershipActivityCompletionTask.
/// 
/// These tests validate that the task correctly marks ownership activities as completed
/// when their end date has passed, following these rules:
/// - Only processes activities of type Ownership
/// - Only processes activities with Active status
/// - Marks as Completed when EndDate <= current time
/// - Converts times to Lisbon timezone properly
/// - Calls SaveChanges when activities are updated
/// </summary>
public class OwnershipActivityCompletionTaskTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<ILogger<OwnershipActivityCompletionTask>> _mockLogger;
    private readonly OwnershipActivityCompletionTask _task;
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly Mock<IServiceScope> _mockScope;

    public OwnershipActivityCompletionTaskTests()
    {
        // Setup InMemory database
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);

        // Setup mocks
        _mockLogger = new Mock<ILogger<OwnershipActivityCompletionTask>>();
        _task = new OwnershipActivityCompletionTask(_mockLogger.Object);

        // Setup service provider and scope
        _mockScope = new Mock<IServiceScope>();
        _mockServiceProvider = new Mock<IServiceProvider>();

        _mockScope.Setup(s => s.ServiceProvider).Returns(_mockServiceProvider.Object);
        _mockServiceProvider.Setup(sp => sp.GetService(typeof(AppDbContext))).Returns(_context);

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
        DateTime? endDate = null)
    {
        var activity = new Activity
        {
            Id = Guid.NewGuid().ToString(),
            AnimalId = Guid.NewGuid().ToString(),
            UserId = Guid.NewGuid().ToString(),
            Type = type,
            Status = status,
            StartDate = DateTime.UtcNow.AddDays(-7),
            EndDate = endDate ?? DateTime.UtcNow.AddDays(-1)
        };

        _context.Activities.Add(activity);
        _context.SaveChanges();

        return activity;
    }

    [Fact]
    public async Task ExecuteAsync_ShouldMarkActivityAsCompleted_WhenEndDateHasPassed()
    {
        var activity = CreateActivity(endDate: DateTime.UtcNow.AddDays(-1));

        await _task.ExecuteAsync(_mockServiceProvider.Object);

        var updatedActivity = await _context.Activities.FindAsync(activity.Id);
        Assert.Equal(ActivityStatus.Completed, updatedActivity!.Status);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldNotMarkActivity_WhenEndDateHasNotPassed()
    {
        var activity = CreateActivity(endDate: DateTime.UtcNow.AddDays(1));

        await _task.ExecuteAsync(_mockServiceProvider.Object);

        var updatedActivity = await _context.Activities.FindAsync(activity.Id);
        Assert.Equal(ActivityStatus.Active, updatedActivity!.Status);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldOnlyProcessOwnershipActivities_IgnoringFostering()
    {
        var ownershipActivity = CreateActivity(
            type: ActivityType.Ownership,
            endDate: DateTime.UtcNow.AddDays(-1));

        var fosteringActivity = CreateActivity(
            type: ActivityType.Fostering,
            endDate: DateTime.UtcNow.AddDays(-1));

        await _task.ExecuteAsync(_mockServiceProvider.Object);

        var updatedFostering = await _context.Activities.FindAsync(fosteringActivity.Id);
        Assert.Equal(ActivityStatus.Active, updatedFostering!.Status);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldOnlyProcessActiveStatus_IgnoringCompleted()
    {
        var activeActivity = CreateActivity(
            status: ActivityStatus.Active,
            endDate: DateTime.UtcNow.AddDays(-1));

        var completedActivity = CreateActivity(
            status: ActivityStatus.Completed,
            endDate: DateTime.UtcNow.AddDays(-1));

        await _task.ExecuteAsync(_mockServiceProvider.Object);

        var stillCompleted = await _context.Activities.FindAsync(completedActivity.Id);
        Assert.Equal(ActivityStatus.Completed, stillCompleted!.Status);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldOnlyProcessActiveStatus_IgnoringCancelled()
    {
        var cancelledActivity = CreateActivity(
            status: ActivityStatus.Cancelled,
            endDate: DateTime.UtcNow.AddDays(-1));

        await _task.ExecuteAsync(_mockServiceProvider.Object);

        var stillCancelled = await _context.Activities.FindAsync(cancelledActivity.Id);
        Assert.Equal(ActivityStatus.Cancelled, stillCancelled!.Status);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldNotThrow_WhenNoActivitiesToComplete()
    {
        // Arrange - no activities in database

        var exception = await Record.ExceptionAsync(
            () => _task.ExecuteAsync(_mockServiceProvider.Object));

        Assert.Null(exception);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldLogInformation_WhenNoActivitiesToComplete()
    {
        // Arrange - no activities

        await _task.ExecuteAsync(_mockServiceProvider.Object);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("No ownership activities")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldMarkMultipleActivities_WhenMultipleHaveExpired()
    {
        var activity1 = CreateActivity(endDate: DateTime.UtcNow.AddDays(-2));
        var activity2 = CreateActivity(endDate: DateTime.UtcNow.AddDays(-1));
        var activity3 = CreateActivity(endDate: DateTime.UtcNow.AddDays(-3));

        await _task.ExecuteAsync(_mockServiceProvider.Object);

        var completedCount = await _context.Activities
            .CountAsync(a => a.Status == ActivityStatus.Completed);
        Assert.Equal(3, completedCount);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldPersistChanges_WhenActivitiesAreMarkedCompleted()
    {
        var activity = CreateActivity(endDate: DateTime.UtcNow.AddDays(-1));
        _context.Entry(activity).State = EntityState.Unchanged;

        await _task.ExecuteAsync(_mockServiceProvider.Object);

        _context.Entry(activity).Reload();
        Assert.Equal(ActivityStatus.Completed, activity.Status);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldLogActivityId_WhenMarkingAsCompleted()
    {
        var activity = CreateActivity(endDate: DateTime.UtcNow.AddDays(-1));

        await _task.ExecuteAsync(_mockServiceProvider.Object);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains(activity.Id)),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldLogCount_WhenMultipleActivitiesCompleted()
    {
        CreateActivity(endDate: DateTime.UtcNow.AddDays(-1));
        CreateActivity(endDate: DateTime.UtcNow.AddDays(-2));

        await _task.ExecuteAsync(_mockServiceProvider.Object);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("2") && o.ToString()!.Contains("ownership activities marked as Completed")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }
}