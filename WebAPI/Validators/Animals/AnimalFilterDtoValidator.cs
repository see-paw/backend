using Domain.Enums;

using FluentValidation;

using WebAPI.DTOs.Animals;

namespace WebAPI.Validators.Animals;

/// <summary>
/// Validator for AnimalFilterDto that ensures filter criteria are valid.
/// Validates enum values, string lengths, and age constraints.
/// </summary>
public class AnimalFilterDtoValidator : AbstractValidator<AnimalFilterDto>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AnimalFilterDtoValidator"/> class.
    /// </summary>
    public AnimalFilterDtoValidator()
    {
        RuleFor(x => x.Species)
            .Must(BeValidSpecies)
            .When(x => !string.IsNullOrWhiteSpace(x.Species))
            .WithMessage("Species must be one of: Dog, Cat, Bird, Rabbit, Other");

        RuleFor(x => x.Age)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Age.HasValue)
            .WithMessage("Age must be a non-negative number");

        RuleFor(x => x.Age)
            .LessThanOrEqualTo(50)
            .When(x => x.Age.HasValue)
            .WithMessage("Age must be less than or equal to 50 years");

        RuleFor(x => x.Size)
            .Must(BeValidSize)
            .When(x => !string.IsNullOrWhiteSpace(x.Size))
            .WithMessage("Size must be one of: Small, Medium, Large");

        RuleFor(x => x.Sex)
            .Must(BeValidSex)
            .When(x => !string.IsNullOrWhiteSpace(x.Sex))
            .WithMessage("Sex must be one of: Male, Female");

        RuleFor(x => x.Name)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.Name))
            .WithMessage("Name filter must not exceed 100 characters");

        RuleFor(x => x.ShelterName)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.ShelterName))
            .WithMessage("Shelter name filter must not exceed 200 characters");

        RuleFor(x => x.Breed)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.Breed))
            .WithMessage("Breed filter must not exceed 100 characters");
    }

    private bool BeValidSpecies(string? species)
    {
        if (string.IsNullOrWhiteSpace(species))
            return true;

        return Enum.TryParse<Species>(species, true, out _);
    }

    private bool BeValidSize(string? size)
    {
        if (string.IsNullOrWhiteSpace(size))
            return true;

        return Enum.TryParse<SizeType>(size, true, out _);
    }

    private bool BeValidSex(string? sex)
    {
        if (string.IsNullOrWhiteSpace(sex))
            return true;

        return Enum.TryParse<SexType>(sex, true, out _);
    }
}
