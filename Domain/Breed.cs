using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Domain;

/// <summary>
/// Represents an animal breed that can be associated with animals.
/// </summary>
public class Breed
{
    [Key]
    public string Id { get; init; } = Guid.NewGuid().ToString();

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    // Navigation Property
    public ICollection<Animal> Animals { get; set; } = new List<Animal>();
}