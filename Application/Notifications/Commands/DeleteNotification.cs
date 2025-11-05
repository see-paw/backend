using Application.Core;
using Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Notifications.Commands;

/// <summary>
/// Handles the deletion of a user notification.
/// </summary>
/// <remarks>
/// This command allows the authenticated user to permanently delete a specific notification
/// from their account. It ensures that users can only delete notifications that belong
/// to them, enforcing proper access control and data integrity.
/// </remarks>
public class DeleteNotification
{
    /// <summary>
    /// Represents the command request to delete a notification.
    /// </summary>
    /// <remarks>
    /// Requires the <see cref="NotificationId"/> of the notification to be removed.
    /// </remarks>
    public class Command : IRequest<Result<Unit>>
    {
        /// <summary>
        /// ID of the notification to delete.
        /// </summary>
        public string NotificationId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Handles the execution of the command to delete a notification.
    /// </summary>
    /// <remarks>
    /// Performs the following operations:
    /// <list type="bullet">
    /// <item>Retrieves the authenticated user's ID using <see cref="IUserAccessor"/>.</item>
    /// <item>Queries the database for the notification matching both the provided ID and user ID.</item>
    /// <item>Returns a <c>404</c> result if no matching notification is found.</item>
    /// <item>Removes the notification and saves the changes.</item>
    /// <item>Returns success if the operation completes successfully.</item>
    /// </list>
    /// </remarks>
    public class Handler(
        AppDbContext context,
        IUserAccessor userAccessor) : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var userId = userAccessor.GetUserId();

            var notification = await context.Notifications
                .FirstOrDefaultAsync(n => n.Id == request.NotificationId && n.UserId == userId, cancellationToken);

            if (notification == null)
                return Result<Unit>.Failure("Notification not found", 404);

            context.Notifications.Remove(notification);

            var success = await context.SaveChangesAsync(cancellationToken) > 0;

            if (!success)
                return Result<Unit>.Failure("Failed to delete notification", 500);

            return Result<Unit>.Success(Unit.Value, 200);
        }
    }
}