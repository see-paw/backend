using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class AnimalDTO
    {
        [Required, StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = "";

        [Required, StringLength(1000)]
        public string Description { get; set; } = "";

        [Required]
        public required AnimalState AnimalState { get; set; }

        [Required]
        public Species Species { get; set; }

        [Required]
        public Breed Breed { get; set; }

        [Required]
        public SizeType Size { get; set; }

        [Required]
        public SexType Sex { get; set; }

        [Required, StringLength(50)]
        public string Colour { get; set; } = "";

        [Required]
        public DateOnly BirthDate { get; set; }

        [Required]
        public bool Sterilized { get; set; }

        [Required, Range(0, 1000)]
        public decimal Cost { get; set; }

        [StringLength(300)]
        public string? Features { get; set; }

        [Required, Url, StringLength(500)]
        public string MainImageUrl { get; set; } = "";
    }
}
