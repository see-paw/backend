using API.DTOs;
using FluentValidation;

namespace API.Validators
{
    /// <summary>
    /// Generic base validator for animal-related data transfer objects.
    /// Defines common validation rules that apply to all <see cref="BaseAnimalDTO"/> types.
    /// </summary>
    /// <typeparam name="T">The type of the object containing the animal DTO.</typeparam>
    /// <typeparam name="TDto">The DTO type that inherits from <see cref="BaseAnimalDTO"/>.</typeparam>
    /// <remarks>
    /// This validator uses a selector function to extract the DTO from the generic object,
    /// allowing consistent validation rules to be reused across multiple structures.
    /// </remarks>
    public class BaseAnimalValidator<T, TDto> : AbstractValidator<T> where TDto : BaseAnimalDTO
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseAnimalValidator{T, TDto}"/> class
        /// and defines common validation rules for animal data.
        /// </summary>
        /// <param name="selector">Function used to extract the DTO instance from the parent object.</param>
        public BaseAnimalValidator(Func<T, TDto> selector)
        {
            RuleFor(x => selector(x).Name)
                .NotNull().WithMessage("Name cannot be null")
                .NotEmpty().WithMessage("Name is required")
                .Length(2, 40).WithMessage("Name must be between 2 and 40 characters")
                .Matches(@"^[a-zA-ZÀ-ÿ\s'-]+$").WithMessage("Name can only contain letters, spaces, hyphens, and apostrophes.");

            RuleFor(x => selector(x).Species)
                .IsInEnum().WithMessage("Invalid species.");

            RuleFor(x => selector(x).Size)
                .IsInEnum().WithMessage("Invalid size type.");

            RuleFor(x => selector(x).Sex)
                .IsInEnum().WithMessage("Invalid sex type.");

            RuleFor(x => selector(x).Breed)
                .IsInEnum().WithMessage("Invalid breed.");

            RuleFor(x => selector(x).Colour)
                .NotNull().WithMessage("Colour cannot be null")
                .NotEmpty().WithMessage("Colour is required")
                .Length(2, 40).WithMessage("Colour must be between 2 and 40 characters")
                .Matches(@"^[a-zA-ZÀ-ÿ\s-]+$").WithMessage("Colour can only contain letters, spaces, and hyphens.");

            RuleFor(x => selector(x).BirthDate)
                .NotEmpty().WithMessage("Birth date is required")
                .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow)).WithMessage("Birth date cannot be in the future.")
                .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30))).WithMessage("Birth date seems unrealistic (more than 30 years ago).");

            RuleFor(x => selector(x).Sterilized)
                .NotNull().WithMessage("Sterilization status is required.");

            RuleFor(x => selector(x).Cost)
                .NotNull().WithMessage("Cost is required")
                .GreaterThanOrEqualTo(0).WithMessage("Cost must be zero or positive.")
                .LessThanOrEqualTo(1000).WithMessage("Cost cannot exceed 1000.")
                .PrecisionScale(6, 2, true).WithMessage("Cost can have at most 2 decimal places.");

            RuleFor(x => selector(x).MainImageUrl)
                .NotNull().WithMessage("Main image URL cannot be null")
                .NotEmpty().WithMessage("Main image URL is required")
                .Must(BeAValidUrl).WithMessage("Main image URL must be a valid HTTP or HTTPS URL.");
        }

        /// <summary>
        /// Validates whether a given string is a well-formed HTTP or HTTPS URL.
        /// </summary>
        /// <param name="url">The URL string to validate.</param>
        /// <returns><c>true</c> if the string is a valid URL; otherwise, <c>false</c>.</returns>
        /// <remarks>
        /// This method only checks the syntax and scheme of the URL (HTTP/HTTPS).
        /// The actual string is stored as-is in the database after validation.
        /// </remarks>
        /// <example>
        /// Valid URLs:
        /// <code>
        /// https://example.com/image.jpg
        /// http://cdn.site.com/photo.png
        /// </code>
        /// Invalid URLs:
        /// <code>
        /// example.com/image.jpg  // missing protocol
        /// ftp://server.com/file.jpg  // unsupported protocol
        /// </code>
        /// </example>
        private bool BeAValidUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;

            return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }
    }
}
