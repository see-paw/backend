using FluentValidation;

namespace WebAPI.Validators;
public static class CustomValidationExtensions
{
    public static IRuleBuilderOptions<T, string> MustBeValidGuidString<T>(
        this IRuleBuilder<T, string> ruleBuilder, string propertyName = "ID")
    {
        return ruleBuilder
            .Must(value => Guid.TryParse(value, out _))
            .WithMessage($"{propertyName} must be a valid GUID.");
    }
}

