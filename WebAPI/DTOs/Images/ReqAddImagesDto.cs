namespace WebAPI.DTOs.Images;

/// <summary>
/// Represents a request to add multiple images to an entity.
/// </summary>
public class ReqAddImagesDto
{
    /// <summary>
    /// The collection of images to be added.
    /// </summary>
    public List<ReqImageDto> Images { get; set; } = new();
}
