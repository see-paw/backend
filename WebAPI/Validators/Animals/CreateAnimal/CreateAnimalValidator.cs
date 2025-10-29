using FluentValidation;
using WebAPI.DTOs.Animals;
using WebAPI.DTOs.Images;
using WebAPI.Validators.Images;

namespace WebAPI.Validators.Animals.CreateAnimal
{
    /// <summary>
    /// Validator for the <see cref="ReqCreateAnimalDto"/> class.
    /// Ensures that all required fields meet business and formatting rules
    /// before the animal creation request is processed.
    /// </summary>
    public class CreateAnimalValidator : AbstractValidator<ReqCreateAnimalDto>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CreateAnimalValidator"/> class.
        /// Defines validation rules for animal creation requests.
        /// </summary>
        public CreateAnimalValidator()
        {
            RuleFor(x => x.Name)
                .NotNull().WithMessage("Name cannot be null")
                .NotEmpty().WithMessage("Name is required")
                .Length(2, 100).WithMessage("Name must be between 2 and 100 characters")
                .Matches(@"^[a-zA-ZÀ-ÿ\s'-]+$")
                .WithMessage("Name can only contain letters, spaces, hyphens, and apostrophes.");

            RuleFor(x => x.Species)
                .IsInEnum().WithMessage("Invalid species.");

            RuleFor(x => x.Size)
                .IsInEnum().WithMessage("Invalid size type.");

            RuleFor(x => x.Sex)
                .IsInEnum().WithMessage("Invalid sex type.");

            RuleFor(x => x.Colour)
                .NotNull().WithMessage("Colour cannot be null")
                .NotEmpty().WithMessage("Colour is required")
                .Length(2, 50).WithMessage("Colour must be between 2 and 50 characters")
                .Matches(@"^[a-zA-ZÀ-ÿ\s-]+$")
                .WithMessage("Colour can only contain letters, spaces, and hyphens.");

            RuleFor(x => x.BirthDate)
                .NotEmpty().WithMessage("Birth date is required")
                .LessThanOrEqualTo(_ => DateOnly.FromDateTime(DateTime.UtcNow.Date))
                .WithMessage("Birth date cannot be in the future.")
                .GreaterThanOrEqualTo(_ => DateOnly.FromDateTime(DateTime.UtcNow.Date.AddYears(-30)))
                .WithMessage("Birth date seems unrealistic (more than 30 years ago).");

            RuleFor(x => x.Cost)
                .NotNull().WithMessage("Cost is required")
                .GreaterThanOrEqualTo(0).WithMessage("Cost must be zero or positive.")
                .LessThanOrEqualTo(1000).WithMessage("Cost cannot exceed 1000.")
                .PrecisionScale(6, 2, true).WithMessage("Cost can have at most 2 decimal places.");

            RuleFor(x => x.Features)
                .MaximumLength(300)
                .WithMessage("Features cannot exceed 300 characters.");

            RuleFor(x => x.Description)
                .MaximumLength(250)
                .WithMessage("Description cannot exceed 250 characters.");

            RuleFor(x => x.BreedId)
                .NotNull().WithMessage("BreedId cannot be null")
                .NotEmpty().WithMessage("BreedId is required");

            RuleFor(x => x.Images)
                .NotNull().WithMessage("Images cannot be null")
                .NotEmpty().WithMessage("At least one image is required.")
                .Must(images => images.Count > 0).WithMessage("At least one image is required.")
                .Must(HaveAtMostOnePrincipalImage)
                .WithMessage("Only one image can be marked as principal.");

            RuleForEach(x => x.Images)
                .SetValidator(new ImageValidator());
        }

        private static bool HaveAtMostOnePrincipalImage(List<ReqImageDto> images)
        {
            return images.Count(img => img.IsPrincipal) <= 1;
        }
    }
}