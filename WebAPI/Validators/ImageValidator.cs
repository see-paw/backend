using Application.Animals.Commands;
using FluentValidation;
using WebAPI.DTOs;

namespace WebAPI.Validators
{
    public class ImageValidator : AbstractValidator<ReqImageDTO>
    {
        public ImageValidator()
        { 
            RuleFor (x => x.Url)
                .NotNull().WithMessage("Url cannot be null")
                .NotEmpty().WithMessage("Url is required")
                .MaximumLength(500).WithMessage("Url cannot exceed 500 characters.")
                .Matches(@"^(https?:\/\/)([a-zA-Z0-9\-\.]+\.[a-zA-Z]{2,})(\/\S*)\.(jpg|jpeg|png)$").WithMessage("Url must be a valid HTTP or HTTPS image ending in .jpg, .jpeg, or .png.");

            RuleFor (x => x.Description)
                .MaximumLength(255).WithMessage("Description cannot exceed 255 characters.");
        }
    }
}
