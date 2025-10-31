namespace WebAPI.DTOs.Breeds
{
    /// <summary>
    /// Data Transfer Object (DTO) representing the breed information of an animal.
    /// Used in responses to provide details about the animal's breed.
    /// </summary>
    public class ResBreedDto
    {
        /// <summary>
        /// The unique identifier of the breed.
        /// </summary>
        public string Id { get; set; } = null!;

        /// <summary>
        /// The name of the breed (e.g., Golden Retriever, Siamese).
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// An optional textual description providing details about the breed.
        /// </summary>
        public string? Description { get; set; }
    }
}
