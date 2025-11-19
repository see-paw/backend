namespace WebAPI.DTOs.User;

/// <summary>
/// Response DTO containing the user's role information.
/// </summary>
public class ResUserRoleDto
{
    /// <summary>
    /// The role of the user: "User" or "AdminCAA".
    /// </summary>
    public string Role { get; set; } = string.Empty;
}
