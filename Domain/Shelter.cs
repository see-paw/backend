using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Domain;

/// <summary>
/// Represents an animal shelter in the system, including its identification, location,
/// contact details, operating hours, and the animals currently under its care.
/// </summary>

public class Shelter
{
    [Key]
    public string Id { get; init; } = Guid.NewGuid().ToString();

    [Required]
    [StringLength(255, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string Street { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string City { get; set; } = string.Empty;

    [Required]
    [RegularExpression(@"^\d{4}-\d{3}$", ErrorMessage = "Postal Code must be in the format 0000-000.")]
    public string PostalCode { get; set; } = string.Empty;

    [Required]
    [Phone]
    [RegularExpression(@"^[29]\d{8}$", ErrorMessage = "Phone number must have 9 digits and start with 2 or 9.")]
    public string Phone { get; set; } = string.Empty;

    [Required]
    [RegularExpression(@"^\d{9}$", ErrorMessage = "NIF must contain exactly 9 digits.")]
    public string NIF { get; init; } = string.Empty;

    [Required]
    public TimeOnly OpeningTime { get; set; }

    [Required]
    public TimeOnly ClosingTime { get; set; }

    [Required]
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation Properties

    [JsonIgnore]
    [MinLength(1, ErrorMessage = "Shelter must have at least one image.")]
    public ICollection<Image> Images { get; set; } = new List<Image>();
    public ICollection<Animal> Animals { get; set; } = new List<Animal>();
}
