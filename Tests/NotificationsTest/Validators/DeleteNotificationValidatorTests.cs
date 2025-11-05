using Application.Notifications.Commands;
using WebAPI.Validators.Notifications;

namespace Tests.Notifications;

public class DeleteNotificationValidatorTests
{
    private readonly DeleteNotificationValidator _validator;

    public DeleteNotificationValidatorTests()
    {
        _validator = new DeleteNotificationValidator();
    }

    [Fact]
    public void Validate_ShouldReturnValid_WhenNotificationIdIsProvided()
    {
        var command = new DeleteNotification.Command
        {
            NotificationId = Guid.NewGuid().ToString()
        };

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_ShouldReturnInvalid_WhenNotificationIdIsEmpty()
    {
        var command = new DeleteNotification.Command
        {
            NotificationId = string.Empty
        };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_ShouldReturnInvalid_WhenNotificationIdIsNull()
    {
        var command = new DeleteNotification.Command
        {
            NotificationId = null!
        };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_ShouldReturnInvalid_WhenNotificationIdIsWhitespace()
    {
        var command = new DeleteNotification.Command
        {
            NotificationId = "   "
        };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_ShouldReturnCorrectErrorMessage_WhenNotificationIdIsEmpty()
    {
        var command = new DeleteNotification.Command
        {
            NotificationId = string.Empty
        };

        var result = _validator.Validate(command);

        Assert.Contains("Notification ID is required", result.Errors.Select(e => e.ErrorMessage));
    }
}