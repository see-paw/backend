using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Domain;

/// <summary>
/// Represents a user within the system.
/// </summary>
/// <remarks>
/// Contains personal, contact, and authentication information,  
/// as well as relationships to shelters, animals, and user-related activities.
/// </remarks>

[Table("Users")]
public class User : IdentityUser
{
    /// <summary>
    /// The identifier of the shelter managed by the user, if the user is an Admin CAA.
    /// </summary>
    [MaxLength(36)]
    public string? ShelterId { get; set; }

    /// <summary>
    /// The full name of the user.
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;
    
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
    /// The postal code of the user�s address.
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string PostalCode { get; set; } = string.Empty;

    /// <summary>
    /// The UTC timestamp when the user account was created.
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

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
