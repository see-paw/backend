using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace WebAPI.DTOs.Animals;

/// <summary>
/// Data transfer object used for filtering animals in API requests.
/// </summary>
public class AnimalFilterDto
{
    /// <summary>
    /// Animal species (e.g., Dog, Cat).
    /// </summary>
    public string? Species { get; set; }

    /// <summary>
    /// Animal age in years.
    /// </summary>
    public int? Age { get; set; }

    /// <summary>
    /// Animal size (e.g., Small, Medium, Large).
    /// </summary>
    public string? Size { get; set; }

    /// <summary>
    /// Animal color.
    /// </summary>
    public string? Color { get; set; }

    /// <summary>
    /// Animal sex (e.g., Male, Female).
    /// </summary>
    public string? Sex { get; set; }

    /// <summary>
    /// Animal name or partial name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Name of the shelter where the animal is located.
    /// </summary>
    public string? ShelterName { get; set; }

    /// <summary>
    /// Animal breed.
    /// </summary>
    public string? Breed { get; set; }
}
