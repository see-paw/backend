
using Application.Interfaces;
using Application.Scheduling;
using Domain;

namespace Application.Services;

/// <summary>
/// Provides functionality to normalize and adjust scheduling slots to fit within shelter operating hours.
/// </summary>
/// <remarks>
/// Implements <see cref="ISlotNormalizer"/> and is responsible for transforming raw slot data into
/// a consistent, day-bound representation that respects shelter opening and closing times.  
/// It splits multi-day slots, trims overlapping times, and filters out invalid or out-of-hours segments.
/// </remarks>
public class SlotNormalizer : ISlotNormalizer
{
    /// <summary>
    /// Normalizes a collection of slots so that they fit within defined shelter hours.
    /// </summary>
    /// <param name="slots">The collection of slots to normalize.</param>
    /// <param name="opening">The daily opening time of the shelter.</param>
    /// <param name="closing">The daily closing time of the shelter.</param>
    /// <returns>
    /// A <see cref="NormalizedSlots"/> object containing all valid and time-adjusted slots 
    /// within the provided opening and closing hours.
    /// </returns>
    /// <remarks>
    /// This method performs the following:
    /// <list type="number">
    /// <item>Splits slots that span multiple days into separate daily segments.</item>
    /// <item>Filters out slots that fall completely outside shelter hours.</item>
    /// <item>Clamps start and end times to match the shelter’s operational window.</item>
    /// <item>Removes invalid results where start and end times overlap incorrectly.</item>
    /// <item>Orders results chronologically by date and time of day.</item>
    /// </list>
    /// </remarks>
    public NormalizedSlots Normalize(
        IEnumerable<Slot> slots, 
        TimeSpan opening, 
        TimeSpan closing)
    {
        return new NormalizedSlots(slots
            .SelectMany(SplitMultiDaySlot)
            .Where(s => HasOverlapWithShelterHours(s, opening, closing))
            .Select(s => ClampSlot(s, opening, closing))
            .Where(IsValidAfterClamp)
            .OrderBy(s => DateOnly.FromDateTime(s.StartDateTime))
            .ThenBy(s => s.StartDateTime.TimeOfDay)
            .ToList());
    }

    /// <summary>
    /// Splits a multi-day slot into one or more single-day segments.
    /// </summary>
    /// <param name="slot">The original slot that may span multiple days.</param>
    /// <returns>
    /// A sequence of <see cref="Slot"/> objects, each contained within a single calendar day.
    /// </returns>
    /// <remarks>
    /// If the slot’s start and end dates are the same, it is returned unchanged.  
    /// Otherwise, it generates a new slot per day until the end date is reached.
    /// </remarks>
    private IEnumerable<Slot> SplitMultiDaySlot(Slot slot)
    {
        var startDate = DateOnly.FromDateTime(slot.StartDateTime);
        var endDate = DateOnly.FromDateTime(slot.EndDateTime);
        
        //slot unico dia
        if (startDate == endDate)
        {
            yield return slot;
            yield break;
        }
        
        // Split em múltiplos dias
        var currentDate = startDate;
        var currentStart = slot.StartDateTime;
        
        while (currentDate <= endDate)
        {
            var endOfDay = currentDate.ToDateTime(TimeOnly.MaxValue);
            var slotEnd = currentDate == endDate 
                ? slot.EndDateTime 
                : endOfDay;
            
            yield return CreateSplitSlot(slot, currentStart, slotEnd);
            
            // Próximo dia começa à meia-noite
            currentDate = currentDate.AddDays(1);
            currentStart = currentDate.ToDateTime(TimeOnly.MinValue);
        }
    }

    /// <summary>
    /// Creates a new slot instance from an existing slot with updated start and end times.
    /// </summary>
    /// <param name="original">The original slot from which data will be copied.</param>
    /// <param name="start">The new start time for the split or clamped segment.</param>
    /// <param name="end">The new end time for the split or clamped segment.</param>
    /// <returns>
    /// A new <see cref="Slot"/> instance of the same type as the original slot.
    /// </returns>
    /// <exception cref="NotSupportedException">Thrown if the slot type is not supported.</exception>
    private static Slot CreateSplitSlot(Slot original, DateTime start, DateTime end)
    {
        return original switch
        {
            ActivitySlot activitySlot => new ActivitySlot
            {
                Id = Guid.NewGuid().ToString(), // novo ID para split
                ActivityId = activitySlot.ActivityId,
                Activity = activitySlot.Activity,
                StartDateTime = start,
                EndDateTime = end,
                Status = activitySlot.Status,
                Type = activitySlot.Type,
                CreatedAt = activitySlot.CreatedAt,
                UpdatedAt = activitySlot.UpdatedAt
            },
            
            ShelterUnavailabilitySlot unavailabilitySlot => new ShelterUnavailabilitySlot
            {
                Id = Guid.NewGuid().ToString(), 
                ShelterId = unavailabilitySlot.ShelterId,
                Reason = unavailabilitySlot.Reason,
                Shelter = unavailabilitySlot.Shelter,
                StartDateTime = start,
                EndDateTime = end,
                Status = unavailabilitySlot.Status,
                Type = unavailabilitySlot.Type,
                CreatedAt = unavailabilitySlot.CreatedAt,
                UpdatedAt = unavailabilitySlot.UpdatedAt
            },
            
            _ => throw new NotSupportedException($"Slot type {original.GetType().Name} not supported")
        };
    }

    /// <summary>
    /// Determines whether a slot overlaps with the defined shelter opening hours.
    /// </summary>
    /// <param name="slot">The slot to evaluate.</param>
    /// <param name="opening">The daily opening time of the shelter.</param>
    /// <param name="closing">The daily closing time of the shelter.</param>
    /// <returns>
    /// <c>true</c> if the slot overlaps with the shelter’s operational window; otherwise, <c>false</c>.
    /// </returns>
    private static bool HasOverlapWithShelterHours(Slot slot, TimeSpan opening, TimeSpan closing)
    {
        var start = slot.StartDateTime.TimeOfDay;
        var end = slot.EndDateTime.TimeOfDay;
        
        return start < closing && end > opening;
    }

    /// <summary>
    /// Checks if the slot remains valid after being clamped.
    /// </summary>
    /// <param name="slot">The slot to validate.</param>
    /// <returns>
    /// <c>true</c> if the slot’s <see cref="Slot.StartDateTime"/> is earlier than its <see cref="Slot.EndDateTime"/>; otherwise, <c>false</c>.
    /// </returns>
    private static bool IsValidAfterClamp(Slot slot)
    {
        return slot.EndDateTime > slot.StartDateTime;
    }

    /// <summary>
    /// Adjusts a slot’s start and end times to ensure they fall within the shelter’s daily operating window.
    /// </summary>
    /// <param name="slot">The slot to adjust.</param>
    /// <param name="opening">The shelter’s opening time.</param>
    /// <param name="closing">The shelter’s closing time.</param>
    /// <returns>
    /// A new <see cref="Slot"/> instance with start and end times clamped to valid operational hours.
    /// </returns>
    /// <exception cref="NotSupportedException">Thrown if the slot type is not supported.</exception>
    private static Slot ClampSlot(Slot slot, TimeSpan opening, TimeSpan closing)
    {
        var date = DateOnly.FromDateTime(slot.StartDateTime);
        var start = slot.StartDateTime.TimeOfDay;
        var end = slot.EndDateTime.TimeOfDay;
        
        var clampedStart = TimeSpan.FromTicks(Math.Max(start.Ticks, opening.Ticks));
        var clampedEnd = TimeSpan.FromTicks(Math.Min(end.Ticks, closing.Ticks));
        
        return slot switch
        {
            ActivitySlot activitySlot => new ActivitySlot
            {
                Id = activitySlot.Id,
                ActivityId = activitySlot.ActivityId,
                Activity = activitySlot.Activity,
                StartDateTime = date.ToDateTime(TimeOnly.FromTimeSpan(clampedStart)),
                EndDateTime = date.ToDateTime(TimeOnly.FromTimeSpan(clampedEnd)),
                Status = activitySlot.Status,
                Type = activitySlot.Type,
                CreatedAt = activitySlot.CreatedAt,
                UpdatedAt = activitySlot.UpdatedAt
            },
            
            ShelterUnavailabilitySlot unavailabilitySlot => new ShelterUnavailabilitySlot
            {
                Id = unavailabilitySlot.Id,
                ShelterId = unavailabilitySlot.ShelterId,
                Reason = unavailabilitySlot.Reason,
                Shelter = unavailabilitySlot.Shelter,
                StartDateTime = date.ToDateTime(TimeOnly.FromTimeSpan(clampedStart)),
                EndDateTime = date.ToDateTime(TimeOnly.FromTimeSpan(clampedEnd)),
                Status = unavailabilitySlot.Status,
                Type = unavailabilitySlot.Type,
                CreatedAt = unavailabilitySlot.CreatedAt,
                UpdatedAt = unavailabilitySlot.UpdatedAt
            },
            
            _ => throw new NotSupportedException($"Slot type {slot.GetType().Name} not supported")
        };
    }
}