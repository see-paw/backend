using Application.Interfaces;
using Domain;
using Domain.Common;
using Domain.Enums;
using Infrastructure.Hubs;
using Infrastructure.Notifications;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Persistence;
using Xunit;

namespace Tests.NotificationsTest;

/// <summary>
/// Unit tests for NotificationService.
/// Validates notification creation, persistence, and SignalR delivery.
/// </summary>
public class NotificationServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<UserManager<User>> _mockUserManager;
    private readonly Mock<IHubContext<NotificationHub>> _mockHubContext;
    private readonly Mock<ILogger<NotificationService>> _mockLogger;
    private readonly Mock<IHubClients> _mockHubClients;
    private readonly Mock<IClientProxy> _mockClientProxy;
    private readonly NotificationService _service;

    public NotificationServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);

        var userStore = new Mock<IUserStore<User>>();
        _mockUserManager = new Mock<UserManager<User>>(
            userStore.Object, null, null, null, null, null, null, null, null);

        _mockHubContext = new Mock<IHubContext<NotificationHub>>();
        _mockLogger = new Mock<ILogger<NotificationService>>();
        _mockHubClients = new Mock<IHubClients>();
        _mockClientProxy = new Mock<IClientProxy>();

        _mockHubContext.Setup(h => h.Clients).Returns(_mockHubClients.Object);
        _mockHubClients.Setup(c => c.User(It.IsAny<string>())).Returns(_mockClientProxy.Object);
        _mockHubClients.Setup(c => c.Users(It.IsAny<IReadOnlyList<string>>())).Returns(_mockClientProxy.Object);

        _service = new NotificationService(
            _mockUserManager.Object,
            _mockHubContext.Object,
            _context,
            _mockLogger.Object);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task CreateAndSendToUserAsync_ShouldCreateNotificationInDatabase()
    {
        var userId = "user-123";
        var type = NotificationType.NEW_ANIMAL_ADDED;
        var message = "New animal available";

        await _service.CreateAndSendToUserAsync(userId, type, message);

        var notification = await _context.Notifications.FirstOrDefaultAsync(n => n.UserId == userId);
        Assert.NotNull(notification);
    }

    [Fact]
    public async Task CreateAndSendToUserAsync_ShouldSetCorrectUserId()
    {
        var userId = "user-456";
        var type = NotificationType.NEW_ANIMAL_ADDED;
        var message = "Test message";

        await _service.CreateAndSendToUserAsync(userId, type, message);

        var notification = await _context.Notifications.FirstAsync();
        Assert.Equal(userId, notification.UserId);
    }

    [Fact]
    public async Task CreateAndSendToUserAsync_ShouldSetCorrectNotificationType()
    {
        var userId = "user-789";
        var type = NotificationType.OWNERSHIP_REQUEST_APPROVED;
        var message = "Request approved";

        await _service.CreateAndSendToUserAsync(userId, type, message);

        var notification = await _context.Notifications.FirstAsync();
        Assert.Equal(type, notification.Type);
    }

    [Fact]
    public async Task CreateAndSendToUserAsync_ShouldSetCorrectMessage()
    {
        var userId = "user-999";
        var type = NotificationType.NEW_ANIMAL_ADDED;
        var message = "Specific test message";

        await _service.CreateAndSendToUserAsync(userId, type, message);

        var notification = await _context.Notifications.FirstAsync();
        Assert.Equal(message, notification.Message);
    }

    [Fact]
    public async Task CreateAndSendToUserAsync_ShouldSetIsReadToFalse()
    {
        var userId = "user-111";
        var type = NotificationType.NEW_ANIMAL_ADDED;
        var message = "Message";

        await _service.CreateAndSendToUserAsync(userId, type, message);

        var notification = await _context.Notifications.FirstAsync();
        Assert.False(notification.IsRead);
    }

    [Fact]
    public async Task CreateAndSendToUserAsync_ShouldSetIsBroadcastToFalse()
    {
        var userId = "user-222";
        var type = NotificationType.NEW_ANIMAL_ADDED;
        var message = "Message";

        await _service.CreateAndSendToUserAsync(userId, type, message);

        var notification = await _context.Notifications.FirstAsync();
        Assert.False(notification.IsBroadcast);
    }

    [Fact]
    public async Task CreateAndSendToUserAsync_ShouldSetAnimalId_WhenProvided()
    {
        var userId = "user-333";
        var type = NotificationType.NEW_ANIMAL_ADDED;
        var message = "Message";
        var animalId = "animal-123";

        await _service.CreateAndSendToUserAsync(userId, type, message, animalId);

        var notification = await _context.Notifications.FirstAsync();
        Assert.Equal(animalId, notification.AnimalId);
    }

    [Fact]
    public async Task CreateAndSendToUserAsync_ShouldSetOwnershipRequestId_WhenProvided()
    {
        var userId = "user-444";
        var type = NotificationType.OWNERSHIP_REQUEST_APPROVED;
        var message = "Message";
        var ownershipRequestId = "request-456";

        await _service.CreateAndSendToUserAsync(userId, type, message, null, ownershipRequestId);

        var notification = await _context.Notifications.FirstAsync();
        Assert.Equal(ownershipRequestId, notification.OwnershipRequestId);
    }

    [Fact]
    public async Task CreateAndSendToUserAsync_ShouldSetActivityId_WhenProvided()
    {
        var userId = "user-555";
        var type = NotificationType.FOSTERING_ACTIVITY_START_REMINDER_USER;
        var message = "Message";
        var activityId = "activity-789";

        await _service.CreateAndSendToUserAsync(userId, type, message, null, null, activityId);

        var notification = await _context.Notifications.FirstAsync();
        Assert.Equal(activityId, notification.ActivityId);
    }

    [Fact]
    public async Task CreateAndSendToUserAsync_ShouldReturnCreatedNotification()
    {
        var userId = "user-666";
        var type = NotificationType.NEW_ANIMAL_ADDED;
        var message = "Message";

        var result = await _service.CreateAndSendToUserAsync(userId, type, message);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task CreateAndSendToUserAsync_ShouldReturnNotificationWithId()
    {
        var userId = "user-777";
        var type = NotificationType.NEW_ANIMAL_ADDED;
        var message = "Message";

        var result = await _service.CreateAndSendToUserAsync(userId, type, message);

        Assert.NotNull(result.Id);
        Assert.NotEmpty(result.Id);
    }

    [Fact]
    public async Task CreateAndSendToUserAsync_ShouldSendViaSignalR()
    {
        var userId = "user-888";
        var type = NotificationType.NEW_ANIMAL_ADDED;
        var message = "Message";

        await _service.CreateAndSendToUserAsync(userId, type, message);

        _mockClientProxy.Verify(
            c => c.SendCoreAsync(
                "ReceiveNotification",
                It.IsAny<object[]>(),
                default),
            Times.Once);
    }

    [Fact]
    public async Task CreateAndSendToRoleAsync_ShouldCreateNotificationForEachUser()
    {
        var role = AppRoles.User;
        var users = new List<User>
        {
            new User { Id = "user-1", UserName = "user1" },
            new User { Id = "user-2", UserName = "user2" }
        };
        _mockUserManager.Setup(um => um.GetUsersInRoleAsync(role))
            .ReturnsAsync(users);

        await _service.CreateAndSendToRoleAsync(role, NotificationType.NEW_ANIMAL_ADDED, "Message");

        var notificationCount = await _context.Notifications.CountAsync();
        Assert.Equal(2, notificationCount);
    }

    [Fact]
    public async Task CreateAndSendToRoleAsync_ShouldSetIsBroadcastToTrue()
    {
        var role = AppRoles.AdminCAA;
        var users = new List<User>
        {
            new User { Id = "admin-1", UserName = "admin1" }
        };
        _mockUserManager.Setup(um => um.GetUsersInRoleAsync(role))
            .ReturnsAsync(users);

        await _service.CreateAndSendToRoleAsync(role, NotificationType.NEW_OWNERSHIP_REQUEST, "Message");

        var notification = await _context.Notifications.FirstAsync();
        Assert.True(notification.IsBroadcast);
    }

    [Fact]
    public async Task CreateAndSendToRoleAsync_ShouldSetTargetRole()
    {
        var role = AppRoles.User;
        var users = new List<User>
        {
            new User { Id = "user-1", UserName = "user1" }
        };
        _mockUserManager.Setup(um => um.GetUsersInRoleAsync(role))
            .ReturnsAsync(users);

        await _service.CreateAndSendToRoleAsync(role, NotificationType.NEW_ANIMAL_ADDED, "Message");

        var notification = await _context.Notifications.FirstAsync();
        Assert.Equal(role, notification.TargetRole);
    }

    [Fact]
    public async Task CreateAndSendToRoleAsync_ShouldReturnListOfNotifications()
    {
        var role = AppRoles.User;
        var users = new List<User>
        {
            new User { Id = "user-1", UserName = "user1" },
            new User { Id = "user-2", UserName = "user2" }
        };
        _mockUserManager.Setup(um => um.GetUsersInRoleAsync(role))
            .ReturnsAsync(users);

        var result = await _service.CreateAndSendToRoleAsync(role, NotificationType.NEW_ANIMAL_ADDED, "Message");

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task CreateAndSendToRoleAsync_ShouldReturnEmptyList_WhenNoUsersInRole()
    {
        var role = "NonExistentRole";
        _mockUserManager.Setup(um => um.GetUsersInRoleAsync(role))
            .ReturnsAsync(new List<User>());

        var result = await _service.CreateAndSendToRoleAsync(role, NotificationType.NEW_ANIMAL_ADDED, "Message");

        Assert.Empty(result);
    }

    [Fact]
    public async Task CreateAndSendToRoleAsync_ShouldSendViaSignalR_WhenUsersExist()
    {
        var role = AppRoles.User;
        var users = new List<User>
        {
            new User { Id = "user-1", UserName = "user1" }
        };
        _mockUserManager.Setup(um => um.GetUsersInRoleAsync(role))
            .ReturnsAsync(users);

        await _service.CreateAndSendToRoleAsync(role, NotificationType.NEW_ANIMAL_ADDED, "Message");

        _mockClientProxy.Verify(
            c => c.SendCoreAsync(
                "ReceiveNotification",
                It.IsAny<object[]>(),
                default),
            Times.Once);
    }

    [Fact]
    public async Task CreateAndSendToRoleAsync_ShouldNotSendViaSignalR_WhenNoUsers()
    {
        var role = "EmptyRole";
        _mockUserManager.Setup(um => um.GetUsersInRoleAsync(role))
            .ReturnsAsync(new List<User>());

        await _service.CreateAndSendToRoleAsync(role, NotificationType.NEW_ANIMAL_ADDED, "Message");

        _mockClientProxy.Verify(
            c => c.SendCoreAsync(
                "ReceiveNotification",
                It.IsAny<object[]>(),
                default),
            Times.Never);
    }

    [Fact]
    public async Task SendToUserAsync_ShouldSendToCorrectUser()
    {
        var userId = "user-123";
        var notification = new Notification
        {
            Id = "notif-1",
            UserId = userId,
            Type = NotificationType.NEW_ANIMAL_ADDED,
            Message = "Test"
        };

        await _service.SendToUserAsync(userId, notification);

        _mockHubClients.Verify(c => c.User(userId), Times.Once);
    }

    [Fact]
    public async Task SendToUserAsync_ShouldNotThrow_WhenSignalRFails()
    {
        var userId = "user-error";
        var notification = new Notification
        {
            Id = "notif-1",
            UserId = userId,
            Type = NotificationType.NEW_ANIMAL_ADDED,
            Message = "Test"
        };
        _mockClientProxy.Setup(c => c.SendCoreAsync(
                It.IsAny<string>(),
                It.IsAny<object[]>(),
                default))
            .ThrowsAsync(new Exception("SignalR error"));

        var exception = await Record.ExceptionAsync(
            () => _service.SendToUserAsync(userId, notification));

        Assert.Null(exception);
    }

    [Fact]
    public async Task SendToUserAsync_ShouldLogError_WhenSignalRFails()
    {
        var userId = "user-error-log";
        var notification = new Notification
        {
            Id = "notif-1",
            UserId = userId,
            Type = NotificationType.NEW_ANIMAL_ADDED,
            Message = "Test"
        };
        _mockClientProxy.Setup(c => c.SendCoreAsync(
                It.IsAny<string>(),
                It.IsAny<object[]>(),
                default))
            .ThrowsAsync(new Exception("SignalR error"));

        await _service.SendToUserAsync(userId, notification);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains(userId)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendToRoleAsync_ShouldSendToAllUsersInRole()
    {
        var role = AppRoles.User;
        var users = new List<User>
        {
            new User { Id = "user-1", UserName = "user1" },
            new User { Id = "user-2", UserName = "user2" }
        };
        _mockUserManager.Setup(um => um.GetUsersInRoleAsync(role))
            .ReturnsAsync(users);

        var notification = new Notification
        {
            Id = "notif-1",
            Type = NotificationType.NEW_ANIMAL_ADDED,
            Message = "Test"
        };

        await _service.SendToRoleAsync(role, notification);

        _mockHubClients.Verify(
            c => c.Users(It.Is<IReadOnlyList<string>>(list => list.Count == 2)),
            Times.Once);
    }

    [Fact]
    public async Task SendToRoleAsync_ShouldNotSend_WhenNoUsersInRole()
    {
        var role = "EmptyRole";
        _mockUserManager.Setup(um => um.GetUsersInRoleAsync(role))
            .ReturnsAsync(new List<User>());

        var notification = new Notification
        {
            Id = "notif-1",
            Type = NotificationType.NEW_ANIMAL_ADDED,
            Message = "Test"
        };

        await _service.SendToRoleAsync(role, notification);

        _mockClientProxy.Verify(
            c => c.SendCoreAsync(
                It.IsAny<string>(),
                It.IsAny<object[]>(),
                default),
            Times.Never);
    }

    [Fact]
    public async Task SendToRoleAsync_ShouldNotThrow_WhenSignalRFails()
    {
        var role = AppRoles.User;
        var users = new List<User>
        {
            new User { Id = "user-1", UserName = "user1" }
        };
        _mockUserManager.Setup(um => um.GetUsersInRoleAsync(role))
            .ReturnsAsync(users);
        _mockClientProxy.Setup(c => c.SendCoreAsync(
                It.IsAny<string>(),
                It.IsAny<object[]>(),
                default))
            .ThrowsAsync(new Exception("SignalR error"));

        var notification = new Notification
        {
            Id = "notif-1",
            Type = NotificationType.NEW_ANIMAL_ADDED,
            Message = "Test"
        };

        var exception = await Record.ExceptionAsync(
            () => _service.SendToRoleAsync(role, notification));

        Assert.Null(exception);
    }

    [Fact]
    public async Task SendToRoleAsync_ShouldLogError_WhenSignalRFails()
    {
        var role = AppRoles.AdminCAA;
        var users = new List<User>
        {
            new User { Id = "admin-1", UserName = "admin1" }
        };
        _mockUserManager.Setup(um => um.GetUsersInRoleAsync(role))
            .ReturnsAsync(users);
        _mockClientProxy.Setup(c => c.SendCoreAsync(
                It.IsAny<string>(),
                It.IsAny<object[]>(),
                default))
            .ThrowsAsync(new Exception("SignalR error"));

        var notification = new Notification
        {
            Id = "notif-1",
            Type = NotificationType.NEW_ANIMAL_ADDED,
            Message = "Test"
        };

        await _service.SendToRoleAsync(role, notification);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains(role)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}