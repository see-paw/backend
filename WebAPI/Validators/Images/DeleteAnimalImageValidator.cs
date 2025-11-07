using Application.Images.Commands;

using FluentValidation;

namespace WebAPI.Validators.Images;

/// <summary>
/// Validates the <see cref="DeleteAnimalImage.Command"/> request.
/// Ensures that both the animal and image identifiers are provided and valid before deletion.
/// </summary>
public class DeleteAnimalImageValidator : AbstractValidator<DeleteAnimalImage.Command>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteAnimalImageValidator"/> class.
    /// Defines validation rules for <c>AnimalId</c> and <c>ImageId</c>,
    /// ensuring that both fields are required and contain valid GUID strings.
    /// </summary>
    public DeleteAnimalImageValidator()
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
