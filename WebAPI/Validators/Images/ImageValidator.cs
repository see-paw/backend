using FluentValidation;
using WebAPI.DTOs;
using WebAPI.DTOs.Images;

namespace WebAPI.Validators.Images
{
    /// <summary>
    /// Validator for the <see cref="ReqImageDto"/> class.
    /// Ensures that image data provided in an animal creation request
    /// meets formatting and content requirements.
    /// </summary>
    public class ImageValidator : AbstractValidator<ReqImageDto>
    {
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private const long MaxFileSizeBytes = 5 * 1024 * 1024;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageValidator"/> class.
        /// Defines validation rules for image URLs and descriptions.
        /// </summary>
        public ImageValidator()
        {
            RuleFor(x => x.File)
                .NotNull()
                .WithMessage("Image file is required.")
                .Must(file => file.Length > 0)
                .WithMessage("Image file cannot be empty.")
                .Must(file => file.Length <= MaxFileSizeBytes)
                .WithMessage($"Image file size cannot exceed {MaxFileSizeBytes / (1024 * 1024)}MB.")
                .Must(BeValidImageType)
                .WithMessage($"Invalid image type. Allowed types: {string.Join(", ", AllowedExtensions)}.");

            RuleFor(x => x.Description)
                .MaximumLength(255)
                .WithMessage("Description cannot exceed 255 characters.");
        }

        private static bool BeValidImageType(IFormFile file)
        {
            if (file == null) return false;

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            return AllowedExtensions.Contains(extension);
        }
    }
}