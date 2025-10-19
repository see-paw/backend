using System.ComponentModel.DataAnnotations;
namespace Domain;

public class Image
{
    [Key]
    public string Id { get; init; } = Guid.NewGuid().ToString();

    [Required]
    public bool IsPrincipal { get; set; }

    // Foreign Key
    public string? AnimalId { get; set; }

    public Animal? Animal { get; set; }
    public string? ShelterId { get; set; }
    public Shelter? Shelter { get; set; }

    [Required]
    [MaxLength(500)]
    public string Url { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Description { get; set; }

    [Required]
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}