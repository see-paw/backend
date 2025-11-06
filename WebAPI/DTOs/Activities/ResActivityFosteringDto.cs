namespace WebAPI.DTOs.Activities;

/// <summary>
/// Data transfer object returned after successfully scheduling a visit slot.
/// </summary>
/// <remarks>
/// Contains all relevant information about the scheduled visit, including details
/// about the animal, shelter, and the created activity and slot.
/// </remarks>
public class ResActivityFosteringDto
{
    /// <summary>
    /// The unique identifier of the created activity slot.
    /// </summary>
    public string ActivitySlotId { get; set; } = string.Empty;

    /// <summary>
    /// The unique identifier of the created activity.
    /// </summary>
    public string ActivityId { get; set; } = string.Empty;

    /// <summary>
    /// The start date and time of the scheduled visit.
    /// </summary>
    public DateTime StartDateTime { get; set; }

    /// <summary>
    /// The end date and time of the scheduled visit.
    /// </summary>
    public DateTime EndDateTime { get; set; }

    /// <summary>
    /// Information about the animal being visited.
    /// </summary>
    public AnimalVisitInfoDto Animal { get; set; } = null!;

    /// <summary>
    /// Information about the shelter where the visit will take place.
    /// </summary>
    public ShelterVisitInfoDto Shelter { get; set; } = null!;

    /// <summary>
    /// Success message confirming the visit was scheduled.
    /// </summary>
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Contains basic information about the animal for the visit response.
/// </summary>
public class AnimalVisitInfoDto
{
    /// <summary>
    /// The unique identifier of the animal.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The name of the animal.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The URL of the principal image of the animal.
    /// </summary>
    public string? PrincipalImageUrl { get; set; }
}

/// <summary>
/// Contains information about the shelter for the visit response.
/// </summary>
public class ShelterVisitInfoDto
{
    /// <summary>
    /// The name of the shelter.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The complete address of the shelter.
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// The opening time of the shelter.
    /// </summary>
    public TimeOnly OpeningTime { get; set; }

    /// <summary>
    /// The closing time of the shelter.
    /// </summary>
    public TimeOnly ClosingTime { get; set; }
}