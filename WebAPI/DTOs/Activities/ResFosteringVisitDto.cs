namespace WebAPI.DTOs.Activities;

/// <summary>
/// Represents a fostering visit slot for a user
/// </summary>
public class ResFosteringVisitDto
{
    /// <summary>
    /// The unique identifier of the activity
    /// </summary>
    public string ActivityId { get; set; } = string.Empty;

    /// <summary>
    /// The name of the animal being visited
    /// </summary>
    public string AnimalName { get; set; } = string.Empty;

    /// <summary>
    /// URL of the animal's principal image
    /// </summary>
    public string AnimalPrincipalImageUrl { get; set; } = string.Empty;

    /// <summary>
    /// The breed name of the animal
    /// </summary>
    public string BreedName { get; set; } = string.Empty;

    /// <summary>
    /// The age of the animal in years
    /// </summary>
    public int AnimalAge { get; set; }

    /// <summary>
    /// The name of the shelter where the animal is located
    /// </summary>
    public string ShelterName { get; set; } = string.Empty;

    /// <summary>
    /// The complete address of the shelter (Street, PostalCode City)
    /// </summary>
    public string ShelterAddress { get; set; } = string.Empty;

    /// <summary>
    /// The opening time of the shelter
    /// </summary>
    public TimeOnly ShelterOpeningTime { get; set; }

    /// <summary>
    /// The closing time of the shelter
    /// </summary>
    public TimeOnly ShelterClosingTime { get; set; }

    /// <summary>
    /// The start date and time of the visit
    /// </summary>
    public DateTime VisitStartDateTime { get; set; }

    /// <summary>
    /// The end date and time of the visit
    /// </summary>
    public DateTime VisitEndDateTime { get; set; }

    /// <summary>
    /// The date of the visit (extracted from VisitStartDateTime)
    /// </summary>
    public DateOnly VisitDate { get; set; }
}