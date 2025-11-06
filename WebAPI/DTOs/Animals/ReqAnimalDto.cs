using Domain.Enums;

namespace WebAPI.DTOs.Animals;

/// <summary>
/// Data transfer object used for creating or updating animal records.
/// </summary>
public class ReqAnimalDto
{
    /// <summary>
    /// Name of the animal.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Current state of the animal (e.g., Available, Adopted, Fostered).
    /// </summary>
    public AnimalState? AnimalState { get; set; }

    /// <summary>
    /// Description or summary about the animal.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Biological species of the animal (e.g., Dog, Cat).
    /// </summary>
    public required Species Species { get; set; }

    /// <summary>
    /// Size classification of the animal (e.g., Small, Medium, Large).
    /// </summary>
    public required SizeType Size { get; set; }

    /// <summary>
    /// Biological sex of the animal (e.g., Male, Female).
    /// </summary>
    public required SexType Sex { get; set; }

    /// <summary>
    /// Dominant colour of the animal’s fur or skin.
    /// </summary>
    public required string Colour { get; set; }

    /// <summary>
    /// Date of birth of the animal.
    /// </summary>
    public required DateOnly BirthDate { get; set; }

    /// <summary>
    /// Indicates whether the animal has been sterilized.
    /// </summary>
    public required bool Sterilized { get; set; }

    /// <summary>
    /// Maintenance or adoption cost of the animal.
    /// </summary>
    public required decimal Cost { get; set; }

    /// <summary>
    /// Additional notable features of the animal.
    /// </summary>
    public string Features { get; set; } = string.Empty;

    /// <summary>
    /// Main image URL associated with the animal.
    /// </summary>
    public required string MainImageUrl { get; set; }

    /// <summary>
    /// Description or caption for the main image.
    /// </summary>
    public string MainImageDesc { get; set; } = string.Empty;

    /// <summary>
    /// Identifier of the shelter where the animal is registered.
    /// </summary>
    public required string ShelterId { get; set; }

    /// <summary>
    /// Identifier of the breed associated with the animal.
    /// </summary>
    public required string BreedId { get; set; }
}
