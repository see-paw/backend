using Application.Images.Commands;
using FluentValidation;
using WebAPI.Validators.Activities;

namespace WebAPI.Validators.Images;

/// <summary>
/// Validates the <see cref="SetAnimalPrincipalImage.Command"/> request.
/// Ensures that both the animal and image identifiers are valid before setting a principal image.
/// </summary>
public class SetAnimalPrincipalImageValidator : AbstractValidator<SetAnimalPrincipalImage.Command>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SetAnimalPrincipalImageValidator"/> class.
    /// Defines validation rules for <c>AnimalId</c> and <c>ImageId</c>,
    /// ensuring they are not empty and represent valid GUID strings.
    /// </summary>
    public SetAnimalPrincipalImageValidator()
    {
        RuleFor(x => x.AnimalId)
            .NotEmpty()
            .WithMessage("AnimalId is required.")
            .MustBeValidGuidString("AnimalId");

        RuleFor(x => x.ImageId)
            .NotEmpty()
            .WithMessage("ImageId is required.")
            .MustBeValidGuidString("ImageId");
    }
}