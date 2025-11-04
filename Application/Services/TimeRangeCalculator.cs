using Application.Interfaces;
using Application.Scheduling;
using Domain;
using Domain.Enums;

namespace Application.Services;

public class TimeRangeCalculator : ITimeRangeCalculator
{
    public IReadOnlyList<TimeBlock> CalculateWeeklyAvailableRanges(
        IEnumerable<Slot> allSlots, 
        TimeSpan opening, 
        TimeSpan closing,
        DateOnly weekStart)
    {
        var occupied = allSlots
            .Where(s => s.Status != SlotStatus.Available)
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