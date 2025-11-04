using Domain;

namespace Application.Scheduling;

public class DailySchedule
{
    public required DateOnly Date { get; init; }
    public required List<TimeBlock> AvailableSlots { get; set; } = new();
    
    public required List<Slot> ReservedSlots { get; set; } = new();
    
    public required List<Slot> UnavailableSlots { get; set; } = new();
}