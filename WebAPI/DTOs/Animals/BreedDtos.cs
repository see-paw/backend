namespace WebAPI.DTOs.Animals;

/// <summary>
/// DTO for TheDogAPI breed response
/// Maps the external API structure to our domain
/// </summary>
public class BreedDtos
{
    /// <summary>
    /// The identifier of the breed as provided by the external API.
    /// This value does NOT correspond to the internal database ID.
    /// </summary>
    public string? Id { get; set; }
    /// <summary>
    /// Breed name from the external API
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Breed temperament (will be used as Description in domain)
    /// Example: "Stubborn, Curious, Playful, Adventurous, Active, Fun-loving"
    /// </summary>
    public string? Description { get; set; }
}
