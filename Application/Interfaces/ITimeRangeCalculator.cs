using Application.Scheduling;
using Domain;

namespace Application.Interfaces;

public interface ITimeRangeCalculator
{
    public IReadOnlyList<TimeBlock> CalculateWeeklyAvailableRanges(
        IEnumerable<Slot> occupied,
        TimeSpan opening,
        TimeSpan closing,
        DateOnly weekStart);
}