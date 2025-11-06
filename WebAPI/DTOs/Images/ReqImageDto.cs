namespace WebAPI.DTOs.Images
{
    /// <summary>
    /// Data Transfer Object (DTO) representing an image associated with an animal.
    /// Used when creating or updating animal records.
    /// </summary>
    public class ReqImageDto
    {
        /// <summary>
        /// Image file to be uploaded and associated with the animal.
        /// </summary>
        public required IFormFile File { get; set; }

        /// <summary>
        /// Optional textual description providing context or details about the image.
        /// </summary>
        public string? Description { get; set; }
    }
}
