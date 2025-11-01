using FluentValidation;
using WebAPI.DTOs.Images;

namespace WebAPI.Validators.Images;

/// <summary>
/// Validator for the <see cref="ReqAddImagesDto"/> class.
/// Ensures that image data meets formatting and content requirements.
/// </summary>
public class AddImagesAnimalValidator : AbstractValidator<ReqAddImagesDto>
{
    private const int MaxImagesPerRequest = 10;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddImagesAnimalValidator"/> class.
    /// </summary>
    public AddImagesAnimalValidator()
    {
        RuleFor(x => x.Images)
            .NotNull().WithMessage("Images cannot be null")
            .NotEmpty().WithMessage("At least one image is required.")
            .Must(images => images.Count <= MaxImagesPerRequest)
            .WithMessage($"Cannot add more than {MaxImagesPerRequest} images at once.");

        RuleForEach(x => x.Images)
            .SetValidator(new ImageValidator());
    }
    
}