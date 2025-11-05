using System.ComponentModel.DataAnnotations;

namespace Domain;

public class ActivitySlot : Slot
{
    [MaxLength(36)]
    public required string ActivityId { get; set; } =  string.Empty;

    public Activity Activity { get; set; } = null!;
}