using Infrastructure.BackgroundTasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Tests.BackgroundTasksTest;

/// <summary>
/// Unit tests for ReminderService.
/// 
/// These tests validate that the background service:
/// - Starts and stops correctly
/// - Executes all registered tasks in each cycle
/// - Handles task exceptions gracefully without stopping
/// - Logs appropriate information and errors
/// - Respects the cancellation token
/// - Creates proper service scopes for dependency injection
/// </summary>
public class ReminderServiceTests
{
    private readonly Mock<IServiceScopeFactory> _mockScopeFactory;
    private readonly Mock<ILogger<ReminderService>> _mockLogger;
    private readonly Mock<IServiceScope> _mockScope;
    private readonly Mock<IServiceProvider> _mockServiceProvider;

    public ReminderServiceTests()
    {
        _mockScopeFactory = new Mock<IServiceScopeFactory>();
        _mockLogger = new Mock<ILogger<ReminderService>>();
        _mockScope = new Mock<IServiceScope>();
        _mockServiceProvider = new Mock<IServiceProvider>();

        _mockScope.Setup(s => s.ServiceProvider).Returns(_mockServiceProvider.Object);
        _mockScopeFactory.Setup(f => f.CreateScope()).Returns(_mockScope.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldLogStartMessage_WhenServiceStarts()
    {
        var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel immediately to stop the loop

        _mockServiceProvider
            .Setup(sp => sp.GetService(typeof(IEnumerable<IReminderTask>)))
            .Returns(new List<IReminderTask>());

        var service = new ReminderService(_mockScopeFactory.Object, _mockLogger.Object);

        await service.StartAsync(cts.Token);
        await Task.Delay(100); // Give it time to process
        await service.StopAsync(CancellationToken.None);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("ReminderService started")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldExecuteAllRegisteredTasks_InEachCycle()
    {
        var cts = new CancellationTokenSource();
        var task1 = new Mock<IReminderTask>();
        var task2 = new Mock<IReminderTask>();
        var task3 = new Mock<IReminderTask>();

        var tasks = new List<IReminderTask> { task1.Object, task2.Object, task3.Object };

        _mockServiceProvider
            .Setup(sp => sp.GetService(typeof(IEnumerable<IReminderTask>)))
            .Returns(tasks);

        var service = new ReminderService(_mockScopeFactory.Object, _mockLogger.Object);

        await service.StartAsync(CancellationToken.None);
        await Task.Delay(100); // Let it run one cycle
        cts.Cancel();
        await service.StopAsync(cts.Token);

        task1.Verify(t => t.ExecuteAsync(It.IsAny<IServiceProvider>()), Times.AtLeastOnce);
        task2.Verify(t => t.ExecuteAsync(It.IsAny<IServiceProvider>()), Times.AtLeastOnce);
        task3.Verify(t => t.ExecuteAsync(It.IsAny<IServiceProvider>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldLogTaskName_WhenExecutingEachTask()
    {
        var cts = new CancellationTokenSource();
        var task = new TestReminderTask();

        _mockServiceProvider
            .Setup(sp => sp.GetService(typeof(IEnumerable<IReminderTask>)))
            .Returns(new List<IReminderTask> { task });

        var service = new ReminderService(_mockScopeFactory.Object, _mockLogger.Object);

        await service.StartAsync(CancellationToken.None);
        await Task.Delay(100);
        cts.Cancel();
        await service.StopAsync(cts.Token);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Executing") && o.ToString()!.Contains("TestReminderTask")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldContinueExecution_WhenTaskThrowsException()
    {
        var cts = new CancellationTokenSource();
        var failingTask = new Mock<IReminderTask>();
        var successTask = new Mock<IReminderTask>();

        failingTask
            .Setup(t => t.ExecuteAsync(It.IsAny<IServiceProvider>()))
            .ThrowsAsync(new Exception("Task failed"));

        _mockServiceProvider
            .Setup(sp => sp.GetService(typeof(IEnumerable<IReminderTask>)))
            .Returns(new List<IReminderTask> { failingTask.Object, successTask.Object });

        var service = new ReminderService(_mockScopeFactory.Object, _mockLogger.Object);

        await service.StartAsync(CancellationToken.None);
        await Task.Delay(100);
        cts.Cancel();
        await service.StopAsync(cts.Token);

        // The success task should still execute despite the failing task
        successTask.Verify(t => t.ExecuteAsync(It.IsAny<IServiceProvider>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldLogError_WhenTaskThrowsException()
    {
        var cts = new CancellationTokenSource();
        var task = new Mock<IReminderTask>();
        var exception = new Exception("Task execution failed");

        task.Setup(t => t.ExecuteAsync(It.IsAny<IServiceProvider>()))
            .ThrowsAsync(exception);

        _mockServiceProvider
            .Setup(sp => sp.GetService(typeof(IEnumerable<IReminderTask>)))
            .Returns(new List<IReminderTask> { task.Object });

        var service = new ReminderService(_mockScopeFactory.Object, _mockLogger.Object);

        await service.StartAsync(CancellationToken.None);
        await Task.Delay(100);
        cts.Cancel();
        await service.StopAsync(cts.Token);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Error executing")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCreateNewScope_ForEachCycle()
    {
        var cts = new CancellationTokenSource();

        _mockServiceProvider
            .Setup(sp => sp.GetService(typeof(IEnumerable<IReminderTask>)))
            .Returns(new List<IReminderTask>());

        var service = new ReminderService(_mockScopeFactory.Object, _mockLogger.Object);

        await service.StartAsync(CancellationToken.None);
        await Task.Delay(100);
        cts.Cancel();
        await service.StopAsync(cts.Token);

        _mockScopeFactory.Verify(f => f.CreateScope(), Times.AtLeastOnce);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldDisposeScope_AfterEachCycle()
    {
        var cts = new CancellationTokenSource();

        _mockServiceProvider
            .Setup(sp => sp.GetService(typeof(IEnumerable<IReminderTask>)))
            .Returns(new List<IReminderTask>());

        var service = new ReminderService(_mockScopeFactory.Object, _mockLogger.Object);

        await service.StartAsync(CancellationToken.None);
        await Task.Delay(100);
        cts.Cancel();
        await service.StopAsync(cts.Token);

        _mockScope.As<IDisposable>().Verify(d => d.Dispose(), Times.AtLeastOnce);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldPassServiceProvider_ToTasks()
    {
        var cts = new CancellationTokenSource();
        var task = new Mock<IReminderTask>();
        IServiceProvider? capturedProvider = null;

        task.Setup(t => t.ExecuteAsync(It.IsAny<IServiceProvider>()))
            .Callback<IServiceProvider>(sp => capturedProvider = sp)
            .Returns(Task.CompletedTask);

        _mockServiceProvider
            .Setup(sp => sp.GetService(typeof(IEnumerable<IReminderTask>)))
            .Returns(new List<IReminderTask> { task.Object });

        var service = new ReminderService(_mockScopeFactory.Object, _mockLogger.Object);

        await service.StartAsync(CancellationToken.None);
        await Task.Delay(100);
        cts.Cancel();
        await service.StopAsync(cts.Token);

        Assert.NotNull(capturedProvider);
        Assert.Equal(_mockServiceProvider.Object, capturedProvider);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldNotExecuteTasks_WhenNoTasksRegistered()
    {
        var cts = new CancellationTokenSource();

        _mockServiceProvider
            .Setup(sp => sp.GetService(typeof(IEnumerable<IReminderTask>)))
            .Returns(new List<IReminderTask>());

        var service = new ReminderService(_mockScopeFactory.Object, _mockLogger.Object);

        await service.StartAsync(CancellationToken.None);
        await Task.Delay(100);
        cts.Cancel();
        await service.StopAsync(cts.Token);

        // Should still log start/stop but not execute any tasks
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("ReminderService started")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldStopImmediately_WhenCancellationRequested()
    {
        var cts = new CancellationTokenSource();
        var task = new Mock<IReminderTask>();
        int executionCount = 0;

        task.Setup(t => t.ExecuteAsync(It.IsAny<IServiceProvider>()))
            .Callback(() => executionCount++)
            .Returns(Task.CompletedTask);

        _mockServiceProvider
            .Setup(sp => sp.GetService(typeof(IEnumerable<IReminderTask>)))
            .Returns(new List<IReminderTask> { task.Object });

        var service = new ReminderService(_mockScopeFactory.Object, _mockLogger.Object);

        await service.StartAsync(CancellationToken.None);
        await Task.Delay(50); // Very short delay
        cts.Cancel();
        await service.StopAsync(cts.Token);

        // Should execute at most once or twice, not multiple cycles
        Assert.InRange(executionCount, 0, 2);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldHandleMultipleTasks_WithMixedSuccessAndFailure()
    {
        var cts = new CancellationTokenSource();
        var successTask1 = new Mock<IReminderTask>();
        var failingTask = new Mock<IReminderTask>();
        var successTask2 = new Mock<IReminderTask>();

        failingTask
            .Setup(t => t.ExecuteAsync(It.IsAny<IServiceProvider>()))
            .ThrowsAsync(new InvalidOperationException("Simulated failure"));

        _mockServiceProvider
            .Setup(sp => sp.GetService(typeof(IEnumerable<IReminderTask>)))
            .Returns(new List<IReminderTask>
            {
                successTask1.Object,
                failingTask.Object,
                successTask2.Object
            });

        var service = new ReminderService(_mockScopeFactory.Object, _mockLogger.Object);

        await service.StartAsync(CancellationToken.None);
        await Task.Delay(100);
        cts.Cancel();
        await service.StopAsync(cts.Token);

        // Both success tasks should execute despite the failure
        successTask1.Verify(t => t.ExecuteAsync(It.IsAny<IServiceProvider>()), Times.AtLeastOnce);
        successTask2.Verify(t => t.ExecuteAsync(It.IsAny<IServiceProvider>()), Times.AtLeastOnce);
        failingTask.Verify(t => t.ExecuteAsync(It.IsAny<IServiceProvider>()), Times.AtLeastOnce);
    }

    /// <summary>
    /// Test helper class to simulate a reminder task.
    /// </summary>
    private class TestReminderTask : IReminderTask
    {
        public Task ExecuteAsync(IServiceProvider services)
        {
            return Task.CompletedTask;
        }
    }
}
