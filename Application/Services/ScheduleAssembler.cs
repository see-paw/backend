using Application.Interfaces;
using Application.Scheduling;
using Domain;

namespace Application.Services;

public class ScheduleAssembler : IScheduleAssembler
{
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