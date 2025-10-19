<<<<<<< HEAD
using System.ComponentModel.DataAnnotations;
=======
﻿using System.ComponentModel.DataAnnotations;
>>>>>>> feature/create-and-list-animals
using Domain.Enums;

namespace Domain;

/// <summary>
/// Represents an ownership request made by a user for a specific animal.
/// </summary>

public class OwnershipRequest
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
    public OwnershipStatus Status { get; set; } = OwnershipStatus.Pending;

    [MaxLength(500)]
    public string? RequestInfo { get; set; }

    [Required]
    public DateTime RequestedAt { get; init; } = DateTime.UtcNow;

<<<<<<< HEAD
    public DateTime? ApprovedAt { get; set; } 
=======
    public DateTime? ApprovedAt { get; set; }
>>>>>>> feature/create-and-list-animals

    public DateTime? UpdatedAt { get; set; }

    // Navigation Properties
    public Animal Animal { get; set; } = null!;
    public User User { get; set; } = null!;


}