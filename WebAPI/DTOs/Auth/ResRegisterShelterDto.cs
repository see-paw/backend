namespace WebAPI.DTOs.Auth;

/// <summary>
/// Data transfer object representing shelter information returned after registration.
/// </summary>
public class ResRegisterShelterDto
{
    /// <summary>
    /// Unique identifier of the shelter.
    /// </summary>
    public string Id { get; init; } = null!;

    /// <summary>
    /// Name of the shelter.
    /// </summary>
    public string Name { get; init; } = null!;

    /// <summary>
    /// Street address of the shelter.
    /// </summary>
    public string Street { get; init; } = null!;

    /// <summary>
    /// City where the shelter is located.
    /// </summary>
    public string City { get; init; } = null!;

    /// <summary>
    /// Postal code of the shelter.
    /// </summary>
    public string PostalCode { get; init; } = null!;

    /// <summary>
    /// Contact phone number of the shelter.
    /// </summary>
    public string Phone { get; init; } = null!;

    /// <summary>
    /// Tax identification number (NIF) of the shelter.
    /// </summary>
    public string NIF { get; init; } = null!;

    /// <summary>
    /// Opening time of the shelter (e.g., "09:00").
    /// </summary>
    public string OpeningTime { get; init; } = null!;

    /// <summary>
    /// Closing time of the shelter (e.g., "18:00").
    /// </summary>
    public string ClosingTime { get; init; } = null!;
}
