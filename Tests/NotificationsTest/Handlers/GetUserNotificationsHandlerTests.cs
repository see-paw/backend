using Application.Interfaces;
using Application.Notifications.Queries;
using Domain;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Moq;
using Persistence;

namespace Tests.Notifications;

public class GetUserNotificationsHandlerTests
{
    private readonly AppDbContext _context;
    private readonly Mock<IUserAccessor> _mockUserAccessor;

    public GetUserNotificationsHandlerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _mockUserAccessor = new Mock<IUserAccessor>();
    }

    private async Task<List<Notification>> SeedNotificationsAsync(string userId, int totalCount, int unreadCount)
    {
        var notifications = new List<Notification>();

        for (int i = 0; i < totalCount; i++)
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                Type = NotificationType.NEW_OWNERSHIP_REQUEST,
                Message = $"Test notification {i}",
                IsRead = i >= unreadCount,
                CreatedAt = DateTime.UtcNow.AddMinutes(-i)
            };
            notifications.Add(notification);
            _context.Notifications.Add(notification);
        }

        await _context.SaveChangesAsync();
        return notifications;
    }

    [Fact]
    public async Task GetUserNotifications_ShouldReturnSuccess_WhenUnreadOnlyIsFalse()
    {
        var userId = Guid.NewGuid().ToString();
        await SeedNotificationsAsync(userId, 5, 2);
        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(userId);
        var handler = new GetUserNotifications.Handler(_context, _mockUserAccessor.Object);

        var result = await handler.Handle(new GetUserNotifications.Query { UnreadOnly = false }, default);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task GetUserNotifications_ShouldReturnAllNotifications_WhenUnreadOnlyIsFalse()
    {
        var userId = Guid.NewGuid().ToString();
        await SeedNotificationsAsync(userId, 5, 2);
        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(userId);
        var handler = new GetUserNotifications.Handler(_context, _mockUserAccessor.Object);

        var result = await handler.Handle(new GetUserNotifications.Query { UnreadOnly = false }, default);

        Assert.Equal(5, result.Value!.Count);
    }

    [Fact]
    public async Task GetUserNotifications_ShouldReturnSuccess_WhenUnreadOnlyIsTrue()
    {
        var userId = Guid.NewGuid().ToString();
        await SeedNotificationsAsync(userId, 5, 2);
        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(userId);
        var handler = new GetUserNotifications.Handler(_context, _mockUserAccessor.Object);

        var result = await handler.Handle(new GetUserNotifications.Query { UnreadOnly = true }, default);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task GetUserNotifications_ShouldReturnOnlyUnreadNotifications_WhenUnreadOnlyIsTrue()
    {
        var userId = Guid.NewGuid().ToString();
        await SeedNotificationsAsync(userId, 5, 2);
        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(userId);
        var handler = new GetUserNotifications.Handler(_context, _mockUserAccessor.Object);

        var result = await handler.Handle(new GetUserNotifications.Query { UnreadOnly = true }, default);

        Assert.Equal(2, result.Value!.Count);
    }

    [Fact]
    public async Task GetUserNotifications_ShouldReturnSuccess_WhenUserHasNoNotifications()
    {
        var userId = Guid.NewGuid().ToString();
        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(userId);
        var handler = new GetUserNotifications.Handler(_context, _mockUserAccessor.Object);

        var result = await handler.Handle(new GetUserNotifications.Query(), default);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task GetUserNotifications_ShouldReturnEmptyList_WhenUserHasNoNotifications()
    {
        var userId = Guid.NewGuid().ToString();
        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(userId);
        var handler = new GetUserNotifications.Handler(_context, _mockUserAccessor.Object);

        var result = await handler.Handle(new GetUserNotifications.Query(), default);

        Assert.Empty(result.Value!);
    }

    [Fact]
    public async Task GetUserNotifications_ShouldReturnNotificationsOrderedByCreatedAtDescending()
    {
        var userId = Guid.NewGuid().ToString();
        await SeedNotificationsAsync(userId, 3, 1);
        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(userId);
        var handler = new GetUserNotifications.Handler(_context, _mockUserAccessor.Object);

        var result = await handler.Handle(new GetUserNotifications.Query(), default);

        Assert.True(result.Value![0].CreatedAt >= result.Value[1].CreatedAt &&
                    result.Value[1].CreatedAt >= result.Value[2].CreatedAt);
    }

    [Fact]
    public async Task GetUserNotifications_ShouldReturnCorrectCount_ForAuthenticatedUser()
    {
        var userId1 = Guid.NewGuid().ToString();
        var userId2 = Guid.NewGuid().ToString();
        await SeedNotificationsAsync(userId1, 3, 1);
        await SeedNotificationsAsync(userId2, 2, 1);
        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(userId1);
        var handler = new GetUserNotifications.Handler(_context, _mockUserAccessor.Object);

        var result = await handler.Handle(new GetUserNotifications.Query(), default);

        Assert.Equal(3, result.Value!.Count);
    }
}