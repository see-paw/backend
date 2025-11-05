namespace WebAPI.DTOs.AnimalSchedule;

public class DailyScheduleDto
{
    public string Date { get; init; } = default!;
    public List<SlotDto> AvailableSlots { get; init; } = new();
    public List<ActivitySlotDto> ReservedSlots { get; init; } = new();
    public List<ShelterUnavailabilitySlotDto> UnavailableSlots { get; init; } = new();
}