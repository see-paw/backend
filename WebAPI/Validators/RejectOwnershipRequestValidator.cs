using FluentValidation;
using WebAPI.DTOs;

namespace WebAPI.Validators;

/// <summary>
/// Validator for rejecting ownership requests.
/// </summary>
public class RejectOwnershipRequestValidator : AbstractValidator<ReqRejectOwnershipRequestDto>
{
    public RejectOwnershipRequestValidator()
    {
        RuleFor(x => x.RejectionReason)
            .MaximumLength(500).WithMessage("Rejection reason cannot exceed 500 characters");
    }
}