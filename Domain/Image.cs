using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Domain;

/// <summary>
/// Represents an image associated with an animal or shelter.
/// </summary>
/// <remarks>
/// Stores metadata such as URL, description, and creation timestamp,  
/// and can be linked either to an <see cref="Animal"/> or a <see cref="Shelter"/>.
/// </remarks>
public class Image
{
    /// <summary>
    /// Unique identifier of the image (GUID).
    /// </summary>
    [Key]
    public string Id { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Indicates whether this image is marked as the principal image.
    /// </summary>
    [Required]
    public bool IsPrincipal { get; set; }

    /// <summary>
    /// The foreign key referencing the associated animal, if applicable.
    /// </summary>
    public string? AnimalId { get; set; }

    [JsonIgnore]
    public Animal? Animal { get; set; }

    // Foreign Key for Shelter 
    public string? ShelterId { get; set; }
    [JsonIgnore]
    public Shelter? Shelter { get; set; }

    /// <summary>
    /// The URL pointing to the stored image resource.
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string Url { get; set; } = null!;

    [Required]

    /// <summary>
    /// A short optional description of the image content.
    /// </summary>
    [MaxLength(255)]
    public string? Description { get; set; }

    /// <summary>
    /// The UTC timestamp when the image record was created.
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}