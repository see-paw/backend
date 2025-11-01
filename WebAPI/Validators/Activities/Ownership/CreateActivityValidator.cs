using FluentValidation;
using WebAPI.DTOs.Activities;

namespace WebAPI.Validators.Activities.Ownership;

/// <summary>
/// Validator for creating ownership activities.
/// Ensures that all required fields meet business rules before the activity creation request is processed.
/// </summary>
public class CreateActivityValidator : AbstractValidator<ReqCreateActivityDto>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateActivityValidator"/> class.
    /// Defines validation rules for activity creation requests.
    /// </summary>
    public CreateActivityValidator()
    {
        // Validate AnimalId
        RuleFor(x => x.AnimalId)
            .NotNull().WithMessage("Animal ID cannot be null")
            .NotEmpty().WithMessage("Animal ID is required")
            .MustBeValidGuidString("Animal ID must be a valid GUID");

        // Validate StartDate
        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required")
            .GreaterThanOrEqualTo(DateTime.UtcNow.AddHours(24))
            .WithMessage("Start date must be at least 24 hours in the future");

        // Validate EndDate
        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("End date is required")
            .GreaterThan(x => x.StartDate)
            .WithMessage("End date must be on the same day or after the start date");

        // Validate that EndDate is not before StartDate's date
        RuleFor(x => x)
            .Must(x => x.EndDate.Date >= x.StartDate.Date)
            .WithMessage("End date must be on the same day or after the start date")
            .When(x => x.StartDate != default && x.EndDate != default);
    }
}