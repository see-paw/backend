using Application.Interfaces;
using Domain;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Services;

/// <summary>
/// Service responsible for managing and generating daily schedules for shelters.
/// </summary>
/// <remarks>
/// This service generates complete daily schedules by:
/// - Retrieving existing slots from the database
/// - Creating unavailable slots for hours outside shelter operating hours
/// - Filling gaps with available slots based on configured duration
/// All slots are ordered chronologically and aligned with shelter business hours.
/// </remarks>
public class SchedulingService(
    AppDbContext dbContext,
    SchedulingSettings schedulingSettings) : ISchedulingService
{
    /// <summary>
    /// Retrieves or generates a complete daily schedule for a specific shelter.
    /// </summary>
    /// <param name="shelterId">The unique identifier of the shelter.</param>
    /// <param name="date">The date for which to generate the schedule.</param>
    /// <param name="ct">Cancellation token for async operation.</param>
    /// <returns>A complete list of slots for the entire day, ordered chronologically.</returns>
    /// <exception cref="ArgumentException">Thrown when shelterId is null or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown when shelter is not found or has invalid hours.</exception>
    public async Task<IEnumerable<Slot>> GetDailyScheduleAsync(string shelterId, DateOnly date, CancellationToken ct)
    {
        ValidateInputs(shelterId);

        var startOfDay = date.ToDateTime(TimeOnly.MinValue);
        var endOfDay = date.ToDateTime(TimeOnly.MinValue).AddDays(1);

        var slots = await dbContext.Slots
            .AsNoTracking()
            .Where(slot =>
                slot.ShelterId == shelterId &&
                slot.StartDateTime >= startOfDay &&
                slot.StartDateTime < endOfDay)
            .OrderBy(slot => slot.StartDateTime)
            .ToListAsync(ct);

        return await CreateDailySchedule(shelterId, slots, startOfDay, endOfDay, ct);
    }

    /// <summary>
    /// Retrieves or generates complete schedules for a week starting from a specific date.
    /// </summary>
    /// <param name="shelterId">The unique identifier of the shelter.</param>
    /// <param name="startDate">The first day of the week to generate schedules for.</param>
    /// <param name="ct">Cancellation token for async operation.</param>
    /// <returns>A dictionary mapping each date to its complete daily schedule.</returns>
    /// <exception cref="ArgumentException">Thrown when shelterId is null or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown when shelter is not found or has invalid hours.</exception>
    public async Task<Dictionary<DateOnly, IEnumerable<Slot>>> GetWeeklyScheduleAsync(
        string shelterId,
        DateOnly startDate,
        CancellationToken ct)
    {
        ValidateInputs(shelterId);

        var weeklySchedule = new Dictionary<DateOnly, IEnumerable<Slot>>();

        for (var date = startDate; date < startDate.AddDays(7); date = date.AddDays(1))
        {
            var daySchedule = await GetDailyScheduleAsync(shelterId, date, ct);
            weeklySchedule[date] = daySchedule;
        }

        return weeklySchedule;
    }

    /// <summary>
    /// Validates input parameters for schedule generation.
    /// </summary>
    /// <param name="shelterId">The shelter identifier to validate.</param>
    /// <exception cref="ArgumentException">Thrown when shelterId is null or empty.</exception>
    private static void ValidateInputs(string shelterId)
    {
        if (string.IsNullOrWhiteSpace(shelterId))
        {
            throw new ArgumentException("Shelter ID cannot be null or empty.", nameof(shelterId));
        }
    }

    /// <summary>
    /// Generates slots for periods when the shelter is closed (before opening and after closing).
    /// </summary>
    /// <param name="shelterId">The shelter identifier.</param>
    /// <param name="startOfDay">The start of the day (00:00:00).</param>
    /// <param name="endOfDay">The end of the day (00:00:00 next day).</param>
    /// <param name="duration">The duration of each slot.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>List of unavailable slots for off-hours periods.</returns>
    /// <exception cref="InvalidOperationException">Thrown when shelter is not found or has invalid hours.</exception>
    private async Task<List<Slot>> GetOffHoursShelterSlots(
        string shelterId,
        DateTime startOfDay,
        DateTime endOfDay,
        TimeSpan duration,
        CancellationToken ct)
    {
        var shelter = await dbContext.Shelters.FindAsync([shelterId], ct);

        if (shelter == null)
        {
            throw new InvalidOperationException($"Shelter with ID '{shelterId}' was not found.");
        }

        if (shelter.OpeningTime >= shelter.ClosingTime)
        {
            throw new InvalidOperationException(
                $"Shelter '{shelterId}' has invalid hours: opening time ({shelter.OpeningTime}) must be before closing time ({shelter.ClosingTime}).");
        }

        var openingDateTime = startOfDay.Date.Add(shelter.OpeningTime.ToTimeSpan());
        var closingDateTime = startOfDay.Date.Add(shelter.ClosingTime.ToTimeSpan());

        var offHoursShelterSlots = new List<Slot>();

        for (var currTime = startOfDay; currTime < openingDateTime; currTime += duration)
        {
            offHoursShelterSlots.Add(CreateUnavailableSlot(shelterId, currTime, duration));
        }

        for (var currTime = closingDateTime; currTime < endOfDay; currTime += duration)
        {
            offHoursShelterSlots.Add(CreateUnavailableSlot(shelterId, currTime, duration));
        }

        return offHoursShelterSlots;
    }

    /// <summary>
    /// Creates a complete daily schedule by combining existing, off-hours, and generated available slots.
    /// </summary>
    /// <param name="shelterId">The shelter identifier.</param>
    /// <param name="existingSlots">Slots already in the database.</param>
    /// <param name="startOfDay">The start of the day.</param>
    /// <param name="endOfDay">The end of the day.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Complete daily schedule with all slots filled.</returns>
    private async Task<IEnumerable<Slot>> CreateDailySchedule(
        string shelterId,
        List<Slot> existingSlots,
        DateTime startOfDay,
        DateTime endOfDay,
        CancellationToken ct)
    {
        var duration = GetValidatedSlotDuration();
        var offHoursSlots = await GetOffHoursShelterSlots(shelterId, startOfDay, endOfDay, duration, ct);

        var slotMap = existingSlots
            .Concat(offHoursSlots)
            .ToDictionary(s => s.StartDateTime, s => s);

        var dailySlots = new List<Slot>();

        for (var currTime = startOfDay; currTime < endOfDay; currTime += duration)
        {
            if (slotMap.TryGetValue(currTime, out var existingSlot))
            {
                dailySlots.Add(existingSlot);
            }
            else
            {
                dailySlots.Add(CreateAvailableSlot(shelterId, currTime, duration));
            }
        }

        return dailySlots;
    }

    /// <summary>
    /// Validates and retrieves the configured slot duration.
    /// </summary>
    /// <returns>TimeSpan representing the slot duration.</returns>
    /// <exception cref="InvalidOperationException">Thrown when slot duration is invalid.</exception>
    private TimeSpan GetValidatedSlotDuration()
    {
        if (schedulingSettings.SlotDurationMinutes <= 0)
        {
            throw new InvalidOperationException(
                $"Slot duration must be greater than 0. Current value: {schedulingSettings.SlotDurationMinutes}");
        }

        if (schedulingSettings.SlotDurationMinutes > 1440)
        {
            throw new InvalidOperationException(
                $"Slot duration cannot exceed 24 hours (1440 minutes). Current value: {schedulingSettings.SlotDurationMinutes}");
        }

        return TimeSpan.FromMinutes(schedulingSettings.SlotDurationMinutes);
    }

    /// <summary>
    /// Creates a new unavailable slot for off-hours periods.
    /// </summary>
    /// <param name="shelterId">The shelter identifier.</param>
    /// <param name="startTime">The start time of the slot.</param>
    /// <param name="duration">The duration of the slot.</param>
    /// <returns>A new slot marked as unavailable.</returns>
    private static Slot CreateUnavailableSlot(string shelterId, DateTime startTime, TimeSpan duration)
    {
        return new Slot
        {
            Id = Guid.NewGuid().ToString(),
            ShelterId = shelterId,
            StartDateTime = startTime,
            EndDateTime = startTime.Add(duration),
            Status = SlotStatus.Unavailable,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a new available slot for open periods without existing reservations.
    /// </summary>
    /// <param name="shelterId">The shelter identifier.</param>
    /// <param name="startTime">The start time of the slot.</param>
    /// <param name="duration">The duration of the slot.</param>
    /// <returns>A new slot marked as available.</returns>
    private static Slot CreateAvailableSlot(string shelterId, DateTime startTime, TimeSpan duration)
    {
        return new Slot
        {
            Id = Guid.NewGuid().ToString(),
            ShelterId = shelterId,
            StartDateTime = startTime,
            EndDateTime = startTime.Add(duration),
            Status = SlotStatus.Available,
            CreatedAt = DateTime.UtcNow
        };
    }
}