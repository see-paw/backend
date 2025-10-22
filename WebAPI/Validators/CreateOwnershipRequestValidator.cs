using FluentValidation;
using WebAPI.DTOs;

namespace WebAPI.Validators;

/// <summary>
/// Validator for creating ownership requests.
/// </summary>
public class CreateOwnershipRequestValidator : AbstractValidator<ReqCreateOwnershipRequestDto>
{
    public CreateOwnershipRequestValidator()
    {
        RuleFor(x => x.AnimalId)
            .NotNull().WithMessage("Animal ID cannot be null")
            .NotEmpty().WithMessage("Animal ID is required")
            .Length(36).WithMessage("Animal ID must be a valid GUID format");

        RuleFor(x => x.RequestInfo)
            .MaximumLength(500).WithMessage("Request info cannot exceed 500 characters");
    }
}