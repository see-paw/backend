using Domain.Enums;

namespace WebAPI.DTOs.AnimalSchedule;

public class SlotDto
{
    public string Id { get; set; }
    
    public DateTime StartDateTime { get; set; }
    
    public DateTime EndDateTime { get; set; }
    
    public SlotStatus Status { get; set; }
    
    public string ReservedBy { get; set; }
    
    public bool IsOwnReservation { get; set; }
}