namespace WebAPI.DTOs.AnimalSchedule;

public class ResWeeklyAnimalScheduleDto
{
    public required string AnimalId { get; set; }
    
    public required string AnimalName { get; set; }
    
    public required string ShelterId { get; set; }
    
    public required string ShelterName { get; set; }
    
    public Dictionary<DateOnly,List<SlotDto>> DailySchedules { get; set; } = new();
}