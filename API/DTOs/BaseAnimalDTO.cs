using Domain.Enums;

namespace API.DTOs
{
    /// <summary>
    /// Represents the base data transfer object (DTO) containing common properties 
    /// for animal-related operations within the SeePaw API.
    /// </summary>
    public class BaseAnimalDTO
    {
        /// <summary>
        /// Gets or sets the name of the animal.
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// Gets or sets an optional description of the animal, such as personality traits or background information.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the species of the animal (e.g., Dog, Cat, etc.).
        /// </summary>
        public Species Species { get; set; }

        /// <summary>
        /// Gets or sets the size classification of the animal (e.g., Small, Medium, Large).
        /// </summary>
        public SizeType Size { get; set; }

        /// <summary>
        /// Gets or sets the biological sex of the animal (e.g., Male, Female).
        /// </summary>
        public SexType Sex { get; set; }

        /// <summary>
        /// Gets or sets the dominant colour of the animal’s fur or skin.
        /// </summary>
        public string Colour { get; set; } = "";

        /// <summary>
        /// Gets or sets the birth date of the animal.
        /// </summary>
        public DateOnly BirthDate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the animal has been sterilized.
        /// </summary>
        public bool Sterilized { get; set; }

        /// <summary>
        /// Gets or sets the breed of the animal (e.g., Labrador, Siamese).
        /// </summary>
        public Breed Breed { get; set; }

        /// <summary>
        /// Gets or sets the adoption cost or associated fee for the animal.
        /// </summary>
        public decimal Cost { get; set; }

        /// <summary>
        /// Gets or sets additional identifying features of the animal, 
        /// such as marks, scars, or behavioral notes.
        /// </summary>
        public string Features { get; set; } = "";

        /// <summary>
        /// Gets or sets the URL of the animal’s main image.
        /// </summary>
        public string MainImageUrl { get; set; } = "";
    }
}
