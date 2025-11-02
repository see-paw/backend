using System.ComponentModel.DataAnnotations;
using Domain.Enums;
using Domain.Interfaces;

namespace Domain;

public abstract class Slot
{
    [Key] [MaxLength(36)] public string Id { get; set; } = Guid.NewGuid().ToString();

    public required DateTime StartDateTime { get; set; }

    public required DateTime EndDateTime { get; set; }

    public required SlotStatus Status { get; set; }
    
    public required SlotType Type { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}