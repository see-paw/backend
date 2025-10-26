using FluentValidation;
using WebAPI.DTOs;

namespace WebAPI.Validators;

/// <summary>
/// Validator for updating ownership request status.
/// </summary>
public class UpdateOwnershipStatusValidator : AbstractValidator<ReqUpdateOwnershipStatusDto>
{
    public UpdateOwnershipStatusValidator()
    {
        RuleFor(x => x.RequestInfo)
            .MaximumLength(500).WithMessage("Request info cannot exceed 500 characters");
    }
}