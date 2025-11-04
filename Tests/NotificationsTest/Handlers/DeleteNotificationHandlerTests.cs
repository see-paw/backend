using Application.Interfaces;
using Application.Notifications.Commands;
using Domain;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Moq;
using Persistence;
using Xunit;

namespace Tests.Notifications;

public class DeleteNotificationHandlerTests
{
    private readonly AppDbContext _context;
    private readonly Mock<IUserAccessor> _mockUserAccessor;

    public DeleteNotificationHandlerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _mockUserAccessor = new Mock<IUserAccessor>();
    }

    private async Task<Notification> SeedNotificationAsync(string userId)
    {
        var notification = new Notification
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            Type = NotificationType.NEW_OWNERSHIP_REQUEST,
            Message = "Test notification",
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();
        return notification;
    }

    [Fact]
    public async Task DeleteNotification_ShouldReturnNotFound_WhenNotificationDoesNotExist()
    {
        var userId = Guid.NewGuid().ToString();
        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(userId);
        var handler = new DeleteNotification.Handler(_context, _mockUserAccessor.Object);

        var result = await handler.Handle(
            new DeleteNotification.Command { NotificationId = Guid.NewGuid().ToString() },
            default);

        Assert.Equal(404, result.Code);
    }

    [Fact]
    public async Task DeleteNotification_ShouldReturnNotFound_WhenNotificationBelongsToAnotherUser()
    {
        var userId1 = Guid.NewGuid().ToString();
        var userId2 = Guid.NewGuid().ToString();
        var notification = await SeedNotificationAsync(userId1);
        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(userId2);
        var handler = new DeleteNotification.Handler(_context, _mockUserAccessor.Object);

        var result = await handler.Handle(
            new DeleteNotification.Command { NotificationId = notification.Id },
            default);

        Assert.Equal(404, result.Code);
    }

    [Fact]
    public async Task DeleteNotification_ShouldReturnSuccess_WhenNotificationIsDeleted()
    {
        var userId = Guid.NewGuid().ToString();
        var notification = await SeedNotificationAsync(userId);
        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(userId);
        var handler = new DeleteNotification.Handler(_context, _mockUserAccessor.Object);

        var result = await handler.Handle(
            new DeleteNotification.Command { NotificationId = notification.Id },
            default);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task DeleteNotification_ShouldRemoveNotificationFromDatabase()
    {
        var userId = Guid.NewGuid().ToString();
        var notification = await SeedNotificationAsync(userId);
        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(userId);
        var handler = new DeleteNotification.Handler(_context, _mockUserAccessor.Object);

        await handler.Handle(
            new DeleteNotification.Command { NotificationId = notification.Id },
            default);

        var deletedNotification = await _context.Notifications.FindAsync(notification.Id);
        Assert.Null(deletedNotification);
    }

    [Fact]
    public async Task DeleteNotification_ShouldReturn200StatusCode()
    {
        var userId = Guid.NewGuid().ToString();
        var notification = await SeedNotificationAsync(userId);
        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(userId);
        var handler = new DeleteNotification.Handler(_context, _mockUserAccessor.Object);

        var result = await handler.Handle(
            new DeleteNotification.Command { NotificationId = notification.Id },
            default);

        Assert.Equal(200, result.Code);
    }

    [Fact]
    public async Task DeleteNotification_ShouldNotDeleteNotificationsFromOtherUsers()
    {
        var userId1 = Guid.NewGuid().ToString();
        var userId2 = Guid.NewGuid().ToString();
        var notification1 = await SeedNotificationAsync(userId1);
        var notification2 = await SeedNotificationAsync(userId2);
        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(userId1);
        var handler = new DeleteNotification.Handler(_context, _mockUserAccessor.Object);

        await handler.Handle(
            new DeleteNotification.Command { NotificationId = notification1.Id },
            default);

        var remainingNotification = await _context.Notifications.FindAsync(notification2.Id);
        Assert.NotNull(remainingNotification);
    }

    [Fact]
    public async Task DeleteNotification_ShouldDecrementNotificationCount()
    {
        var userId = Guid.NewGuid().ToString();
        await SeedNotificationAsync(userId);
        var notificationToDelete = await SeedNotificationAsync(userId);
        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(userId);
        var handler = new DeleteNotification.Handler(_context, _mockUserAccessor.Object);

        await handler.Handle(
            new DeleteNotification.Command { NotificationId = notificationToDelete.Id },
            default);

        var count = await _context.Notifications.CountAsync(n => n.UserId == userId);
        Assert.Equal(1, count);
    }
}