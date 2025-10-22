using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Domain;

public class User
{
    [Key]
    public string Id { get; init; } = Guid.NewGuid().ToString();

    // CAA Administration (optional, only for Admin CAA users)
    public string? ShelterId { get; set; }

    // Personal Information
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public DateTime BirthDate { get; set; }

    // Address
    [Required]
    [MaxLength(255)]
    public string Street { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string City { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string PostalCode { get; set; } = string.Empty;

    // Contact
    [Required]
    [Phone]
    [MaxLength(9)]
    public string PhoneNumber { get; set; } = string.Empty;

    // Authentication
    [Required]
    [MaxLength(255)]
    public string PasswordHash { get; set; } = string.Empty;

    // Timestamps
    [Required]
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation Properties
    public Shelter? Shelter { get; set; }

    public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    public ICollection<Activity> Activities { get; set; } = new List<Activity>();
    public ICollection<Fostering> Fosterings { get; set; } = new List<Fostering>();
    public ICollection<OwnershipRequest> OwnershipRequests { get; set; } = new List<OwnershipRequest>();
    public ICollection<Animal> OwnedAnimals { get; set; } = new List<Animal>();
}