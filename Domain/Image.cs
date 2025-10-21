using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Domain;

public class Image
{
    [Key]
    public string Id { get; init; } = Guid.NewGuid().ToString();

    [Required]
    public bool IsPrincipal { get; set; }

    // Foreign Key
    public string? AnimalId { get; set; }

    [JsonIgnore]
    public Animal? Animal { get; set; }

    // Foreign Key for Shelter 
    public string? ShelterId { get; set; }
    [JsonIgnore]
    public Shelter? Shelter { get; set; }

    [Required]
    [MaxLength(500)]
    public string Url { get; set; } = null!;

    [Required]

    [MaxLength(255)]
    public string? Description { get; set; }

    [Required]
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}