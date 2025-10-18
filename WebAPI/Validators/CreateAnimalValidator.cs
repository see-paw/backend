using Application.Animals.Commands;
using FluentValidation;
using WebAPI.DTOs;

namespace WebAPI.Validators
{
    public class CreateAnimalValidator : AbstractValidator<ReqCreateAnimalDto>
    {
        public CreateAnimalValidator()
        {
            RuleFor(x => x.Name)
                .NotNull().WithMessage("Name cannot be null")
                .NotEmpty().WithMessage("Name is required")
                .Length(2, 100).WithMessage("Name must be between 2 and 100 characters")
                .Matches(@"^[a-zA-ZÀ-ÿ\s'-]+$").WithMessage("Name can only contain letters, spaces, hyphens, and apostrophes.");

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
                .Matches(@"^[a-zA-ZÀ-ÿ\s-]+$").WithMessage("Colour can only contain letters, spaces, and hyphens.");

            RuleFor(x => x.BirthDate)
                .NotEmpty().WithMessage("Birth date is required")
                .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow)).WithMessage("Birth date cannot be in the future.")
                .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30))).WithMessage("Birth date seems unrealistic (more than 30 years ago).");

            RuleFor(x => x.Sterilized)
                .NotNull().WithMessage("Sterilization status is required.");

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


            // quando as imagens estiverem implementadas add:
            // RuleForEach(x => x.Images).SetValidator(new ImageValidator());
        }
    }
}
