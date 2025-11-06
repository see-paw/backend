using System.ComponentModel.DataAnnotations;
using Domain.Enums;
using Domain.Interfaces;

namespace Domain;


/// <summary>
/// Represents a generic time slot used for scheduling within the system.
/// </summary>
public abstract class Slot
{
    /// <summary>
    /// Unique identifier of the slot (GUID).
    /// </summary>
    [Key] [MaxLength(36)] public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// The UTC start date and time of the slot.
    /// </summary>
    public required DateTime StartDateTime { get; set; }
    
    /// <summary>
    /// The UTC end date and time of the slot.
    /// </summary>
    public required DateTime EndDateTime { get; set; }
    
    /// <summary>
    /// The current availability status of the slot.
    /// </summary>
    public required SlotStatus Status { get; set; }
    
    /// <summary>
    /// The type of slot within the scheduling context.
    /// </summary>
    public required SlotType Type { get; set; }

    /// <summary>
    /// The UTC timestamp when the slot record was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// The UTC timestamp when the slot record was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}