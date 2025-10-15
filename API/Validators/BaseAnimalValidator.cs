using API.DTOs;
using FluentValidation;

namespace API.Validators;

/// <summary>
/// Base generic validator for animal-related operations.
/// Defines common validation rules that apply to all animal commands/queries.
/// Other validators can inherit from this class and add specific rules.
/// </summary>
/// <typeparam name="T">The type of command/query (e.g., CreateAnimal.Command)</typeparam>
/// <typeparam name="TDto">The type of DTO containing the animal data</typeparam>
/// <remarks>
/// This validator uses a selector function to extract the DTO from the command/query,allowing it to work with different command/query structures while maintaining consistent validation rules across all animal operations.
/// </remarks>
public class BaseAnimalValidator<T, TDto> : AbstractValidator<T> where TDto : BaseAnimalDTO
{
    // <summary>
    /// Initializes a new instance of the BaseAnimalValidator with common validation rules.
    /// </summary>
    /// <param name="selector">Function that extracts the DTO from the command/query object</param>
    public BaseAnimalValidator(Func<T, TDto> selector)
    {
        RuleFor(x => selector(x).Name)
            .NotNull().WithMessage("Name cannot be null")
            .NotEmpty().WithMessage("Name is required")
            .Length(2, 40).WithMessage("Name must be between 2 and 40 characters")
            .Matches(@"^[a-zA-ZÀ-ÿ\s'-]+$").WithMessage("Name can only contain letters, spaces, hyphens and apostrophes");

        RuleFor(x => selector(x).Species)
            .NotNull().WithMessage("Species is required")
            .IsInEnum().WithMessage("Invalid species");

        RuleFor(x => selector(x).Size)
           .NotNull().WithMessage("Size is required")
           .IsInEnum().WithMessage("Invalid size type");

        RuleFor(x => selector(x).Sex)
            .NotNull().WithMessage("Sex is required")
            .IsInEnum().WithMessage("Invalid sex type");

        RuleFor(x => selector(x).Breed)
            .NotNull().WithMessage("Breed is required")
            .IsInEnum().WithMessage("Invalid breed");

        RuleFor(x => selector(x).Colour)
            .NotNull().WithMessage("Colour cannot be null")
            .NotEmpty().WithMessage("Colour is required")
            .Length(2, 40).WithMessage("Colour must be between 2 and 40 characters")
            .Matches(@"^[a-zA-ZÀ-ÿ\s-]+$").WithMessage("Colour can only contain letters, spaces and hyphens");

        RuleFor(x => selector(x).BirthDate)
            .NotEmpty().WithMessage("Birth date is required")
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow)).WithMessage("Birth date cannot be in the future")
            .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30))).WithMessage("Birth date seems unrealistic (more than 30 years ago)");

        RuleFor(x => selector(x).Sterilized)
           .NotNull().WithMessage("Sterilization status is required");


        RuleFor(x => selector(x).Cost)
            .NotNull().WithMessage("Cost is required")
            .GreaterThanOrEqualTo(0).WithMessage("Cost must be zero or positive")
            .LessThanOrEqualTo(1000).WithMessage("Cost cannot exceed 1000")
            .PrecisionScale(6, 2, true).WithMessage("Cost can have at most 2 decimal places");

        RuleFor(x => selector(x).MainImageUrl)
           .NotNull().WithMessage("Main image URL cannot be null")
           .NotEmpty().WithMessage("Main image URL is required")
           .Must(BeAValidUrl).WithMessage("Main image URL must be a valid URL");
    }

    /// <summary>
    /// Validates whether a string represents a valid URL.
    /// Checks if the URL has a correct format and uses HTTP or HTTPS protocol.
    /// </summary>
    /// <param name="url">The string to validate as a URL</param>
    /// <returns>true if it's a valid HTTP/HTTPS URL, false otherwise</returns>
    /// <remarks>
    /// This method only validates the URL format. The actual string is stored as-is in the database.
    /// The Uri object created during validation is temporary and discarded after validation.
    /// </remarks>
    /// <example>
    /// Valid URLs:
    /// - "https://example.com/image.jpg" → returns true
    /// - "http://cdn.site.com/photo.png" → returns true
    /// 
    /// Invalid URLs:
    /// - "example.com/image.jpg" → returns false (missing protocol)
    /// - "ftp://server.com/file.jpg" → returns false (FTP protocol not allowed)
    /// - "" → returns false (empty string)
    /// </example>
    private bool BeAValidUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;

        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
            && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}

