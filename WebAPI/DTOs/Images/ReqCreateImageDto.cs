namespace WebAPI.DTOs.Images;

/// <summary>
/// DTO for images during entity creation.
/// Includes IsPrincipal to allow specifying the initial principal image.
/// </summary>
public class ReqCreateImageDto :  ReqImageDto
{
    public required bool IsPrincipal { get; set; }
}