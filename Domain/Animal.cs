using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Domain;

/// <summary>
/// Represents an animal entity managed by the application.
/// </summary>
/// <remarks>
/// The <see cref="Animal"/> class defines the core attributes of an animal stored in the system’s database,
/// including identification, biological characteristics, state, and adoption-related metadata.  
/// 
/// Each instance corresponds to a record in the <c>Animals</c> table and includes validation attributes
/// that enforce data integrity at both model and database levels.  
/// 
/// The entity also includes creation and update timestamps to support auditability and change tracking.
/// </remarks>

public class Animal
{
    [Key]
    public string AnimalId { get; init; } = Guid.NewGuid().ToString();

    [Required, StringLength(100, MinimumLength = 2)]
    public required string Name { get; init; }

    [Required]
    public required AnimalState AnimalState { get; init; }

    [StringLength(1000)]
    public string? Description { get; init; }

    [Required]
    public required Species Species { get; init; }

    [Required]
    public required SizeType Size { get; init; }

    [Required]
    public required SexType Sex { get; init; }

    [Required, StringLength(50)]
    public required string Colour { get; init; }

    [Required]
    public required DateOnly BirthDate { get; init; }

    [Required]
    public required bool Sterilized { get; init; }

    [Required]
    public required Breed Breed { get; init; }

    [Required, Range(0, 1000, ErrorMessage = "Cost must be between 0 and 1000.")]
    public required decimal Cost { get; init; }

    [StringLength(300)]
    public required string? Features { get; init; }

    [Required]
    public  DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    [Required]
    public  DateTime UpdatedAt { get; init; } = DateTime.UtcNow;

    [Required, Url, StringLength(500)]
    public required string MainImageUrl { get; init; }
}
