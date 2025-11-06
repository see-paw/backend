using Application.Interfaces;
using Domain;
using Domain.Enums;
using Infrastructure.Hubs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Infrastructure.Notifications;

/// <summary>
/// Implementation of notification service using SignalR for real-time delivery.
/// </summary>
public class NotificationService(
    UserManager<User> userManager,
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
        string? ownershipRequestId = null,
        string? activityId = null,
        CancellationToken cancellationToken = default)
    {
        // Create notification in database
        var notification = new Notification
        {
            UserId = userId,
            Type = type,
            Message = message,
            AnimalId = animalId,
            OwnershipRequestId = ownershipRequestId,
            ActivityId = activityId,
            IsRead = false,
            IsBroadcast = false,
            CreatedAt = DateTime.UtcNow
        };

        context.Notifications.Add(notification);
        await context.SaveChangesAsync(cancellationToken);

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
        // Fetch all users with the specified role using Identity
        var users = await userManager.GetUsersInRoleAsync(role);

        var notifications = new List<Notification>();

        foreach (var user in users)
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

        await context.SaveChangesAsync();

        // Send via SignalR to all users with the role
        if (notifications.Count != 0)
        {
            await SendToRoleAsync(role, notifications.First());
        }

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
            // Filter users by role based on ShelterId
            var users = await userManager.GetUsersInRoleAsync(role);
            var userIds = users.Select(u => u.Id).ToList();

            if (userIds.Count != 0)
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