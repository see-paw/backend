namespace WebAPI.DTOs.User;

/// <summary>
/// Response DTO containing the user's ID information.
/// </summary>
public class ResUserIdDto
{
    /// <summary>
    /// The unique identifier of the user.
    /// </summary>
    public string UserId { get; set; } = string.Empty;
}
