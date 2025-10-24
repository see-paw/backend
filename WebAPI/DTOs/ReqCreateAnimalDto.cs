namespace WebAPI.DTOs
{
    using Domain;
    using Domain.Enums;

    /// <summary>
    /// Data Transfer Object (DTO) used to receive animal creation requests from the client.
    /// Maps directly to the <see cref="Animal"/> domain entity through AutoMapper.
    /// </summary>
    public class ReqCreateAnimalDto
    {
        /// <summary>
        /// The name of the animal.
        /// </summary>
        public string Name { get; set; } = null!; // This property is set during model binding.

        /// <summary>
        /// The biological species of the animal (e.g., Dog, Cat).
        /// </summary>
        public Species Species { get; set; }

        /// <summary>
        /// The unique identifier of the animal's breed.
        /// </summary>
        public string BreedId { get; set; } = null!;

        /// <summary>
        /// The size classification of the animal (e.g., Small, Medium, Large).
        /// </summary>
        public SizeType Size { get; set; }

        /// <summary>
        /// The biological sex of the animal (e.g., Male, Female).
        /// </summary>
        public SexType Sex { get; set; }

        /// <summary>
        /// The predominant color of the animal's fur or skin.
        /// </summary>
        public string Colour { get; set; } = null!;

        /// <summary>
        /// The date of birth of the animal.
        /// </summary>
        public DateOnly BirthDate { get; set; }

        /// <summary>
        /// Indicates whether the animal has been sterilized.
        /// </summary>
        public bool Sterilized { get; set; }

        /// <summary>
        /// The adoption or maintenance cost associated with the animal.
        /// </summary>
        public decimal Cost { get; set; }

        /// <summary>
        /// Additional physical or behavioral features of the animal.
        /// </summary>
        public string? Features { get; set; }

        /// <summary>
        /// A textual description or summary about the animal.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// A list of images associated with the animal at creation.
        /// </summary>
        public List<ReqImageDto> Images { get; set; } = new();
    }
}
