namespace WebAPI.DTOs.Fostering;

/// <summary>
/// Data transfer object used to request the creation of a new fostering agreement for an animal.
/// </summary>
public class ReqAddFosteringDto
{
    /// <summary>
    /// Monthly monetary contribution value for the fostering.
    /// </summary>
    public required decimal MonthValue { get; set; }
}
