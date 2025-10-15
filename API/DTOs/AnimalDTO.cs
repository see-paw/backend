
using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace API.DTOs
{
    /// <summary>
    /// Data Transfer Object (DTO) representing an animal entity exposed through the API.
    /// </summary>
    /// <remarks>
    /// The <see cref="AnimalDto"/> class defines the structure of data returned by the API when
    /// retrieving animal information. It mirrors the <see cref="Domain.Animal"/> entity but omits
    /// internal metadata to ensure that only relevant, safe, and validated fields are exposed to clients.  
    /// 
    /// This DTO includes biological attributes (e.g., species, sex, size), descriptive fields (e.g., name,
    /// colour, features), and adoption-related details (e.g., <see cref="AnimalState"/>).  
    /// 
    /// Validation attributes enforce field constraints to ensure that incoming or outgoing data
    /// complies with the application’s integrity rules.
    /// </remarks>
    public class AnimalDto
    {
        [Required]
        public string AnimalId { get; init; } = "";

        [Required, StringLength(100, MinimumLength = 2)]
        public string Name { get; init; } = "";

        [Required, StringLength(1000)]
        public string Description { get; init; } = "";

        [Required]
        public required AnimalState AnimalState { get; init; }

        [Required]
        public Species Species { get; init; }

        [Required]
        public Breed Breed { get; init; }

        [Required]
        public SizeType Size { get; init; }

        [Required]
        public SexType Sex { get; init; }

        [Required, StringLength(50)]
        public string Colour { get; init; } = "";

        [Required]
        public DateOnly BirthDate { get; init; }

        [Required]
        public bool Sterilized { get; init; }

        [Required, Range(0, 1000)]
        public decimal Cost { get; init; }

        [StringLength(300)]
        public string? Features { get; init; }

        [Required, Url, StringLength(500)]
        public string MainImageUrl { get; init; } = "";
    }

}
