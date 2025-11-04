
using Application.Interfaces;
using Application.Scheduling;
using Domain;

namespace Application.Services;

public class SlotNormalizer : ISlotNormalizer
{
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

    private static bool HasOverlapWithShelterHours(Slot slot, TimeSpan opening, TimeSpan closing)
    {
        var start = slot.StartDateTime.TimeOfDay;
        var end = slot.EndDateTime.TimeOfDay;
        
        return start < closing && end > opening;
    }

    private static bool IsValidAfterClamp(Slot slot)
    {
        return slot.EndDateTime > slot.StartDateTime;
    }

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