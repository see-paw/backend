namespace WebAPI.DTOs.Shelter;

/// <summary>
/// Data transfer object representing basic shelter information.
/// </summary>
public class ResShelterDto
{
    /// <summary>
    /// Unique identifier of the shelter.
    /// </summary>
    public string Id { get; init; } = null!;

    /// <summary>
    /// Name of the shelter.
    /// </summary>
    public string Name { get; init; } = null!;
}
