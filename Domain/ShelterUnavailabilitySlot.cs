using System.ComponentModel.DataAnnotations;

namespace Domain;

public class ShelterUnavailabilitySlot : Slot
{
    [MaxLength(36)]
    public required string ShelterId { get; set; }
    
    public string? Reason { get; set; }

    public Shelter Shelter { get; set; } = null!;
}