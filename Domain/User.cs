using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Domain;

/// <summary>
/// Represents a user within the system.
/// </summary>
/// <remarks>
/// Contains personal, contact, and authentication information,  
/// as well as relationships to shelters, animals, and user-related activities.
/// </remarks>
public class User
{
    /// <summary>
    /// Unique identifier of the user (GUID).
    /// </summary>
    [Key]
    public string Id { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// The identifier of the shelter managed by the user, if the user is an Admin CAA.
    /// </summary>
    public string? ShelterId { get; set; }

    /// <summary>
    /// The full name of the user.
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The user’s email address.
    /// </summary>
    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// The date of birth of the user.
    /// </summary>
    [Required]
    public DateTime BirthDate { get; set; }

    /// <summary>
    /// The street address of the user.
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string Street { get; set; } = string.Empty;

    /// <summary>
    /// The city where the user resides.
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string City { get; set; } = string.Empty;

    /// <summary>
    /// The postal code of the user’s address.
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string PostalCode { get; set; } = string.Empty;

    /// <summary>
    /// The user’s phone number.  
    /// Must contain 9 digits.
    /// </summary>
    [Required]
    [Phone]
    [MaxLength(9)]
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// The hashed password used for user authentication.
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// The UTC timestamp when the user account was created.
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// The UTC timestamp when the user account was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// The shelter associated with this user (if the user is an Admin CAA).
    /// </summary>
    public Shelter? Shelter { get; set; }

    /// <summary>
    /// The collection of animals marked as favorites by the user.
    /// </summary>
    public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();

    /// <summary>
    /// The list of activities performed by the user within the system.
    /// </summary>
    public ICollection<Activity> Activities { get; set; } = new List<Activity>();

    /// <summary>
    /// The list of fostering relationships associated with the user.
    /// </summary>
    public ICollection<Fostering> Fosterings { get; set; } = new List<Fostering>();

    /// <summary>
    /// The collection of ownership requests submitted by the user.
    /// </summary>
    public ICollection<OwnershipRequest> OwnershipRequests { get; set; } = new List<OwnershipRequest>();

    /// <summary>
    /// The list of animals currently owned by the user.
    /// </summary>
    public ICollection<Animal> OwnedAnimals { get; set; } = new List<Animal>();
}
