using System.ComponentModel.DataAnnotations;

namespace WebAPI.DTOs.Activities;

/// <summary>
/// Data transfer object for cancelling a fostering activity visit.
/// </summary>
public class ReqCancelActivityFosteringDto
{
    /// <summary>
    /// The unique identifier of the activity to cancel.
    /// </summary>
    [Required(ErrorMessage = "ActivityId is required.")]
    [MaxLength(36, ErrorMessage = "ActivityId cannot exceed 36 characters.")]
    [RegularExpression(@"^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$",
        ErrorMessage = "ActivityId must be a valid GUID.")]
    public string ActivityId { get; set; } = string.Empty;
}
