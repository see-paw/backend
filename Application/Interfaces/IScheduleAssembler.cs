using System.Runtime.InteropServices.JavaScript;
using Application.Scheduling;
using Domain;

namespace Application.Interfaces;

/// <summary>
/// Defines the contract for assembling an animal’s complete weekly schedule.
/// </summary>
/// <remarks>
/// Implementations of this interface combine reserved, unavailable, and available time slots 
/// to produce a structured <see cref="AnimalWeeklySchedule"/> used in scheduling and calendar views.
/// </remarks>
public interface IScheduleAssembler
{
    /// <summary>
    /// Builds a full weekly schedule for the specified animal.
    /// </summary>
    /// <param name="reservedSlots">The list of reserved activity slots for the week.</param>
    /// <param name="unavailableSlots">The list of shelter unavailability slots for the same period.</param>
    /// <param name="availableSlots">The list of available time blocks for the week.</param>
    /// <param name="animal">The <see cref="Animal"/> whose schedule is being built.</param>
    /// <param name="startDate">The date marking the start of the week.</param>
    /// <returns>
    /// An <see cref="AnimalWeeklySchedule"/> containing all available, reserved, and unavailable slots 
    /// organized by day.
    /// </returns>
    /// <remarks>
    /// The returned schedule aggregates daily slot information, ensuring that all seven days from 
    /// <paramref name="startDate"/> are represented.  
    /// This method is used by higher-level scheduling services to generate complete weekly availability views.
    /// </remarks>
    AnimalWeeklySchedule AssembleWeekSchedule(
        IReadOnlyList<ActivitySlot> reservedSlots, 
        IReadOnlyList<ShelterUnavailabilitySlot> unavailableSlots,
        IReadOnlyList<TimeBlock> availableSlots,
        Animal animal,
        DateOnly startDate);
}