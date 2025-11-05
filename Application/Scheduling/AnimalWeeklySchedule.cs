using Domain;

namespace Application.Scheduling;

/// <summary>
/// Represents the complete weekly schedule for a specific animal.
/// </summary>
public class AnimalWeeklySchedule
{
    /// <summary>
    /// The <see cref="Animal"/> entity for which this weekly schedule is generated.
    /// </summary>
    public required Animal Animal { get; set; }
    
    /// <summary>
    /// The <see cref="Shelter"/> that manages the animal and defines the operational hours for scheduling.
    /// </summary>
    public required Shelter Shelter { get; set; }
    
    /// <summary>
    /// The start date of the weekly schedule period.
    /// </summary>
    public required DateOnly StartDate { get; set; }
    
    /// <summary>
    /// The collection of daily schedules that make up the week’s schedule.
    /// </summary>
    public List<DailySchedule> WeekSchedule { get; set; } = new();
}