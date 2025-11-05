using Application.Notifications.Commands;
using FluentValidation;

namespace WebAPI.Validators.Notifications;

/// <summary>
/// Validates the <see cref="DeleteNotification.Command"/> request before processing.
/// </summary>
/// <remarks>
/// Ensures that the notification deletion request contains a valid and non-empty
/// notification identifier.  
/// This validator prevents invalid delete operations and improves API robustness
/// by enforcing input integrity at the boundary layer.
/// </remarks>
public class DeleteNotificationValidator : AbstractValidator<DeleteNotification.Command>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteNotificationValidator"/> class.
    /// </summary>
    /// <remarks>
    /// Defines validation rules for the <see cref="DeleteNotification.Command"/>:
    /// <list type="bullet">
    /// <item><description><see cref="DeleteNotification.Command.NotificationId"/> must not be empty.</description></item>
    /// </list>
    /// </remarks>
    public DeleteNotificationValidator()
    {
        RuleFor(x => x.NotificationId)
            .NotEmpty()
            .WithMessage("Notification ID is required");
    }
}