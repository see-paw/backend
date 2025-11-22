namespace WebAPI.DTOs.User;

/// <summary>
/// Response DTO containing complete information about the currently authenticated user.
/// Returned by the GET /api/users/me endpoint.
/// </summary>
public class ResCurrentUserDto
{
    /// <summary>
    /// The unique identifier of the user.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// The user's email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// The full name of the user.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The role assigned to the user.
    /// Possible values: "User", "AdminCAA", "PlatformAdmin".
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// The identifier of the shelter managed by the user.
    /// Only populated when the user has the AdminCAA role; otherwise null.
    /// </summary>
    public string? ShelterId { get; set; }

    /// <summary>
    /// The user's date of birth.
    /// </summary>
    public DateTime BirthDate { get; set; }

    /// <summary>
    /// The street address of the user.
    /// </summary>
    public string Street { get; set; } = string.Empty;

    /// <summary>
    /// The city where the user resides.
    /// </summary>
    public string City { get; set; } = string.Empty;

    /// <summary>
    /// The postal code of the user's address.
    /// Format: 0000-000 (Portuguese postal code format).
    /// </summary>
    public string PostalCode { get; set; } = string.Empty;

    /// <summary>
    /// The user's phone number.
    /// </summary>
    public string PhoneNumber { get; set; } = string.Empty;
}
