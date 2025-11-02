using Domain;

namespace Application.Interfaces;

public interface ISchedulingService
{
    Task<IEnumerable<Slot>> GetDailyScheduleAsync(string shelterId, DateOnly date, CancellationToken ct);
    Task<Dictionary<DateOnly, IEnumerable<Slot>>> GetWeeklyScheduleAsync(
        string shelterId, 
        DateOnly startDate, 
        CancellationToken ct);
}