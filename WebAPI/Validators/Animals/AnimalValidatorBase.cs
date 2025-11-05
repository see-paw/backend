using Domain;
using FluentValidation;

namespace WebAPI.Validators.Animals;

/// <summary>
/// Defines the base validation rules that apply to any <see cref="Animal"/> entity.
/// This validator contains all common constraints shared between create and edit operations,
/// excluding image-related or state-specific logic.
/// </summary>
public class AnimalValidatorBase : AbstractValidator<Animal>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AnimalValidatorBase"/> class.
    /// </summary>
    public AnimalValidatorBase()
    {
        RuleFor(a => a.Name)
            .NotEmpty().WithMessage("Name is required.")
            .Length(2, 100).WithMessage("Name must be between 2 and 100 characters.");

        RuleFor(a => a.Colour)
            .NotEmpty().WithMessage("Colour is required.")
            .MaximumLength(50).WithMessage("Colour cannot exceed 50 characters.");

        RuleFor(a => a.Description)
            .MaximumLength(250).WithMessage("Description cannot exceed 250 characters.");

        RuleFor(a => a.Features)
            .MaximumLength(300).WithMessage("Features cannot exceed 300 characters.");

        RuleFor(a => a.Cost)
            .GreaterThanOrEqualTo(0).WithMessage("Cost must be zero or positive.");

        RuleFor(a => a.BreedId)
            .NotEmpty().WithMessage("BreedId is required.")
            .Must(id => Guid.TryParse(id, out _)).WithMessage("BreedId must be a valid GUID.");

        RuleFor(a => a.ShelterId)
            .NotEmpty().WithMessage("ShelterId is required.")
            .Must(id => Guid.TryParse(id, out _)).WithMessage("ShelterId must be a valid GUID.");

        RuleFor(a => a.BirthDate)
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("BirthDate cannot be in the future.");

        RuleFor(a => a.Species)
            .IsInEnum().WithMessage("Invalid species type.");

        RuleFor(a => a.Size)
            .IsInEnum().WithMessage("Invalid size type.");

        RuleFor(a => a.Sex)
            .IsInEnum().WithMessage("Invalid sex type.");
    }
}
