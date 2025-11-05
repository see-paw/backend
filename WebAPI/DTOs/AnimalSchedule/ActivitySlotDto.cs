namespace WebAPI.DTOs.AnimalSchedule;

public class ActivitySlotDto : SlotDto
{
    public string ReservedBy { get; init; } = string.Empty;
    
    public bool IsOwnReservation { get; init; }
}