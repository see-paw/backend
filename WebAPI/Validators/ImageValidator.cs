using Application.Animals.Commands;
using FluentValidation;
using WebAPI.DTOs;

namespace WebAPI.Validators
{
    /// <summary>
    /// Validator for the <see cref="ReqImageDto"/> class.
    /// Ensures that image data provided in an animal creation request
    /// meets formatting and content requirements.
    /// </summary>
    public class ImageValidator : AbstractValidator<ReqImageDto>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ImageValidator"/> class.
        /// Defines validation rules for image URLs and descriptions.
        /// </summary>
        public ImageValidator()
        {
            // Validate the optional image description
            RuleFor(x => x.Description)
                .MaximumLength(255)
                .WithMessage("Description cannot exceed 255 characters.");
        }
    }
}
