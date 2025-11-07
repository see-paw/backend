namespace WebAPI.DTOs.Activities;

/// <summary>
/// Data transfer object returned after successfully cancelling an activity.
/// </summary>
public class ResCancelActivityFosteringDto
{
    /// <summary>
    /// The unique identifier of the cancelled activity.
    /// </summary>
    public string ActivityId { get; set; } = string.Empty;

    /// <summary>
    /// Success message confirming the cancellation.
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
