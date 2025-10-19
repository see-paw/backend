using Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;


namespace Domain;

public class Activity
{   
    [Key] // There is a constraint in AppDbContext.OnModelCreating that makes sure that (AnimalId, StartDate) is unique
    public string Id { get; init; } = Guid.NewGuid().ToString(); // Init instead of set to prevent changes after creation

    // Foreign Keys
    [Required]
    public string AnimalId { get; set; } = string.Empty;

    [Required]
    public string UserId { get; set; } = string.Empty;

    // Properties
    [Required]
    public ActivityType Type { get; set; } // "Fostering" or "Ownership"

    [Required]
    public ActivityStatus Status { get; set; } // "Active", "Cancelled", "Completed"

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public Animal Animal { get; set; } = null!;
    public User User { get; set; } = null!;
}
