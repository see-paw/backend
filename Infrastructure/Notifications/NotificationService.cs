using Application.Interfaces;
using Domain;
using Domain.Enums;
using Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Infrastructure.Notifications;

/// <summary>
/// Implementation of notification service using SignalR for real-time delivery.
/// </summary>
public class NotificationService(
    IHubContext<NotificationHub> hubContext, 
    AppDbContext context,
    ILogger<NotificationService> logger) : INotificationService
{
    /// <summary>
    /// Creates and sends a notification to a specific user.
    /// </summary>
    public async Task<Notification> CreateAndSendToUserAsync(
        string userId,
        NotificationType type,
        string message,
        string? animalId = null,
        string? ownershipRequestId = null)
    {
        // Create notification in database
        var notification = new Notification
        {
            UserId = userId,
            Type = type,
            Message = message,
            AnimalId = animalId,
            OwnershipRequestId = ownershipRequestId,
            IsRead = false,
            IsBroadcast = false,
            CreatedAt = DateTime.UtcNow
        };

        context.Notifications.Add(notification);
        await context.SaveChangesAsync();

        // Send via SignalR (if user is connected)
        await SendToUserAsync(userId, notification);

        return notification;
    }

    /// <summary>
    /// Creates and sends a broadcast notification to all users with a specific role.
    /// </summary>
    public async Task<List<Notification>> CreateAndSendToRoleAsync(
        string role,
        NotificationType type,
        string message,
        string? animalId = null)
    {
        // [TODO]: refactor when roles are implemented, this was implemented without defined user roles "AdminCAA" / "User"
        // Get all users with the specified role
        var users = role == "AdminCAA"
        ? await context.Users.Where(u => u.ShelterId != null).ToListAsync()
        : await context.Users.Where(u => u.ShelterId == null).ToListAsync();

        var notifications = new List<Notification>();

        foreach (var user in users)
        {
            // Check if user has the specified role
            var userRoles = await context.UserRoles
                .Where(ur => ur.UserId == user.Id)
                .Join(context.Roles,
                    ur => ur.RoleId,
                    r => r.Id,
                    (ur, r) => r.Name)
                .ToListAsync();

            if (userRoles.Contains(role))
            {
                var notification = new Notification
                {
                    UserId = user.Id,
                    Type = type,
                    Message = message,
                    AnimalId = animalId,
                    IsRead = false,
                    IsBroadcast = true,
                    TargetRole = role,
                    CreatedAt = DateTime.UtcNow
                };

                context.Notifications.Add(notification);
                notifications.Add(notification);
            }
        }

        await context.SaveChangesAsync();

        // Send via SignalR to all users with the role
        await SendToRoleAsync(role, notifications.First());

        return notifications;
    }

    /// <summary>
    /// Sends a notification via SignalR to a specific user (if connected).
    /// </summary>
    public async Task SendToUserAsync(string userId, Notification notification)
    {
        try
        {
            await hubContext.Clients.User(userId).SendAsync("ReceiveNotification", new
            {
                notification.Id,
                notification.Type,
                notification.Message,
                notification.AnimalId,
                notification.OwnershipRequestId,
                notification.CreatedAt
            });
        }
        catch (Exception ex)
        {
            // Log error but don't fail, user might not be connected
            logger.LogError(ex, "Error sending notification to user {UserId}", userId);
        }
    }

    /// <summary>
    /// Sends a notification via SignalR to all users with a specific role (if connected).
    /// </summary>
    public async Task SendToRoleAsync(string role, Notification notification)
    {
        try
        {
            // [TODO]: refactor when roles are implemented, this was implemented without defined user roles "AdminCAA"/"User"
            // Filter users by role based on ShelterId
            var userIds = role == "AdminCAA"
                ? await context.Users.Where(u => u.ShelterId != null).Select(u => u.Id).ToListAsync()
                : await context.Users.Where(u => u.ShelterId == null).Select(u => u.Id).ToListAsync();

            if (userIds.Any())
            {
                await hubContext.Clients.Users(userIds).SendAsync("ReceiveNotification", new
                {
                    notification.Id,
                    notification.Type,
                    notification.Message,
                    notification.AnimalId,
                    notification.OwnershipRequestId,
                    notification.CreatedAt,
                    notification.TargetRole
                });
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending notification to role {Role}", role);
        }
    }
}