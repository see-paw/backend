using System.ComponentModel.DataAnnotations;
using Domain.Enums;
using Domain.Interfaces;

namespace Domain;

public class Slot
{
    [Key]
    [MaxLength(36)]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    public required DateTime StartDateTime { get; set; }
    
    public required DateTime EndDateTime { get; set; }

    public required SlotStatus Status { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; } 
    
    public string? ShelterId { get; set; }
    public Shelter? Shelter { get; set; }
    public Activity? Activity { get; set; } 
}