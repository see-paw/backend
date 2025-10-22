using System.ComponentModel.DataAnnotations;
using Domain.Enums;


namespace Domain;

/// <summary>
/// Represents an animal fostering,including the fostering period and contribution amount.
/// </summary>

public class Fostering
{
    [Key]
    public string Id { get; init; } = Guid.NewGuid().ToString();

    // Foreign Keys
    [Required]
    public string AnimalId { get; set; } = string.Empty;

    [Required]
    public string UserId { get; set; } = string.Empty;

    // Properties
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0.")]
    public decimal Amount { get; set; }

    [Required]
    public FosteringStatus Status { get; set; } = FosteringStatus.Active;

    [Required]
    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public DateTime? UpdatedAt { get; set; }

    // Navigation Properties
    public Animal Animal { get; set; } = null!;
    public User User { get; set; } = null!;
}
