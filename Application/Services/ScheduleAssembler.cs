using Application.Interfaces;
using Application.Scheduling;
using Domain;

namespace Application.Services;

/// <summary>
/// Provides functionality to assemble a complete weekly schedule for a specific animal.
/// </summary>
/// <remarks>
/// Combines reserved activity slots, shelter unavailability slots, and available time blocks 
/// to generate a structured <see cref="AnimalWeeklySchedule"/>.  
/// This class implements <see cref="IScheduleAssembler"/> and is used to build the final schedule view 
/// consumed by higher-level services or controllers.
/// </remarks>
public class ScheduleAssembler : IScheduleAssembler
{
    /// <summary>
    /// Builds a complete weekly schedule for the specified animal.
    /// </summary>
    /// <param name="reservedSlots">A list of reserved activity slots for the given week.</param>
    /// <param name="unavailableSlots">A list of shelter unavailability slots for the same period.</param>
    /// <param name="availableSlots">A list of available time blocks calculated for the week.</param>
    /// <param name="animal">The <see cref="Animal"/> whose schedule is being assembled.</param>
    /// <param name="startDate">The starting date (typically the beginning of the week).</param>
    /// <returns>
    /// An <see cref="AnimalWeeklySchedule"/> object containing daily breakdowns of available, 
    /// reserved, and unavailable slots.
    /// </returns>
    public AnimalWeeklySchedule AssembleWeekSchedule(
        IReadOnlyList<ActivitySlot> reservedSlots, 
        IReadOnlyList<ShelterUnavailabilitySlot> unavailableSlots, 
        IReadOnlyList<TimeBlock> availableSlots, 
        Animal animal,
        DateOnly startDate)
    {
        var availByDay = availableSlots
            .GroupBy(r => r.Date)
            .ToDictionary(g => g.Key, g => g.Select(r => r).ToList());

        var reservedByDay = reservedSlots
            .GroupBy(s => DateOnly.FromDateTime(s.StartDateTime))
            .ToDictionary(g => g.Key, g => g.Cast<Slot>().ToList());

        var unavailableByDay = unavailableSlots
            .GroupBy(s => DateOnly.FromDateTime(s.StartDateTime))
            .ToDictionary(g => g.Key, g => g.Cast<Slot>().ToList());

        var sched = new AnimalWeeklySchedule { Animal = animal, Shelter = animal.Shelter, StartDate = startDate};

        for (var day = startDate; day < startDate.AddDays(7); day = day.AddDays(1))
        {
            availByDay.TryGetValue(day, out var dayAvail);
            reservedByDay.TryGetValue(day, out var dayRes);
            unavailableByDay.TryGetValue(day, out var dayUnv);

            sched.WeekSchedule.Add(new DailySchedule
            {
                Date = day,
                AvailableSlots = dayAvail ?? new List<TimeBlock>(),        
                ReservedSlots = dayRes ?? new List<Slot>(),           
                UnavailableSlots = dayUnv ?? new List<Slot>()  
            });
        }

        return sched;
    }
}