using Domain;

namespace Application.Scheduling;

public class AnimalWeeklySchedule
{
    public required Animal Animal { get; set; }
    
    public required Shelter Shelter { get; set; }
    
    public required DateOnly StartDate { get; set; }
    
    public List<DailySchedule> WeekSchedule { get; set; } = new();
}