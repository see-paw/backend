using FluentValidation;

using WebAPI.DTOs;
using WebAPI.DTOs.Ownership;

namespace WebAPI.Validators.Ownership;

/// <summary>
/// Validator for rejecting ownership requests.
/// </summary>
public class RejectOwnershipRequestValidator : AbstractValidator<ReqRejectOwnershipRequestDto>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RejectOwnershipRequestValidator"/> class  
    /// and defines validation rules for rejecting ownership requests.
    /// </summary>
    public RejectOwnershipRequestValidator()
    {
        RuleFor(x => x.RejectionReason)
            .MaximumLength(500).WithMessage("Rejection reason cannot exceed 500 characters");
    }
}
