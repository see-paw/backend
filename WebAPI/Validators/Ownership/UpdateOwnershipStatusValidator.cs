using FluentValidation;

using WebAPI.DTOs;
using WebAPI.DTOs.Ownership;

namespace WebAPI.Validators.Ownership;

/// <summary>
/// Validator for updating ownership request status.
/// </summary>
public class UpdateOwnershipStatusValidator : AbstractValidator<ReqUpdateOwnershipStatusDto>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateOwnershipStatusValidator"/> class  
    /// and defines validation rules for updating ownership request status.
    /// </summary>
    public UpdateOwnershipStatusValidator()
    {
        RuleFor(x => x.RequestInfo)
            .MaximumLength(500).WithMessage("Request info cannot exceed 500 characters");
    }
}
