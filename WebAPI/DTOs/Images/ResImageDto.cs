namespace WebAPI.DTOs.Images;

/// <summary>
/// Data Transfer Object (DTO) representing an image associated with an animal or shelter.
/// </summary>
/// <remarks>
/// Used to transfer image data from the backend to API responses, including the image
/// identifier, whether it is the main image, its URL, and an optional description.
/// </remarks>
public class ResImageDto
{
    /// <summary>
    /// Unique identifier of the image.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Public identifier of the image in the external storage service (e.g., Cloudinary).
    /// </summary>
    public required string PublicId { get; init; }

    /// <summary>
    /// Indicates whether the image is the main (principal) image.
    /// </summary>
    public bool IsPrincipal { get; set; }

    /// <summary>
    /// URL where the image can be accessed.
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Optional description or caption for the image.
    /// </summary>
    public string Description { get; set; } = string.Empty;

}
