namespace WebAPI.DTOs.Images;

/// <summary>
/// DTO for images during entity creation.
/// Includes IsPrincipal to allow specifying the initial principal image.
/// </summary>
public class ReqCreateImageDto : ReqImageDto
{
    /// <summary>
    /// Indicates whether the image should be set as the entity’s main (principal) image.
    /// </summary>
    public required bool IsPrincipal { get; set; }
}
