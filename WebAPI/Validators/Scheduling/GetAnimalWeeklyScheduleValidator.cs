using Application.Scheduling.Queries;
using FluentValidation;

namespace WebAPI.Validators.Scheduling;

/// <summary>
/// Provides validation rules for the <see cref="GetAnimalWeeklySchedule.Query"/> request.
/// </summary>
public class GetAnimalWeeklyScheduleValidator : AbstractValidator<GetAnimalWeeklySchedule.Query>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetAnimalWeeklyScheduleValidator"/> class.
    /// </summary>
    public GetAnimalWeeklyScheduleValidator()
    {
        RuleFor(x => x.AnimalId)
            .NotEmpty()
            .WithMessage("Animal ID is required.")
            .MustBeValidGuidString("Animal ID");

        RuleFor(x => x.StartDate)
            .NotEmpty()
            .WithMessage("Start date is required.")
            .Must(BeWithinValidRange)
            .WithMessage("Start date must be within the last month and the next year.")
            .Must(BeAMonday)
            .WithMessage("Start date must be a Monday.");
    }

    private static bool BeWithinValidRange(DateOnly startDate)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var minDate = today.AddMonths(-1);
        var maxDate = today.AddYears(1);
        
        return startDate >= minDate && startDate <= maxDate;
    }

    private static bool BeAMonday(DateOnly startDate)
    {
        return startDate.DayOfWeek == DayOfWeek.Monday;
    }
}