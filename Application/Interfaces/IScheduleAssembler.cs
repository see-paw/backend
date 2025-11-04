using System.Runtime.InteropServices.JavaScript;
using Application.Scheduling;
using Domain;

namespace Application.Interfaces;

public interface IScheduleAssembler
{
    AnimalWeeklySchedule AssembleWeekSchedule(
        IReadOnlyList<ActivitySlot> reservedSlots, 
        IReadOnlyList<ShelterUnavailabilitySlot> unavailableSlots,
        IReadOnlyList<TimeBlock> availableSlots,
        Animal animal,
        DateOnly startDate);
}