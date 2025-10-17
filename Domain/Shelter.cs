using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Domain
{
    /// <summary>
    /// Represents an animal shelter within the SeePaw system,
    /// including its identification, contact information, and operating details.
    /// </summary>
    public class Shelter
    {
        [Key]
        public string ShelterId { get; set; } = Guid.NewGuid().ToString();

        [Required, StringLength(150, MinimumLength = 2)]
        public required string Name { get; set; }

        [Required, StringLength(200)]
        public required string Street { get; set; }

        [Required, StringLength(100)]
        public required string City { get; set; }

        [Required, RegularExpression(@"^\d{4}-\d{3}$", ErrorMessage = "Postal Code must be in the format 0000-000.")]
        public required string PostalCode { get; set; }

        [Required, RegularExpression(@"^[29]\d{8}$", ErrorMessage = "Phone number must have 9 digits and start with 2 or 9.")]
        public required string Phone { get; set; }

        [Required, RegularExpression(@"^\d{9}$", ErrorMessage = "NIF must contain exactly 9 digits.")]
        public required string NIF { get; set; }

        [Url, StringLength(500)]
        public string? MainImageUrl { get; set; }

        [Required]
        public required TimeSpan OpeningTime { get; set; }

        [Required]
        public required TimeSpan ClosingTime { get; set; }

        [Required]
        public required DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public ICollection<Animal> Animals { get; set; } = new List<Animal>();
    }
}
