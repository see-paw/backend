using Application.Notifications.Commands;
using FluentValidation;

namespace WebAPI.Validators.Notifications;

/// <summary>
/// Validates the <see cref="MarkNotificationAsRead.Command"/> request before execution.
/// </summary>
/// <remarks>
/// Ensures that the command to mark a notification as read includes a valid,
/// non-empty notification identifier.  
/// This validation layer prevents invalid data from reaching the application logic
/// and enforces early input correctness.
/// </remarks>
public class MarkNotificationAsReadValidator : AbstractValidator<MarkNotificationAsRead.Command>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MarkNotificationAsReadValidator"/> class.
    /// </summary>
    /// <remarks>
    /// Defines validation rules for the <see cref="MarkNotificationAsRead.Command"/>:
    /// <list type="bullet">
    /// <item><description><see cref="MarkNotificationAsRead.Command.NotificationId"/> must not be empty.</description></item>
    /// </list>
    /// </remarks>
    public MarkNotificationAsReadValidator()
    {
        RuleFor(x => x.NotificationId)
            .NotEmpty()
            .WithMessage("Notification ID is required");
    }
}