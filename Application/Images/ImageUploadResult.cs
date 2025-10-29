namespace Application.Images;

/// <summary>
/// Represents the result of an image upload.
/// </summary>
public class ImageUploadResult
{
    /// <summary>
    /// The public identifier of the uploaded image in the cloud service.
    /// </summary>
    public required string PublicId { get; set; }

    /// <summary>
    /// The secure URL of the uploaded image.
    /// </summary>
    public required string Url { get; set; }
}