using Domain;

namespace Application.Scheduling;

public class AnimalWeeklySchedule
{
    public Animal Animal { get; set; }
    
    public Shelter Shelter { get; set; }
    
    public Dictionary<DateOnly, List<Slot>> WeeklySchedule { get; set; } = new();
}