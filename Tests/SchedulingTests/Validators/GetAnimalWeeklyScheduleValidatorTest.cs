using Application.Scheduling.Queries;
using FluentValidation.TestHelper;
using WebAPI.Validators.Scheduling;

namespace Tests.Scheduling.Validators;

/// <summary>
/// Tests for GetAnimalWeeklyScheduleValidator using equivalence class partitioning and boundary value analysis.
/// Focuses on AnimalId validation, date range boundaries, and day-of-week constraints.
/// </summary>
public class GetAnimalWeeklyScheduleValidatorTests
{
    private readonly GetAnimalWeeklyScheduleValidator _validator;

    public GetAnimalWeeklyScheduleValidatorTests()
    {
        _validator = new GetAnimalWeeklyScheduleValidator();
    }

    #region AnimalId - Equivalence Classes

    [Fact]
    public void Validate_ValidAnimalId_PassesValidation()
    {
        var query = new GetAnimalWeeklySchedule.Query
        {
            AnimalId = Guid.NewGuid().ToString(),
            StartDate = GetNextMonday(DateOnly.FromDateTime(DateTime.UtcNow))
        };

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveValidationErrorFor(x => x.AnimalId);
    }

    [Fact]
    public void Validate_NullAnimalId_FailsValidation()
    {
        var query = new GetAnimalWeeklySchedule.Query
        {
            AnimalId = null!,
            StartDate = GetNextMonday(DateOnly.FromDateTime(DateTime.UtcNow))
        };

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.AnimalId);
    }

    [Fact]
    public void Validate_EmptyAnimalId_FailsValidation()
    {
        var query = new GetAnimalWeeklySchedule.Query
        {
            AnimalId = string.Empty,
            StartDate = GetNextMonday(DateOnly.FromDateTime(DateTime.UtcNow))
        };

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.AnimalId);
    }

    [Fact]
    public void Validate_WhitespaceAnimalId_FailsValidation()
    {
        var query = new GetAnimalWeeklySchedule.Query
        {
            AnimalId = "   ",
            StartDate = GetNextMonday(DateOnly.FromDateTime(DateTime.UtcNow))
        };

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.AnimalId);
    }

    [Fact]
    public void Validate_InvalidGuidFormatAnimalId_FailsValidation()
    {
        var query = new GetAnimalWeeklySchedule.Query
        {
            AnimalId = "not-a-valid-guid",
            StartDate = GetNextMonday(DateOnly.FromDateTime(DateTime.UtcNow))
        };

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.AnimalId);
    }

    [Fact]
    public void Validate_PartialGuidAnimalId_FailsValidation()
    {
        var query = new GetAnimalWeeklySchedule.Query
        {
            AnimalId = "12345678-1234-1234",
            StartDate = GetNextMonday(DateOnly.FromDateTime(DateTime.UtcNow))
        };

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.AnimalId);
    }

    [Fact]
    public void Validate_GuidWithExtraCharacters_FailsValidation()
    {
        var guid = Guid.NewGuid().ToString();
        var query = new GetAnimalWeeklySchedule.Query
        {
            AnimalId = guid + "extra",
            StartDate = GetNextMonday(DateOnly.FromDateTime(DateTime.UtcNow))
        };

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.AnimalId);
    }

    [Fact]
    public void Validate_AllZerosGuid_PassesOrFailsBasedOnImplementation()
    {
        var query = new GetAnimalWeeklySchedule.Query
        {
            AnimalId = "00000000-0000-0000-0000-000000000000",
            StartDate = GetNextMonday(DateOnly.FromDateTime(DateTime.UtcNow))
        };

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveValidationErrorFor(x => x.AnimalId);
    }

    [Fact]
    public void Validate_UppercaseGuid_PassesValidation()
    {
        var query = new GetAnimalWeeklySchedule.Query
        {
            AnimalId = Guid.NewGuid().ToString().ToUpperInvariant(),
            StartDate = GetNextMonday(DateOnly.FromDateTime(DateTime.UtcNow))
        };

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveValidationErrorFor(x => x.AnimalId);
    }

    #endregion

    #region StartDate Empty - Equivalence Classes

    [Fact]
    public void Validate_DefaultStartDate_FailsValidation()
    {
        var query = new GetAnimalWeeklySchedule.Query
        {
            AnimalId = Guid.NewGuid().ToString(),
            StartDate = default
        };

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.StartDate);
    }

    [Fact]
    public void Validate_MinValueStartDate_FailsValidation()
    {
        var query = new GetAnimalWeeklySchedule.Query
        {
            AnimalId = Guid.NewGuid().ToString(),
            StartDate = DateOnly.MinValue
        };

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.StartDate);
    }

    #endregion

    #region StartDate Range - Boundary Value Analysis

    [Fact]
    public void Validate_StartDateExactlyOneMonthAgoMonday_PassesValidation()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var oneMonthAgo = today.AddMonths(-1);
        var monday = GetNearestMonday(oneMonthAgo);

        var query = new GetAnimalWeeklySchedule.Query
        {
            AnimalId = Guid.NewGuid().ToString(),
            StartDate = monday
        };

        var result = _validator.TestValidate(query);

        if (monday >= today.AddMonths(-1))
        {
            result.ShouldNotHaveValidationErrorFor(x => x.StartDate);
        }
    }

    [Fact]
    public void Validate_StartDateOneDayBeforeOneMonthAgo_FailsValidation()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var justBeforeRange = today.AddMonths(-1).AddDays(-1);
        var monday = GetNearestMondayBefore(justBeforeRange);

        var query = new GetAnimalWeeklySchedule.Query
        {
            AnimalId = Guid.NewGuid().ToString(),
            StartDate = monday
        };

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.StartDate);
    }

    [Fact]
    public void Validate_StartDateExactlyOneYearFromTodayMonday_PassesValidation()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var oneYearFromNow = today.AddYears(1);
        var monday = GetNearestMonday(oneYearFromNow);

        var query = new GetAnimalWeeklySchedule.Query
        {
            AnimalId = Guid.NewGuid().ToString(),
            StartDate = monday
        };

        var result = _validator.TestValidate(query);

        if (monday <= today.AddYears(1))
        {
            result.ShouldNotHaveValidationErrorFor(x => x.StartDate);
        }
    }

    [Fact]
    public void Validate_StartDateOneDayAfterOneYear_FailsValidation()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var justAfterRange = today.AddYears(1).AddDays(1);
        var monday = GetNearestMondayAfter(justAfterRange);

        var query = new GetAnimalWeeklySchedule.Query
        {
            AnimalId = Guid.NewGuid().ToString(),
            StartDate = monday
        };

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.StartDate);
    }

    [Fact]
    public void Validate_StartDateTodayIfMonday_PassesValidation()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        
        if (today.DayOfWeek != DayOfWeek.Monday)
        {
            return;
        }

        var query = new GetAnimalWeeklySchedule.Query
        {
            AnimalId = Guid.NewGuid().ToString(),
            StartDate = today
        };

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveValidationErrorFor(x => x.StartDate);
    }

    [Fact]
    public void Validate_StartDateMaxValue_FailsValidation()
    {
        var query = new GetAnimalWeeklySchedule.Query
        {
            AnimalId = Guid.NewGuid().ToString(),
            StartDate = DateOnly.MaxValue
        };

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.StartDate);
    }

    [Fact]
    public void Validate_StartDateWellWithinRange_PassesValidation()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var validDate = today.AddDays(7);
        var monday = GetNextMonday(validDate);

        var query = new GetAnimalWeeklySchedule.Query
        {
            AnimalId = Guid.NewGuid().ToString(),
            StartDate = monday
        };

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveValidationErrorFor(x => x.StartDate);
    }

    #endregion

    #region Day of Week - Equivalence Classes

    [Theory]
    [InlineData(DayOfWeek.Sunday)]
    [InlineData(DayOfWeek.Tuesday)]
    public void Validate_StartDateNotMonday_FailsValidation(DayOfWeek dayOfWeek)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var targetDate = GetNextDayOfWeek(today, dayOfWeek);

        var query = new GetAnimalWeeklySchedule.Query
        {
            AnimalId = Guid.NewGuid().ToString(),
            StartDate = targetDate
        };

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.StartDate)
            .WithErrorMessage("Start date must be a Monday.");
    }

    [Fact]
    public void Validate_StartDateIsMonday_PassesValidation()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var monday = GetNextMonday(today);

        var query = new GetAnimalWeeklySchedule.Query
        {
            AnimalId = Guid.NewGuid().ToString(),
            StartDate = monday
        };

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveValidationErrorFor(x => x.StartDate);
    }

    #endregion

    #region Combined Validations

    [Fact]
    public void Validate_MondayOutsideRange_FailsWithRangeError()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var farFutureMonday = today.AddYears(2);
        while (farFutureMonday.DayOfWeek != DayOfWeek.Monday)
        {
            farFutureMonday = farFutureMonday.AddDays(1);
        }

        var query = new GetAnimalWeeklySchedule.Query
        {
            AnimalId = Guid.NewGuid().ToString(),
            StartDate = farFutureMonday
        };

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.StartDate)
            .WithErrorMessage("Start date must be within the last month and the next year.");
    }

    [Fact]
    public void Validate_TuesdayWithinRange_FailsWithDayOfWeekError()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var tuesday = GetNextDayOfWeek(today, DayOfWeek.Tuesday);

        var query = new GetAnimalWeeklySchedule.Query
        {
            AnimalId = Guid.NewGuid().ToString(),
            StartDate = tuesday
        };

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.StartDate)
            .WithErrorMessage("Start date must be a Monday.");
    }

    [Fact]
    public void Validate_TuesdayOutsideRange_FailsWithBothErrors()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var farFutureTuesday = today.AddYears(2);
        while (farFutureTuesday.DayOfWeek != DayOfWeek.Tuesday)
        {
            farFutureTuesday = farFutureTuesday.AddDays(1);
        }

        var query = new GetAnimalWeeklySchedule.Query
        {
            AnimalId = Guid.NewGuid().ToString(),
            StartDate = farFutureTuesday
        };

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.StartDate);
    }

    [Fact]
    public void Validate_AllFieldsInvalid_FailsAllValidations()
    {
        var query = new GetAnimalWeeklySchedule.Query
        {
            AnimalId = string.Empty,
            StartDate = default
        };

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.AnimalId);
        result.ShouldHaveValidationErrorFor(x => x.StartDate);
    }

    [Fact]
    public void Validate_AllFieldsValid_PassesValidation()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var monday = GetNextMonday(today);

        var query = new GetAnimalWeeklySchedule.Query
        {
            AnimalId = Guid.NewGuid().ToString(),
            StartDate = monday
        };

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveAnyValidationErrors();
    }

    #endregion

    #region Date Range Edge Cases

    [Fact]
    public void Validate_LeapYearFebruary29Monday_HandlesCorrectly()
    {
        var leapYearDate = new DateOnly(2024, 2, 26);
        if (leapYearDate.DayOfWeek != DayOfWeek.Monday)
        {
            leapYearDate = GetNextMonday(leapYearDate);
        }

        var query = new GetAnimalWeeklySchedule.Query
        {
            AnimalId = Guid.NewGuid().ToString(),
            StartDate = leapYearDate
        };

        var result = _validator.TestValidate(query);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var minDate = today.AddMonths(-1);
        var maxDate = today.AddYears(1);

        if (leapYearDate >= minDate && leapYearDate <= maxDate)
        {
            result.ShouldNotHaveValidationErrorFor(x => x.StartDate);
        }
        else
        {
            result.ShouldHaveValidationErrorFor(x => x.StartDate);
        }
    }

    [Fact]
    public void Validate_EndOfMonthMonday_HandlesCorrectly()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var endOfMonth = new DateOnly(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));
        var monday = GetNearestMonday(endOfMonth);

        var query = new GetAnimalWeeklySchedule.Query
        {
            AnimalId = Guid.NewGuid().ToString(),
            StartDate = monday
        };

        var result = _validator.TestValidate(query);

        var minDate = today.AddMonths(-1);
        var maxDate = today.AddYears(1);

        if (monday >= minDate && monday <= maxDate)
        {
            result.ShouldNotHaveValidationErrorFor(x => x.StartDate);
        }
    }

    [Fact]
    public void Validate_StartOfMonthMonday_HandlesCorrectly()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var startOfMonth = new DateOnly(today.Year, today.Month, 1);
        var monday = GetNextMonday(startOfMonth);

        var query = new GetAnimalWeeklySchedule.Query
        {
            AnimalId = Guid.NewGuid().ToString(),
            StartDate = monday
        };

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveValidationErrorFor(x => x.StartDate);
    }

    [Fact]
    public void Validate_YearBoundaryMonday_HandlesCorrectly()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var yearEnd = new DateOnly(today.Year, 12, 31);
        var monday = GetNearestMonday(yearEnd);

        var query = new GetAnimalWeeklySchedule.Query
        {
            AnimalId = Guid.NewGuid().ToString(),
            StartDate = monday
        };

        var result = _validator.TestValidate(query);

        var minDate = today.AddMonths(-1);
        var maxDate = today.AddYears(1);

        if (monday >= minDate && monday <= maxDate)
        {
            result.ShouldNotHaveValidationErrorFor(x => x.StartDate);
        }
    }

    #endregion

    #region Time Component Handling

    [Fact]
    public void Validate_DateOnlyHasNoTimeComponent_ValidationNotAffected()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var monday = GetNextMonday(today);

        var query = new GetAnimalWeeklySchedule.Query
        {
            AnimalId = Guid.NewGuid().ToString(),
            StartDate = monday
        };

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveValidationErrorFor(x => x.StartDate);
    }

    #endregion

    #region Helper Methods

    private static DateOnly GetNextMonday(DateOnly date)
    {
        while (date.DayOfWeek != DayOfWeek.Monday)
        {
            date = date.AddDays(1);
        }
        return date;
    }

    private static DateOnly GetNearestMonday(DateOnly date)
    {
        var daysUntilMonday = ((int)DayOfWeek.Monday - (int)date.DayOfWeek + 7) % 7;
        if (daysUntilMonday == 0)
            return date;
        
        var daysFromMonday = ((int)date.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        
        if (daysFromMonday <= 3)
            return date.AddDays(-daysFromMonday);
        
        return date.AddDays(daysUntilMonday);
    }

    private static DateOnly GetNearestMondayBefore(DateOnly date)
    {
        while (date.DayOfWeek != DayOfWeek.Monday)
        {
            date = date.AddDays(-1);
        }
        return date;
    }

    private static DateOnly GetNearestMondayAfter(DateOnly date)
    {
        while (date.DayOfWeek != DayOfWeek.Monday)
        {
            date = date.AddDays(1);
        }
        return date;
    }

    private static DateOnly GetNextDayOfWeek(DateOnly date, DayOfWeek targetDay)
    {
        while (date.DayOfWeek != targetDay)
        {
            date = date.AddDays(1);
        }
        return date;
    }

    #endregion
}