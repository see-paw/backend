using Application.Animals.Commands;
using FluentValidation;
using WebAPI.DTOs;

namespace WebAPI.Validators
{
    /// <summary>
    /// Validator for the <see cref="ReqImageDTO"/> class.
    /// Ensures that image data provided in an animal creation request
    /// meets formatting and content requirements.
    /// </summary>
    public class ImageValidator : AbstractValidator<ReqImageDTO>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ImageValidator"/> class.
        /// Defines validation rules for image URLs and descriptions.
        /// </summary>
        public ImageValidator()
        {
            // Validate the image URL
            RuleFor(x => x.Url)
                .NotNull().WithMessage("Url cannot be null")
                .NotEmpty().WithMessage("Url is required")
                .MaximumLength(500).WithMessage("Url cannot exceed 500 characters.")
                .Matches(@"^(https?:\/\/)([a-zA-Z0-9\-\.]+\.[a-zA-Z]{2,})(\/\S*)\.(jpg|jpeg|png)$")
                .WithMessage("Url must be a valid HTTP or HTTPS image ending in .jpg, .jpeg, or .png.");

            // Validate the optional image description
            RuleFor(x => x.Description)
                .MaximumLength(255)
                .WithMessage("Description cannot exceed 255 characters.");
        }
    }
}
