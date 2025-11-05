using System.ComponentModel.DataAnnotations;

namespace WebAPI.DTOs.Activities;

/// <summary>
/// Data transfer object for scheduling a visit slot for a fostered animal.
/// </summary>
/// <remarks>
/// This DTO is used when a foster user wants to schedule a visit with an animal they are fostering.
/// The UserId is obtained from the authenticated context and not included in this DTO.
/// </remarks>
public class ReqCreateActivityFosteringDto
{
    // <summary>
    /// The unique identifier of the animal to visit.
    /// </summary>
    [Required(ErrorMessage = "AnimalId is required.")]
    [MaxLength(36, ErrorMessage = "AnimalId cannot exceed 36 characters.")]
    [RegularExpression(@"^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$", 
        ErrorMessage = "AnimalId must be a valid GUID.")]
    public string AnimalId { get; set; } = string.Empty;

    /// <summary>
    /// The start date and time of the visit.
    /// </summary>
    [Required(ErrorMessage = "StartDateTime is required.")]
    public DateTime StartDateTime { get; set; }

    /// <summary>
    /// The end date and time of the visit.
    /// </summary>
    [Required(ErrorMessage = "EndDateTime is required.")]
    public DateTime EndDateTime { get; set; }
}