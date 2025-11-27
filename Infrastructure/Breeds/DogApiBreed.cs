namespace Infrastructure.Breeds;

public class DogApiBreed
{
    /// <summary>
    /// Breed name from the external API
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Breed temperament (will be used as Description in domain)
    /// Example: "Stubborn, Curious, Playful, Adventurous, Active, Fun-loving"
    /// </summary>
    public string? Temperament { get; set; }
}
