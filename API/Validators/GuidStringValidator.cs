using System.Diagnostics.CodeAnalysis;

namespace API.Validators;

using FluentValidation;

public class GuidStringValidator : AbstractValidator<string>
{
    public GuidStringValidator(string? fieldName = "ID")
    {
        RuleFor(id => id)
            .NotEmpty().WithMessage($"{fieldName} is required")
            .Must(id => Guid.TryParse(id, out _)).WithMessage($"Invalid {fieldName}");

    }
}
