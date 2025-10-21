using System.ComponentModel.DataAnnotations;

namespace Domain;

/// <summary>
/// Represents an animal breed that can be associated with animals.
/// </summary>
/// <remarks>
/// Contains identifying and descriptive information about a breed,  
/// as well as its relationship with the animals belonging to it.
/// </remarks>
public class Breed
{
    /// <summary>
    /// Unique identifier of the breed (GUID).
    /// </summary>
    [Key]
    public string Id { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// The name of the breed.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// An optional textual description of the breed.
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// The UTC timestamp when the breed record was created.
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// The collection of animals that belong to this breed.
    /// </summary>
    public ICollection<Animal> Animals { get; set; } = new List<Animal>();
}