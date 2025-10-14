using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain;

public class Photo
{
    [Key]
    public string ImageId { get; set; } = Guid.NewGuid().ToString();

    // Foreign Key
    [Required]
    public required string AnimalId { get; set; }
    public Animal? Animal { get; set; }

    [Required]
    [MaxLength(500)]
    public required string Url { get; set; }

    [MaxLength(255)]
    public string? Description { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
