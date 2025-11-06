using WebAPI.DTOs.Animals;
using WebAPI.DTOs.Shelter;

namespace WebAPI.DTOs.AnimalSchedule;

/// <summary>
/// Data transfer object representing an animal’s weekly schedule,
/// including its associated shelter and daily availability details.
/// </summary>
public class AnimalWeeklyScheduleDto
{
    /// <summary>
    /// The animal to which the schedule belongs.
    /// </summary>
    public ResAnimalDto Animal { get; set; } = null!;

    /// <summary>
    /// The shelter responsible for the animal.
    /// </summary>
    public ResShelterDto Shelter { get; set; } = null!;

    /// <summary>
    /// The starting date of the represented week (in ISO string format).
    /// </summary>
    public string StartDate { get; set; } = null!;

    /// <summary>
    /// List containing each day’s schedule, including available and reserved slots.
    /// </summary>
    public List<DailyScheduleDto> Days { get; set; } = new();
}
