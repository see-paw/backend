using Application.Notifications.Queries;

using WebAPI.Validators.Notifications;

namespace Tests.NotificationsTests.Validators;

public class MarkNotificationAsReadValidatorTests
{
    private readonly MarkNotificationAsReadValidator _validator;

    public MarkNotificationAsReadValidatorTests()
    {
        _validator = new MarkNotificationAsReadValidator();
    }

    [Fact]
    public void Validate_ShouldReturnValid_WhenNotificationIdIsProvided()
    {
        var command = new MarkNotificationAsRead.Command
        {
            NotificationId = Guid.NewGuid().ToString()
        };

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_ShouldReturnInvalid_WhenNotificationIdIsEmpty()
    {
        var command = new MarkNotificationAsRead.Command
        {
            NotificationId = string.Empty
        };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_ShouldReturnInvalid_WhenNotificationIdIsNull()
    {
        var command = new MarkNotificationAsRead.Command
        {
            NotificationId = null!
        };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_ShouldReturnInvalid_WhenNotificationIdIsWhitespace()
    {
        var command = new MarkNotificationAsRead.Command
        {
            NotificationId = "   "
        };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_ShouldReturnCorrectErrorMessage_WhenNotificationIdIsEmpty()
    {
        var command = new MarkNotificationAsRead.Command
        {
            NotificationId = string.Empty
        };

        var result = _validator.Validate(command);

        Assert.Contains("Notification ID is required", result.Errors.Select(e => e.ErrorMessage));
    }
}