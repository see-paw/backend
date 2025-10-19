namespace WebAPI.DTOs
{
    /// <summary>
    /// Data Transfer Object (DTO) representing an image associated with an animal.
    /// Returned in API responses to provide image details and metadata.
    /// </summary>
    public class ResImageDto
    {
        /// <summary>
        /// The unique identifier of the image.
        /// </summary>
        public string ImageId { get; set; } = null!;

        /// <summary>
        /// Indicates whether this image is the principal (main) image of the animal.
        /// </summary>
        public bool isPrincipal { get; set; }

        /// <summary>
        /// The URL where the image is stored or can be accessed.
        /// </summary>
        public string Url { get; set; } = null!;

        /// <summary>
        /// Optional textual description providing additional context about the image.
        /// </summary>
        public string? Description { get; set; }
    }
}
