namespace API.Validators;

using FluentValidation;

/// <summary>
/// Provides validation rules for string inputs that are expected to represent valid GUID values.
/// </summary>
/// <remarks>
/// This validator ensures that the input string is not empty and can be parsed into a valid <see cref="Guid"/>.  
/// It is primarily used to validate entity identifiers (e.g., <c>ID</c> fields) received in API requests.
/// </remarks>
public class GuidStringValidator : AbstractValidator<string>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GuidStringValidator"/> class
    /// and defines validation rules for a string-based GUID field.
    /// </summary>
    /// <param name="fieldName">
    /// The display name of the field being validated, used in error messages.  
    /// Defaults to <c>"ID"</c> if not specified.
    /// </param>
    /// <remarks>
    /// The validation enforces two rules:
    /// <list type="bullet">
    /// <item><description>The field must not be empty.</description></item>
    /// <item><description>The field must contain a valid GUID format.</description></item>
    /// </list>
    /// Example error messages:
    /// <c>"ID is required"</c> or <c>"Invalid ID"</c>.
    /// </remarks>
    public GuidStringValidator(string? fieldName = "ID")
    {
        RuleFor(id => id)
            .NotEmpty().WithMessage($"{fieldName} is required")
            .Must(id => Guid.TryParse(id, out _)).WithMessage($"Invalid {fieldName}");
    }
}
