using System.ComponentModel.DataAnnotations;

namespace Domain;

/// <summary>
/// Represents a time slot specifically associated with an <see cref="Activity"/>.
/// </summary>
public class ActivitySlot : Slot
{
    /// <summary>
    /// The foreign key referencing the associated <see cref="Activity"/>.
    /// </summary>
    [MaxLength(36)]
    public required string ActivityId { get; set; } =  string.Empty;
    
    /// <summary>
    /// The <see cref="Activity"/> entity that this slot belongs to.
    /// </summary>
    public Activity Activity { get; set; } = null!;
}