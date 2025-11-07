using Application.Animals.Commands;

using FluentValidation;

namespace WebAPI.Validators.Animals;

/// <summary>
/// Defines validation rules for the <see cref="CreateAnimal.Command"/>.
/// Inherits common rules from <see cref="AnimalValidatorBase"/> and adds
/// creation-specific validation, such as enforcing image uploads.
/// </summary>
public class CreateAnimalValidator : AbstractValidator<CreateAnimal.Command>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateAnimalValidator"/> class.
    /// </summary>
    public CreateAnimalValidator()
    {
        RuleFor(x => x.Animal)
            .SetValidator(new AnimalValidatorBase());

        RuleFor(x => x.Files)
            .NotEmpty().WithMessage("At least one image file is required.")
            .Must(files => files.All(f => f.Length > 0))
            .WithMessage("Each image file must contain data.");
    }
}
