using Infrastructure.Hubs;

using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

using Moq;

namespace Tests.NotificationsTests;

/// <summary>
/// Unit tests for NotificationHub.
/// Validates connection/disconnection logging behavior.
/// </summary>
public class NotificationHubTests
{
    private readonly Mock<ILogger<NotificationHub>> _mockLogger;
    private readonly Mock<HubCallerContext> _mockContext;
    private readonly NotificationHub _hub;

    public NotificationHubTests()
    {
        _mockLogger = new Mock<ILogger<NotificationHub>>();
        _mockContext = new Mock<HubCallerContext>();
        _hub = new NotificationHub(_mockLogger.Object)
        {
            Context = _mockContext.Object
        };
    }

    [Fact]
    public async Task OnConnectedAsync_ShouldLogUserConnection()
    {
        var userId = "user-123";
        _mockContext.Setup(c => c.UserIdentifier).Returns(userId);

        await _hub.OnConnectedAsync();

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains(userId) && o.ToString()!.Contains("connected")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task OnConnectedAsync_ShouldLogCorrectMessage_WhenUserConnects()
    {
        var userId = "test-user-456";
        _mockContext.Setup(c => c.UserIdentifier).Returns(userId);

        await _hub.OnConnectedAsync();

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("NotificationHub")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task OnDisconnectedAsync_ShouldLogUserDisconnection()
    {
        var userId = "user-789";
        _mockContext.Setup(c => c.UserIdentifier).Returns(userId);

        await _hub.OnDisconnectedAsync(null);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains(userId) && o.ToString()!.Contains("disconnected")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task OnDisconnectedAsync_ShouldLogCorrectMessage_WhenUserDisconnects()
    {
        var userId = "another-user-999";
        _mockContext.Setup(c => c.UserIdentifier).Returns(userId);

        await _hub.OnDisconnectedAsync(null);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("NotificationHub")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task OnDisconnectedAsync_ShouldHandleException_WhenProvided()
    {
        var userId = "user-with-error";
        var exception = new Exception("Connection lost");
        _mockContext.Setup(c => c.UserIdentifier).Returns(userId);

        var exceptionThrown = await Record.ExceptionAsync(
            () => _hub.OnDisconnectedAsync(exception));

        Assert.Null(exceptionThrown);
    }

    [Fact]
    public async Task OnDisconnectedAsync_ShouldLogDisconnection_WhenExceptionIsNull()
    {
        var userId = "user-clean-disconnect";
        _mockContext.Setup(c => c.UserIdentifier).Returns(userId);

        await _hub.OnDisconnectedAsync(null);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task OnConnectedAsync_ShouldNotThrow_WhenUserIdentifierIsNull()
    {
        _mockContext.Setup(c => c.UserIdentifier).Returns((string?)null);

        var exception = await Record.ExceptionAsync(
            () => _hub.OnConnectedAsync());

        Assert.Null(exception);
    }

    [Fact]
    public async Task OnDisconnectedAsync_ShouldNotThrow_WhenUserIdentifierIsNull()
    {
        _mockContext.Setup(c => c.UserIdentifier).Returns((string?)null);

        var exception = await Record.ExceptionAsync(
            () => _hub.OnDisconnectedAsync(null));

        Assert.Null(exception);
    }

    [Fact]
    public async Task OnConnectedAsync_ShouldLogWithInformationLevel()
    {
        var userId = "user-log-level-test";
        _mockContext.Setup(c => c.UserIdentifier).Returns(userId);

        await _hub.OnConnectedAsync();

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task OnDisconnectedAsync_ShouldLogWithInformationLevel()
    {
        var userId = "user-disconnect-log-level";
        _mockContext.Setup(c => c.UserIdentifier).Returns(userId);

        await _hub.OnDisconnectedAsync(null);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}