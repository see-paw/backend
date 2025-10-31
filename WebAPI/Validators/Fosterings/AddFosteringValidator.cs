using Application.Fosterings;
using Application.Fosterings.Commands;
using FluentValidation;
using Microsoft.Extensions.Options;

namespace WebAPI.Validators.Fosterings;

public class AddFosteringValidator : AbstractValidator<AddFostering.Command>
{
    
    public AddFosteringValidator(IOptions<FosteringSettings> options)
    {
        var fosteringSettings = options.Value;
        
        RuleFor(x => x.AnimalId)
            .NotNull().WithMessage("AnimalId is required.")
            .NotEmpty().WithMessage("AnimalId is required.")
            .MustBeValidGuidString("Id with wrong format");

        RuleFor(x => x.MonthValue)
            .GreaterThanOrEqualTo(fosteringSettings.MinMonthlyValue);
        
    }
}