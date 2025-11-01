using Domain.Enums;

namespace WebAPI.DTOs.Activities;

/// <summary>
/// Response DTO for activity data.
/// </summary>
public class ResActivityDto
{
    public string Id { get; set; } = string.Empty;
    public string AnimalId { get; set; } = string.Empty;
    public string AnimalName { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public ActivityType Type { get; set; }
    public ActivityStatus Status { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime CreatedAt { get; set; }
}