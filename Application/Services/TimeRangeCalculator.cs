using Application.Interfaces;
using Application.Scheduling;
using Domain;
using Domain.Enums;

namespace Application.Services;

/// <summary>
/// Provides functionality to calculate available time ranges within a given week based on occupied and available slots.
/// </summary>
/// <remarks>
/// Implements <see cref="ITimeRangeCalculator"/> and determines the free time blocks within the shelter’s operating hours
/// by processing both reserved and unavailable slots.  
/// The resulting data is used to build weekly schedules for animals.
/// </remarks>
public class TimeRangeCalculator : ITimeRangeCalculator
{
    /// <summary>
    /// Calculates all available time ranges for a given week.
    /// </summary>
    /// <param name="allSlots">A collection of all slots (available, reserved, or unavailable) to analyze.</param>
    /// <param name="opening">The daily opening time of the shelter.</param>
    /// <param name="closing">The daily closing time of the shelter.</param>
    /// <param name="weekStart">The start date of the week for which the availability will be calculated.</param>
    /// <returns>
    /// A list of <see cref="TimeBlock"/> objects representing all available time ranges for the specified week.
    /// </returns>
    /// <remarks>
    /// This method performs the following:
    /// <list type="number">
    /// <item>Filters out all slots that are not available within the week range.</item>
    /// <item>Identifies occupied time ranges and groups them by date.</item>
    /// <item>Calculates available blocks between occupied ones for each day.</item>
    /// <item>Adds fully free days when no occupied slots exist for a given date.</item>
    /// </list>
    /// If no occupied slots are found for the week, the method returns a full week of open availability.
    /// </remarks>
    public IReadOnlyList<TimeBlock> CalculateWeeklyAvailableRanges(
        IEnumerable<Slot> allSlots, 
        TimeSpan opening, 
        TimeSpan closing,
        DateOnly weekStart)
    {
        var weekEnd = weekStart.AddDays(7);

        var occupied = allSlots
            .Where(s => s.Status != SlotStatus.Available
                        && DateOnly.FromDateTime(s.StartDateTime) >= weekStart
                        && DateOnly.FromDateTime(s.StartDateTime) < weekEnd)
            .Select(s => new TimeBlock
            {
                Date = DateOnly.FromDateTime(s.StartDateTime),
                Start = s.StartDateTime.TimeOfDay,
                End = s.EndDateTime.TimeOfDay
            })
            .ToList();
        
        if (occupied.Count == 0)
        {
            return GetFreeWeek(opening, closing, weekStart);
        }

        var occupiedByDate = occupied.GroupBy(o => o.Date).ToDictionary(k => k.Key, v => v.ToList());

        var availableBlocks = AssembleAvailableBlocks(opening, closing, occupiedByDate);
        
        AddFreeDays(opening, closing, weekStart, occupiedByDate, availableBlocks);
        
        return availableBlocks
            .OrderBy(b => b.Date)
            .ThenBy(b => b.Start)
            .ToList();
    }

    /// <summary>
    /// Builds available time blocks for days that contain occupied slots.
    /// </summary>
    /// <param name="opening">The shelter’s opening time.</param>
    /// <param name="closing">The shelter’s closing time.</param>
    /// <param name="occupiedByDate">A dictionary grouping occupied time blocks by date.</param>
    /// <returns>
    /// A list of <see cref="TimeBlock"/> entries representing all available gaps between occupied periods.
    /// </returns>
    /// <remarks>
    /// Iterates through each day’s occupied slots, identifying gaps between consecutive time blocks
    /// and adding them as available intervals.  
    /// Ensures that time blocks at the start or end of the day are also captured.
    /// </remarks>
    private static List<TimeBlock> AssembleAvailableBlocks(TimeSpan opening, TimeSpan closing, Dictionary<DateOnly, List<TimeBlock>> occupiedByDate)
    {
        var available = new List<TimeBlock>();
        
        foreach (var group in occupiedByDate)
        {
            var current = opening;

            foreach (var block in group.Value
                         .Where(block => block.End > opening)
                         .TakeWhile(block => block.Start < closing)
                         .OrderBy(b => b.Start))
            {
                // Se houver espaço entre o último fim e o próximo início → intervalo livre
                if (block.Start > current)
                {
                    available.Add(new TimeBlock
                    {
                        Date = group.Key,
                        Start = current,
                        End = block.Start
                    });
                }

                // Avança o tempo
                current = block.End > current ? block.End : current;
            }

            if (current < closing)
            {
                available.Add(new TimeBlock
                {
                    Date = group.Key,
                    Start = current,
                    End = closing
                });
            }
        }

        return available;
    }

    /// <summary>
    /// Adds fully free days to the weekly availability list when no occupied slots exist.
    /// </summary>
    /// <param name="opening">The shelter’s opening time.</param>
    /// <param name="closing">The shelter’s closing time.</param>
    /// <param name="weekStart">The start date of the week.</param>
    /// <param name="occupiedByDate">A dictionary of occupied slots grouped by date.</param>
    /// <param name="available">The list of currently available time blocks to be updated.</param>
    /// <remarks>
    /// For each day in the week range, this method checks whether any slots exist.  
    /// If none are found, it adds a full-day availability block from opening to closing time.
    /// </remarks>
    private static void AddFreeDays(TimeSpan opening, TimeSpan closing, DateOnly weekStart, Dictionary<DateOnly, List<TimeBlock>> occupiedByDate,
        List<TimeBlock> available)
    {
        for (var i = 0 ; i < 7; i++)
        {
            var currDate = weekStart.AddDays(i);
            if (!occupiedByDate.ContainsKey(currDate))
            {
                available.Add(new TimeBlock
                {
                    Date = currDate,
                    Start = opening,
                    End = closing
                });
            }
        }
    }

    /// <summary>
    /// Generates a full week of available time blocks when no occupied slots are found.
    /// </summary>
    /// <param name="opening">The shelter’s opening time.</param>
    /// <param name="closing">The shelter’s closing time.</param>
    /// <param name="weekStart">The start date of the week.</param>
    /// <returns>
    /// A list of <see cref="TimeBlock"/> entries representing seven fully available days.
    /// </returns>
    /// <remarks>
    /// Each day of the week is assigned a time block spanning from <paramref name="opening"/> to <paramref name="closing"/>.
    /// </remarks>
    private static List<TimeBlock> GetFreeWeek(TimeSpan opening, TimeSpan closing, DateOnly weekStart)
    {
        var availableWeek = new List<TimeBlock>();

        for (var i = 0; i < 7; i++)
        {
            availableWeek.Add(new TimeBlock
            {
                Date = weekStart.AddDays(i),
                Start = opening,
                End = closing
            });
        }

        return availableWeek;
    }
}