namespace WebAPI.DTOs
{
    /// <summary>
    /// Data Transfer Object (DTO) representing an image associated with an animal.
    /// Used when creating or updating animal records.
    /// </summary>
    public class ReqImageDto
    {
        /// <summary>
        /// Indicates whether this image is the animal's main (principal) image.
        /// </summary>
        public bool isPrincipal { get; set; }

        /// <summary>
        /// The URL where the image is stored or accessible.
        /// </summary>
        public string Url { get; set; } = null!;

        /// <summary>
        /// Optional textual description providing context or details about the image.
        /// </summary>
        public string? Description { get; set; }
    }
}
