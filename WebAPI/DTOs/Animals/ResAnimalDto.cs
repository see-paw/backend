using Domain.Enums;

using WebAPI.DTOs.Breeds;
using WebAPI.DTOs.Images;

namespace WebAPI.DTOs.Animals;

/// <summary>
/// Data Transfer Object (DTO) used to return detailed information about an animal.
/// Includes biological data, shelter association, and related entities such as breed and images.
/// </summary>
public class ResAnimalDto
{
    /// <summary>
    /// The unique identifier of the animal.
    /// </summary>
    public string Id { get; set; } = null!;

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
    public ResBreedDto Breed { get; set; } = null!;

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

    /// <summary>
    /// The total active fostering support currently received by the animal.
    /// </summary>
    public decimal CurrentSupportValue { get; set; }

}

