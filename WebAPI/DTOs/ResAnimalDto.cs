using Domain;
using Domain.Enums;

namespace WebAPI.DTOs
{
<<<<<<< HEAD
    public required string Id { get; set; }        
    public required string Name { get; set; }
    public required Species Species { get; set; }
    public required SizeType Size { get; set; }
    public required SexType Sex { get; set; }
    public required AnimalState AnimalState { get; set; }
    public required string Colour { get; set; }
    public required DateOnly BirthDate { get; set; }
    public int Age { get; set; }                         
    public string Description { get; set; } = string.Empty;
    public required bool Sterilized { get; set; }
    public string Features { get; set; } = string.Empty;
    public decimal Cost { get; set; }
    public string BreedName { get; set; } = string.Empty;

    public ICollection<ResImageDto> Images { get; set; } = [];
}
=======
    /// <summary>
    /// Data Transfer Object (DTO) used to return detailed information about an animal.
    /// Includes biological data, shelter association, and related entities such as breed and images.
    /// </summary>
    public class ResAnimalDto
    {
        /// <summary>
        /// The unique identifier of the animal.
        /// </summary>
        public string AnimalId { get; set; } = null!;
>>>>>>> feature/create-and-list-animals

        /// <summary>
        /// The name of the animal.
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// The biological species of the animal (e.g., Dog, Cat).
        /// </summary>
        public Species Species { get; set; }

        /// <summary>
        /// The size classification of the animal (e.g., Small, Medium, Large).
        /// </summary>
        public SizeType Size { get; set; }

        /// <summary>
        /// The biological sex of the animal (e.g., Male, Female).
        /// </summary>
        public SexType Sex { get; set; }

        /// <summary>
        /// The breed information associated with the animal.
        /// </summary>
        public ResBreedDto Breed { get; set; }

        /// <summary>
        /// The current state of the animal (e.g., Available, Fostered, Adopted).
        /// </summary>
        public AnimalState AnimalState { get; set; }

        /// <summary>
        /// The predominant color of the animal's fur or skin.
        /// </summary>
        public string Colour { get; set; } = null!;

        /// <summary>
        /// The date of birth of the animal.
        /// </summary>
        public DateOnly BirthDate { get; set; }

        /// <summary>
        /// The calculated age of the animal in years.
        /// </summary>
        public int Age { get; set; }

        /// <summary>
        /// A textual description or summary providing details about the animal.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Indicates whether the animal has been sterilized.
        /// </summary>
        public bool Sterilized { get; set; }

        /// <summary>
        /// Additional physical or behavioral features of the animal.
        /// </summary>
        public string? Features { get; set; }

        /// <summary>
        /// The adoption or maintenance cost associated with the animal.
        /// </summary>
        public decimal Cost { get; set; }

        /// <summary>
        /// The unique identifier of the shelter where the animal is hosted.
        /// </summary>
        public string shelterId { get; set; } = null!;

        /// <summary>
        /// A list of images associated with the animal.
        /// </summary>
        public List<ResImageDto>? Images { get; set; }
    }
}
