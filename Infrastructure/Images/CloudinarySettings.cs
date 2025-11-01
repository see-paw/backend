namespace Infrastructure.Images;

/// <summary>
/// Holds configuration settings required to connect to the Cloudinary service.
/// </summary>
public class CloudinarySettings
{
    /// <summary>
    /// The Cloudinary account name.
    /// </summary>
    public required string CloudName { get; set; }

    /// <summary>
    /// The Cloudinary API key used for authentication.
    /// </summary>
    public required string ApiKey { get; set; }

    /// <summary>
    /// The Cloudinary API secret used for secure access.
    /// </summary>
    public required string ApiSecret { get; set; }
}