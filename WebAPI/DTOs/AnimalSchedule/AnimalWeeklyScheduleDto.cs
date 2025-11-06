using WebAPI.DTOs.Animals;
using WebAPI.DTOs.Shelter;

namespace WebAPI.DTOs.AnimalSchedule;

public class AnimalWeeklyScheduleDto
{
    public ResAnimalDto Animal { get; set; } = null!;
    public ResShelterDto Shelter { get; set; } = null!;
    public string StartDate { get; set; } = null!;
    public List<DailyScheduleDto> Days { get; set; } = new();
}