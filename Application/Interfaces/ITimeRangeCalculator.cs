using Application.Scheduling;
using Domain;

namespace Application.Interfaces;

/// <summary>
/// Defines the contract for calculating available time ranges within a weekly schedule.
/// </summary>
/// <remarks>
/// Implementations of this interface determine open time periods for a shelter by analyzing existing 
/// <see cref="Slot"/> instances (both occupied and unavailable) against operational hours.  
/// The resulting list of <see cref="TimeBlock"/> objects represents all free time ranges 
/// available for scheduling during the specified week.
/// </remarks>
public interface ITimeRangeCalculator
{
    /// <summary>
    /// Calculates the available time ranges for a full week based on occupied slots and shelter operating hours.
    /// </summary>
    /// <param name="occupiedSlots">A collection of all slots that are reserved or unavailable.</param>
    /// <param name="opening">The shelter’s daily opening time.</param>
    /// <param name="closing">The shelter’s daily closing time.</param>
    /// <param name="weekStart">The date marking the start of the week for which availability is being calculated.</param>
    /// <returns>
    /// A read-only list of <see cref="TimeBlock"/> instances representing available time ranges 
    /// for each day within the specified week.
    /// </returns>
    /// <remarks>
    /// The calculation process includes:
    /// <list type="bullet">
    /// <item>Identifying occupied time ranges and grouping them by day.</item>
    /// <item>Determining available intervals between occupied blocks.</item>
    /// <item>Adding fully open days when no occupied slots exist.</item>
    /// <item>Ensuring all results respect the shelter’s opening and closing hours.</item>
    /// </list>
    /// </remarks>
    public IReadOnlyList<TimeBlock> CalculateWeeklyAvailableRanges(
        IEnumerable<Slot> occupiedSlots,
        TimeSpan opening,
        TimeSpan closing,
        DateOnly weekStart);
}