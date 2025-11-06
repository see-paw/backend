using FluentValidation;
using WebAPI.DTOs.Activities;

namespace WebAPI.Validators.Activities.Fostering;

/// <summary>
/// Validator for the ReqCreateActivityFosteringDto.
/// </summary>
/// <remarks>
/// Validates basic format requirements and business rules for scheduling a visit,
/// including time constraints, duration limits, and advance booking requirements.
/// </remarks>
public class ReqCreateActivityFostValidator : AbstractValidator<ReqCreateActivityFosteringDto>
{
    private const int MinimumDurationHours = 1;
    private const int MaximumDurationHours = 3;
    private const int MinimumAdvanceHours = 24;

    /// <summary>
    /// Initializes validation rules for fostering visit scheduling.
    /// </summary>
    public ReqCreateActivityFostValidator()
    {
        // AnimalId validation
        RuleFor(x => x.AnimalId)
            .NotEmpty()
            .WithMessage("AnimalId is required.")
            .MaximumLength(36)
            .WithMessage("AnimalId cannot exceed 36 characters.")
            .Must(BeAValidGuid)
            .WithMessage("AnimalId must be a valid GUID.");

        // StartDateTime validation
        RuleFor(x => x.StartDateTime)
            .NotEmpty()
            .WithMessage("StartDateTime is required.")
            .Must(BeAValidDateTime)
            .WithMessage("StartDateTime must be a valid date and time.")
            .Must(BeInTheFuture)
            .WithMessage("StartDateTime must be in the future.")
            .Must(HaveMinimumAdvance)
            .WithMessage($"Visit must be scheduled at least {MinimumAdvanceHours} hours in advance.");

        // EndDateTime validation
        RuleFor(x => x.EndDateTime)
            .NotEmpty()
            .WithMessage("EndDateTime is required.")
            .Must(BeAValidDateTime)
            .WithMessage("EndDateTime must be a valid date and time.");

        // StartDateTime < EndDateTime
        RuleFor(x => x)
            .Must(x => x.EndDateTime > x.StartDateTime)
            .WithMessage("EndDateTime must be after StartDateTime.");

        // Duration validation (1-3 hours)
        RuleFor(x => x)
            .Must(HaveValidDuration)
            .WithMessage($"Visit duration must be between {MinimumDurationHours} and {MaximumDurationHours} hours.");

        // Same day validation (cannot span multiple days)
        RuleFor(x => x)
            .Must(BeOnSameDay)
            .WithMessage("Visit must start and end on the same day.");
    }

    /// <summary>
    /// Validates if the string is a valid GUID format.
    /// </summary>
    private bool BeAValidGuid(string guid)
    {
        return Guid.TryParse(guid, out _);
    }

    /// <summary>
    /// Validates if the DateTime is not default/empty.
    /// </summary>
    private bool BeAValidDateTime(DateTime dateTime)
    {
        return dateTime != default && dateTime != DateTime.MinValue;
    }

    /// <summary>
    /// Validates if the DateTime is in the future (converted to UTC).
    /// </summary>
    private bool BeInTheFuture(DateTime dateTime)
    {
        var utcDateTime = dateTime.Kind == DateTimeKind.Utc
            ? dateTime
            : dateTime.ToUniversalTime();

        return utcDateTime > DateTime.UtcNow;
    }

    /// <summary>
    /// Validates if the visit has at least 24 hours advance notice.
    /// </summary>
    private bool HaveMinimumAdvance(DateTime startDateTime)
    {
        var utcStartDateTime = startDateTime.Kind == DateTimeKind.Utc
            ? startDateTime
            : startDateTime.ToUniversalTime();

        var minimumAllowedTime = DateTime.UtcNow.AddHours(MinimumAdvanceHours);

        return utcStartDateTime >= minimumAllowedTime;
    }

    /// <summary>
    /// Validates if the visit duration is between 1 and 3 hours.
    /// </summary>
    private bool HaveValidDuration(ReqCreateActivityFosteringDto dto)
    {
        if (dto.EndDateTime <= dto.StartDateTime)
            return false;

        var duration = dto.EndDateTime - dto.StartDateTime;

        return duration.TotalHours >= MinimumDurationHours
               && duration.TotalHours <= MaximumDurationHours;
    }

    /// <summary>
    /// Validates if the visit starts and ends on the same day.
    /// </summary>
    private bool BeOnSameDay(ReqCreateActivityFosteringDto dto)
    {
        var startDate = dto.StartDateTime.Kind == DateTimeKind.Utc
            ? dto.StartDateTime
            : dto.StartDateTime.ToUniversalTime();

        var endDate = dto.EndDateTime.Kind == DateTimeKind.Utc
            ? dto.EndDateTime
            : dto.EndDateTime.ToUniversalTime();

        return startDate.Date == endDate.Date;
    }
}