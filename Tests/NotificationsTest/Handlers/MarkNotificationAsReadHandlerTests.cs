using Application.Interfaces;
using Application.Notifications.Queries;
using Domain;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Moq;
using Persistence;

namespace Tests.NotificationsTest.Handlers;

public class MarkNotificationAsReadHandlerTests
{
    private readonly AppDbContext _context;
    private readonly Mock<IUserAccessor> _mockUserAccessor;

    public MarkNotificationAsReadHandlerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _mockUserAccessor = new Mock<IUserAccessor>();
    }

    private async Task<Notification> SeedNotificationAsync(string userId, bool isRead = false)
    {
        var notification = new Notification
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            Type = NotificationType.NEW_OWNERSHIP_REQUEST,
            Message = "Test notification",
            IsRead = isRead,
            CreatedAt = DateTime.UtcNow
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();
        return notification;
    }

    [Fact]
    public async Task MarkNotificationAsRead_ShouldReturnNotFound_WhenNotificationDoesNotExist()
    {
        var userId = Guid.NewGuid().ToString();
        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(userId);
        var handler = new MarkNotificationAsRead.Handler(_context, _mockUserAccessor.Object);

        var result = await handler.Handle(
            new MarkNotificationAsRead.Command { NotificationId = Guid.NewGuid().ToString() },
            default);

        Assert.Equal(404, result.Code);
    }

    [Fact]
    public async Task MarkNotificationAsRead_ShouldReturnNotFound_WhenNotificationBelongsToAnotherUser()
    {
        var userId1 = Guid.NewGuid().ToString();
        var userId2 = Guid.NewGuid().ToString();
        var notification = await SeedNotificationAsync(userId1);
        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(userId2);
        var handler = new MarkNotificationAsRead.Handler(_context, _mockUserAccessor.Object);

        var result = await handler.Handle(
            new MarkNotificationAsRead.Command { NotificationId = notification.Id },
            default);

        Assert.Equal(404, result.Code);
    }

    [Fact]
    public async Task MarkNotificationAsRead_ShouldReturnSuccess_WhenNotificationIsMarkedAsRead()
    {
        var userId = Guid.NewGuid().ToString();
        var notification = await SeedNotificationAsync(userId);
        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(userId);
        var handler = new MarkNotificationAsRead.Handler(_context, _mockUserAccessor.Object);

        var result = await handler.Handle(
            new MarkNotificationAsRead.Command { NotificationId = notification.Id },
            default);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task MarkNotificationAsRead_ShouldSetIsReadToTrue()
    {
        var userId = Guid.NewGuid().ToString();
        var notification = await SeedNotificationAsync(userId);
        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(userId);
        var handler = new MarkNotificationAsRead.Handler(_context, _mockUserAccessor.Object);

        await handler.Handle(
            new MarkNotificationAsRead.Command { NotificationId = notification.Id },
            default);

        var updatedNotification = await _context.Notifications.FindAsync(notification.Id);
        Assert.True(updatedNotification!.IsRead);
    }

    [Fact]
    public async Task MarkNotificationAsRead_ShouldSetReadAtTimestamp()
    {
        var userId = Guid.NewGuid().ToString();
        var notification = await SeedNotificationAsync(userId);
        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(userId);
        var handler = new MarkNotificationAsRead.Handler(_context, _mockUserAccessor.Object);

        await handler.Handle(
            new MarkNotificationAsRead.Command { NotificationId = notification.Id },
            default);

        var updatedNotification = await _context.Notifications.FindAsync(notification.Id);
        Assert.NotNull(updatedNotification!.ReadAt);
    }

    [Fact]
    public async Task MarkNotificationAsRead_ShouldReturnSuccess_WhenNotificationAlreadyRead()
    {
        var userId = Guid.NewGuid().ToString();
        var notification = await SeedNotificationAsync(userId, isRead: true);
        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(userId);
        var handler = new MarkNotificationAsRead.Handler(_context, _mockUserAccessor.Object);

        var result = await handler.Handle(
            new MarkNotificationAsRead.Command { NotificationId = notification.Id },
            default);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task MarkNotificationAsRead_ShouldReturn200StatusCode()
    {
        var userId = Guid.NewGuid().ToString();
        var notification = await SeedNotificationAsync(userId);
        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(userId);
        var handler = new MarkNotificationAsRead.Handler(_context, _mockUserAccessor.Object);

        var result = await handler.Handle(
            new MarkNotificationAsRead.Command { NotificationId = notification.Id },
            default);

        Assert.Equal(200, result.Code);
    }
}