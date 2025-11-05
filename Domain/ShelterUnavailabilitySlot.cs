using System.ComponentModel.DataAnnotations;

namespace Domain;

/// <summary>
/// Represents a time slot during which a <see cref="Shelter"/> is unavailable.
/// </summary>
public class ShelterUnavailabilitySlot : Slot
{
    /// <summary>
    /// The foreign key referencing the associated <see cref="Shelter"/>.
    /// </summary>
    [MaxLength(36)]
    public required string ShelterId { get; set; }
    
    /// <summary>
    /// The reason for the shelter’s unavailability during this slot.
    /// </summary>
    [MaxLength(300)]
    public string? Reason { get; set; }

    /// <summary>
    /// The <see cref="Shelter"/> entity associated with this unavailability slot.
    /// </summary>
    public Shelter Shelter { get; set; } = null!;
}