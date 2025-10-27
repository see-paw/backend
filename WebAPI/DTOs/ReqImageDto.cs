namespace WebAPI.DTOs
{
    /// <summary>
    /// Data Transfer Object (DTO) representing an image associated with an animal.
    /// Used when creating or updating animal records.
    /// </summary>
    public class ReqImageDto
    {
        public required IFormFile File { get; set; }
        /// <summary>
        /// Indicates whether this image is the animal's main (principal) image.
        /// </summary>
        public bool isPrincipal { get; set; }

        /// <summary>
        /// Optional textual description providing context or details about the image.
        /// </summary>
        public string? Description { get; set; }
    }
}
