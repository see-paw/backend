using FluentValidation;

namespace WebAPI.Validators;
/// <summary>
/// Provides custom extension methods for FluentValidation rules.
/// </summary>
/// <remarks>
/// This static class contains reusable validation logic that extends the FluentValidation
/// <see cref="IRuleBuilder{T,TProperty}"/> interface, allowing for cleaner and more expressive
/// validation rules across the application.
/// </remarks>
public static class CustomValidationExtensions
{
    /// <summary>
    /// Adds a validation rule to ensure that a string property represents a valid GUID value.
    /// </summary>
    /// <typeparam name="T">The type of the object being validated.</typeparam>
    /// <param name="ruleBuilder">The FluentValidation rule builder instance.</param>
    /// <param name="propertyName">
    /// The name of the property being validated.  
    /// Defaults to <c>"ID"</c> if not specified.
    /// </param>
    /// <returns>
    /// An IRuleBuilderOptions that can be further configured or finalized
    /// as part of the FluentValidation rule chain.
    /// </returns>
    /// <remarks>
    /// This rule checks whether the input string can be parsed as a valid <see cref="Guid"/>.
    /// If the validation fails, an error message is generated in the format:
    /// <c>"{propertyName} must be a valid GUID."</c>
    /// </remarks>
    public static IRuleBuilderOptions<T, string> MustBeValidGuidString<T>(
        this IRuleBuilder<T, string> ruleBuilder, string propertyName = "ID")
    {
        return ruleBuilder
            .Must(value => Guid.TryParse(value, out _))
            .WithMessage($"{propertyName} must be a valid GUID.");
    }
}

