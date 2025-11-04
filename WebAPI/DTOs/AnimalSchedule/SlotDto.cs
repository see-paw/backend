using Domain.Enums;

namespace WebAPI.DTOs.AnimalSchedule;

public class SlotDto
{
    public string Id { get; set; }
    
    public string Start { get; init; } = null!;
    
    public string End { get; init; } = null!;
    
}