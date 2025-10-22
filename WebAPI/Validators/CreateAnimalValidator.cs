using Application.Animals.Commands;
using FluentValidation;
using WebAPI.DTOs;

namespace WebAPI.Validators
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
            // Validate Name
            RuleFor(x => x.Name)
                .NotNull().WithMessage("Name cannot be null")
                .NotEmpty().WithMessage("Name is required")
                .Length(2, 100).WithMessage("Name must be between 2 and 100 characters")
                .Matches(@"^[a-zA-ZÀ-ÿ\s'-]+$")
                .WithMessage("Name can only contain letters, spaces, hyphens, and apostrophes.");

            // Validate Species (enum)
            RuleFor(x => x.Species)
                .IsInEnum().WithMessage("Invalid species.");

            // Validate Size (enum)
            RuleFor(x => x.Size)
                .IsInEnum().WithMessage("Invalid size type.");

            // Validate Sex (enum)
            RuleFor(x => x.Sex)
                .IsInEnum().WithMessage("Invalid sex type.");

            // Validate Colour
            RuleFor(x => x.Colour)
                .NotNull().WithMessage("Colour cannot be null")
                .NotEmpty().WithMessage("Colour is required")
                .Length(2, 50).WithMessage("Colour must be between 2 and 50 characters")
                .Matches(@"^[a-zA-ZÀ-ÿ\s-]+$")
                .WithMessage("Colour can only contain letters, spaces, and hyphens.");

            // Validate BirthDate
            RuleFor(x => x.BirthDate)
                .NotEmpty().WithMessage("Birth date is required")
                .LessThanOrEqualTo(_ => DateOnly.FromDateTime(DateTime.UtcNow.Date))
                .WithMessage("Birth date cannot be in the future.")
                .GreaterThanOrEqualTo(_ => DateOnly.FromDateTime(DateTime.UtcNow.Date.AddYears(-30)))
                .WithMessage("Birth date seems unrealistic (more than 30 years ago).");

            // Validate Cost
            RuleFor(x => x.Cost)
                .NotNull().WithMessage("Cost is required")
                .GreaterThanOrEqualTo(0).WithMessage("Cost must be zero or positive.")
                .LessThanOrEqualTo(1000).WithMessage("Cost cannot exceed 1000.")
                .PrecisionScale(6, 2, true).WithMessage("Cost can have at most 2 decimal places.");

            // Validate Features
            RuleFor(x => x.Features)
                .MaximumLength(300)
                .WithMessage("Features cannot exceed 300 characters.");

            // Validate Description
            RuleFor(x => x.Description)
                .MaximumLength(250)
                .WithMessage("Description cannot exceed 250 characters.");

            // Validate BreedId
            RuleFor(x => x.BreedId)
                .NotNull().WithMessage("BreedId cannot be null")
                .NotEmpty().WithMessage("BreedId is required");

            //Must have one image at least
            RuleFor(x => x.Images)
                .NotNull().WithMessage("Images cannot be null.")
                .Must(images => images != null && images.Count > 0)
                .WithMessage("At least one image is required.");

            // Apply the image validator to each image in the collection
            RuleForEach(x => x.Images)
                .SetValidator(new ImageValidator())
                .When(x => x.Images != null && x.Images.Any());

            // Ensure exactly one image is marked as principal
            RuleFor(x => x.Images)
                .Must(images => images != null && images.Count(img => img.isPrincipal) == 1)
                .WithMessage("Exactly one image must be marked as principal.")
                .When(x => x.Images != null && x.Images.Any());
        }
    }
}
