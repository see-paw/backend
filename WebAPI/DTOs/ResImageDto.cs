namespace WebAPI.DTOs;

/// <summary>
/// Data Transfer Object (DTO) representing an image associated with an animal or shelter.
/// </summary>
/// <remarks>
/// Used to transfer image data from the backend to API responses, including the image
/// identifier, whether it is the main image, its URL, and an optional description.
/// </remarks>
public class ResImageDto
{
    public required string Id { get; init; }
    
    public required string PublicId { get; init; }
    public bool IsPrincipal { get; set; }
    public string Url { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
