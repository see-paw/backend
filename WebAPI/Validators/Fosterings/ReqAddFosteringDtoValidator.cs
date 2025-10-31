using FluentValidation;
using WebAPI.DTOs.Fostering;

namespace WebAPI.Validators.Fosterings;

public class ReqAddFosteringDtoValidator : AbstractValidator<ReqAddFosteringDto>
{
    public ReqAddFosteringDtoValidator()
    {
        RuleFor(x => x.MonthValue)
            .NotNull().WithMessage("Month value is required")
            .NotEmpty().WithMessage("Month value is required")
            .GreaterThan(0).WithMessage("Month value must be greater than 0")
            .LessThan(Decimal.MaxValue).WithMessage("Month value must be less than Double.MaxValue");
    }
}