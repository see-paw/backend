using Application.Fosterings;
using Application.Fosterings.Commands;

using FluentValidation;

using Microsoft.Extensions.Options;

namespace WebAPI.Validators.Fosterings;

/// <summary>
/// Provides validation rules for the <see cref="AddFostering.Command"/> request.
/// Ensures that the animal identifier is valid and that the monthly contribution
/// meets the configured minimum requirements.
/// </summary>
public class AddFosteringValidator : AbstractValidator<AddFostering.Command>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddFosteringValidator"/> class.
    /// </summary>
    /// <param name="options">The application configuration options containing fostering settings.</param>
    /// <remarks>
    /// Validation rules:
    /// <list type="bullet">
    /// <item><description><c>AnimalId</c> must not be null or empty and must be a valid GUID string.</description></item>
    /// <item><description><c>MonthValue</c> must be greater than or equal to the configured minimum monthly value.</description></item>
    /// </list>
    /// </remarks>
    public AddFosteringValidator(IOptions<FosteringSettings> options)
    {
        var fosteringSettings = options.Value;

        RuleFor(x => x.AnimalId)
            .NotNull().WithMessage("AnimalId is required.")
            .NotEmpty().WithMessage("AnimalId is required.")
            .MustBeValidGuidString("Id with wrong format");

        RuleFor(x => x.MonthValue)
            .GreaterThanOrEqualTo(fosteringSettings.MinMonthlyValue)
            .WithMessage($"Month value must be greater than or equal than {fosteringSettings.MinMonthlyValue}");

    }
}
