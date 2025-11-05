using Application.Activities.Commands;
using FluentValidation;

namespace WebAPI.Validators.Activities.Ownership;

/// <summary>
/// Validator for cancelling ownership activities.
/// </summary>
/// <remarks>
/// Validates that the activity ID provided in the cancellation request is a valid GUID.
/// </remarks>
public class CancelOwnershipActivityValidator : AbstractValidator<CancelOwnershipActivity.Command>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CancelOwnershipActivityValidator"/> class.
    /// </summary>
    public CancelOwnershipActivityValidator()
    {
        RuleFor(x => x.ActivityId)
            .NotEmpty()
            .WithMessage("Activity ID is required")
            .MustBeValidGuidString("Activity ID");
    }
}