namespace WebAPI.DTOs.Favorites;

/// <summary>
/// Response DTO containing essential information about a favorite animal.
/// </summary>
/// <remarks>
/// Used to return a simplified view of animals marked as favorites by the user,
/// including the principal image, calculated age, and current state.
/// </remarks>
public class ResFavoriteAnimalDto
{
    /// <summary>
    /// Unique identifier of the animal.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The display name of the animal.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The species of the animal (e.g., Dog, Cat).
    /// </summary>
    public string Species { get; set; } = string.Empty;

    /// <summary>
    /// The breed name of the animal.
    /// </summary>
    public string Breed { get; set; } = string.Empty;

    /// <summary>
    /// The calculated age of the animal in years.
    /// </summary>
    public int Age { get; set; }

    /// <summary>
    /// The current availability or ownership state of the animal.
    /// </summary>
    public string AnimalState { get; set; } = string.Empty;

    /// <summary>
    /// The URL of the animal's principal (main) image.
    /// </summary>
    public string PrincipalImageUrl { get; set; } = string.Empty;

    /// <summary>
    /// The name of the shelter where the animal is located.
    /// </summary>
    public string ShelterName { get; set; } = string.Empty;

    /// <summary>
    /// The size category of the animal (e.g., Small, Medium, Large).
    /// </summary>
    public string Size { get; set; } = string.Empty;

    /// <summary>
    /// The biological sex of the animal.
    /// </summary>
    public string Sex { get; set; } = string.Empty;
}
