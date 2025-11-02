using FluentValidation.TestHelper;
using WebAPI.DTOs.Activities;
using WebAPI.Validators.Activities.Ownership;
using Xunit;

namespace Tests.Validators;

/// <summary>
/// Unit tests for CreateActivityValidator.
/// Validates that all business rules for activity creation are correctly enforced.
/// </summary>
public class CreateActivityValidatorTests
{
    private readonly CreateActivityValidator _validator;

    public CreateActivityValidatorTests()
    {
        _validator = new CreateActivityValidator();
    }

    #region AnimalId Tests

    [Fact]
    public void Validate_ShouldHaveError_WhenAnimalIdIsNull()
    {
        var dto = new ReqCreateActivityDto
        {
            AnimalId = null!,
            StartDate = DateTime.UtcNow.AddDays(2),
            EndDate = DateTime.UtcNow.AddDays(2).AddHours(2)
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.AnimalId);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenAnimalIdIsEmpty()
    {
        var dto = new ReqCreateActivityDto
        {
            AnimalId = string.Empty,
            StartDate = DateTime.UtcNow.AddDays(2),
            EndDate = DateTime.UtcNow.AddDays(2).AddHours(2)
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.AnimalId);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenAnimalIdIsWhitespace()
    {
        var dto = new ReqCreateActivityDto
        {
            AnimalId = "   ",
            StartDate = DateTime.UtcNow.AddDays(2),
            EndDate = DateTime.UtcNow.AddDays(2).AddHours(2)
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.AnimalId);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenAnimalIdIsNotValidGuid()
    {
        var dto = new ReqCreateActivityDto
        {
            AnimalId = "not-a-valid-guid",
            StartDate = DateTime.UtcNow.AddDays(2),
            EndDate = DateTime.UtcNow.AddDays(2).AddHours(2)
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.AnimalId);
    }

    [Fact]
    public void Validate_ShouldNotHaveError_WhenAnimalIdIsValidGuid()
    {
        var dto = new ReqCreateActivityDto
        {
            AnimalId = Guid.NewGuid().ToString(),
            StartDate = DateTime.UtcNow.AddDays(2),
            EndDate = DateTime.UtcNow.AddDays(2).AddHours(2)
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.AnimalId);
    }

    [Fact]
    public void Validate_ShouldNotHaveError_WhenAnimalIdIsValidGuidWithoutHyphens()
    {
        var dto = new ReqCreateActivityDto
        {
            AnimalId = Guid.NewGuid().ToString("N"),
            StartDate = DateTime.UtcNow.AddDays(2),
            EndDate = DateTime.UtcNow.AddDays(2).AddHours(2)
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.AnimalId);
    }

    #endregion

    #region StartDate Tests

    [Fact]
    public void Validate_ShouldHaveError_WhenStartDateIsEmpty()
    {
        var dto = new ReqCreateActivityDto
        {
            AnimalId = Guid.NewGuid().ToString(),
            StartDate = default,
            EndDate = DateTime.UtcNow.AddDays(2)
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.StartDate);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenStartDateIsLessThan24HoursInFuture()
    {
        var dto = new ReqCreateActivityDto
        {
            AnimalId = Guid.NewGuid().ToString(),
            StartDate = DateTime.UtcNow.AddHours(23),
            EndDate = DateTime.UtcNow.AddHours(25)
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.StartDate);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenStartDateIsInThePast()
    {
        var dto = new ReqCreateActivityDto
        {
            AnimalId = Guid.NewGuid().ToString(),
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddDays(1)
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.StartDate);
    }

    [Fact]
    public void Validate_ShouldNotHaveError_WhenStartDateIsExactly24HoursInFuture()
    {
        var dto = new ReqCreateActivityDto
        {
            AnimalId = Guid.NewGuid().ToString(),
            StartDate = DateTime.UtcNow.AddHours(24),
            EndDate = DateTime.UtcNow.AddHours(26)
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.StartDate);
    }

    [Fact]
    public void Validate_ShouldNotHaveError_WhenStartDateIsMoreThan24HoursInFuture()
    {
        var dto = new ReqCreateActivityDto
        {
            AnimalId = Guid.NewGuid().ToString(),
            StartDate = DateTime.UtcNow.AddDays(2),
            EndDate = DateTime.UtcNow.AddDays(2).AddHours(2)
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.StartDate);
    }

    #endregion

    #region EndDate Tests

    [Fact]
    public void Validate_ShouldHaveError_WhenEndDateIsEmpty()
    {
        var dto = new ReqCreateActivityDto
        {
            AnimalId = Guid.NewGuid().ToString(),
            StartDate = DateTime.UtcNow.AddDays(2),
            EndDate = default
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.EndDate);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenEndDateIsBeforeStartDate()
    {
        var dto = new ReqCreateActivityDto
        {
            AnimalId = Guid.NewGuid().ToString(),
            StartDate = DateTime.UtcNow.AddDays(2),
            EndDate = DateTime.UtcNow.AddDays(1)
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.EndDate);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenEndDateEqualsStartDate()
    {
        var startDate = DateTime.UtcNow.AddDays(2);
        var dto = new ReqCreateActivityDto
        {
            AnimalId = Guid.NewGuid().ToString(),
            StartDate = startDate,
            EndDate = startDate
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.EndDate);
    }

    [Fact]
    public void Validate_ShouldNotHaveError_WhenEndDateIsAfterStartDate()
    {
        var dto = new ReqCreateActivityDto
        {
            AnimalId = Guid.NewGuid().ToString(),
            StartDate = DateTime.UtcNow.AddDays(2),
            EndDate = DateTime.UtcNow.AddDays(2).AddHours(2)
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.EndDate);
    }

    #endregion

    #region Date Validation Tests

    [Fact]
    public void Validate_ShouldHaveError_WhenEndDateIsOnDayBeforeStartDate()
    {
        var dto = new ReqCreateActivityDto
        {
            AnimalId = Guid.NewGuid().ToString(),
            StartDate = new DateTime(2025, 1, 5, 10, 0, 0, DateTimeKind.Utc),
            EndDate = new DateTime(2025, 1, 4, 14, 0, 0, DateTimeKind.Utc)
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x);
    }

    [Fact]
    public void Validate_ShouldNotHaveError_WhenBothDatesAreOnSameDay()
    {
        var dto = new ReqCreateActivityDto
        {
            AnimalId = Guid.NewGuid().ToString(),
            StartDate = new DateTime(2025, 12, 1, 10, 0, 0, DateTimeKind.Utc),
            EndDate = new DateTime(2025, 12, 1, 14, 0, 0, DateTimeKind.Utc)
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x);
    }

    [Fact]
    public void Validate_ShouldNotHaveError_WhenEndDateIsOnDayAfterStartDate()
    {
        var dto = new ReqCreateActivityDto
        {
            AnimalId = Guid.NewGuid().ToString(),
            StartDate = new DateTime(2025, 12, 1, 10, 0, 0, DateTimeKind.Utc),
            EndDate = new DateTime(2025, 12, 3, 14, 0, 0, DateTimeKind.Utc)
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x);
    }

    #endregion

    #region Complete Valid Scenario

    [Fact]
    public void Validate_ShouldNotHaveAnyErrors_WhenAllFieldsAreValid()
    {
        var dto = new ReqCreateActivityDto
        {
            AnimalId = Guid.NewGuid().ToString(),
            StartDate = DateTime.UtcNow.AddDays(2),
            EndDate = DateTime.UtcNow.AddDays(2).AddHours(3)
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }

    #endregion
}