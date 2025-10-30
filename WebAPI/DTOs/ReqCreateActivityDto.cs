namespace WebAPI.DTOs;

/// <summary>
/// Request DTO for creating a new ownership activity.
/// </summary>
public class ReqCreateActivityDto
{
    /// <summary>
    /// The unique identifier of the animal for the activity.
    /// </summary>
    public string AnimalId { get; set; } = string.Empty;

    /// <summary>
    /// The start date and time of the activity.
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// The end date and time of the activity.
    /// </summary>
    public DateTime EndDate { get; set; }
}