using Application.Activities.Commands;
using FluentValidation;

namespace WebAPI.Validators;

/// <summary>
/// Validator for cancelling ownership activities.
/// </summary>
/// <remarks>
/// Validates that the activity ID provided in the cancellation request is a valid GUID.
/// </remarks>
public class CancelOwnershipActivityValidator : AbstractValidator<CancelOwnershipActivity.Command>
{
    public CancelOwnershipActivityValidator()
    {
        RuleFor(x => x.ActivityId)
            .NotEmpty()
            .WithMessage("Activity ID is required")
            .MustBeValidGuidString("Activity ID");
    }
}