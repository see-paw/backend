using System.ComponentModel.DataAnnotations;

namespace Domain;
public class Favorite
{
    [Key]
    public string Id { get; init; } = Guid.NewGuid().ToString();

    // Foreign Keys
    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public string AnimalId { get; set; } = string.Empty;

    // Properties
    [Required]
    public bool IsActive { get; set; } // Indicates if it still is currently a favorite

    [Required]
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation Properties
    public Animal Animal { get; set; } = null!;
    public User User { get; set; } = null!;
}