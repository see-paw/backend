using FluentValidation;
using WebAPI.DTOs.Fostering;

namespace WebAPI.Validators.Fosterings;


/// <summary>
/// Provides validation rules for the <see cref="ReqAddFosteringDto"/> request model.
/// Ensures that the monthly fostering value is properly defined and within valid limits.
/// </summary>
public class ReqAddFosteringDtoValidator : AbstractValidator<ReqAddFosteringDto>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ReqAddFosteringDtoValidator"/> class.
    /// </summary>
    /// <remarks>
    /// Validation rules:
    /// <list type="bullet">
    /// <item><description><c>MonthValue</c> must not be null or empty.</description></item>
    /// <item><description><c>MonthValue</c> must be greater than 0.</description></item>
    /// <item><description><c>MonthValue</c> must be less than <see cref="Decimal.MaxValue"/>.</description></item>
    /// </list>
    /// </remarks>
    public ReqAddFosteringDtoValidator()
    {
        RuleFor(x => x.MonthValue)
            .NotNull().WithMessage("Month value is required")
            .NotEmpty().WithMessage("Month value is required")
            .GreaterThan(0).WithMessage("Month value must be greater than 0")
            .LessThan(Decimal.MaxValue).WithMessage("Month value must be less than Double.MaxValue");
    }
}