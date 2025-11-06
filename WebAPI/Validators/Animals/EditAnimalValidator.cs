using Application.Animals.Commands;
using FluentValidation;

namespace WebAPI.Validators.Animals;

/// <summary>
/// Defines validation rules for the <see cref="EditAnimal.Command"/>.
/// Extends <see cref="AnimalValidatorBase"/> and adds edit-specific constraints,
/// such as validating the <see cref="Domain.Enums.AnimalState"/> field.
/// </summary>
public class EditAnimalValidator : AbstractValidator<EditAnimal.Command>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EditAnimalValidator"/> class.
    /// </summary>
    public EditAnimalValidator()
    {
        RuleFor(x => x.Animal)
            .SetValidator(new AnimalValidatorBase());

        RuleFor(x => x.Animal.AnimalState)
            .IsInEnum()
            .WithMessage("Invalid animal state value.");
    }
}