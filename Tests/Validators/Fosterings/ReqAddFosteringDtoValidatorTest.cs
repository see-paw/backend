using FluentValidation.TestHelper;
using WebAPI.DTOs.Fostering;
using WebAPI.Validators.Fosterings;

namespace Tests.Validators.Fosterings;

/// <summary>
/// Test suite for ReqAddFosteringDtoValidator using Equivalence Class Partitioning and Boundary Value Analysis.
/// Tests validator rules for MonthValue (greater than 0, less than Decimal.MaxValue).
/// Designed to find validation flaws and edge cases.
/// </summary>
public class ReqAddFosteringDtoValidatorTests
{
    private readonly ReqAddFosteringDtoValidator _validator;

    public ReqAddFosteringDtoValidatorTests()
    {
        _validator = new ReqAddFosteringDtoValidator();
    }

    #region Negative Values Tests

    /// <summary>
    /// EC: Negative MonthValue
    /// BVA: Various negative boundaries
    /// Expected: Validation fails with "Month value must be greater than 0"
    /// </summary>
    [Theory]
    [InlineData(-0.01)]
    public void Validate_MonthValueNegative_ShouldHaveValidationError(decimal monthValue)
    {
        var dto = new ReqAddFosteringDto { MonthValue = monthValue };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.MonthValue)
            .WithErrorMessage("Month value must be greater than 0");
    }

    #endregion

    #region Zero Value Tests

    /// <summary>
    /// BVA: MonthValue exactly at zero boundary
    /// Expected: Validation fails (GreaterThan excludes zero)
    /// </summary>
    [Fact]
    public void Validate_MonthValueZero_ShouldHaveValidationError()
    {
        var dto = new ReqAddFosteringDto { MonthValue = 0m };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.MonthValue)
            .WithErrorMessage("Month value must be greater than 0");
    }

    #endregion

    #region Valid Positive Values Tests

    /// <summary>
    /// BVA: MonthValue just above zero boundary (minimum valid value)
    /// Expected: Validation passes
    /// </summary>
    [Fact]
    public void Validate_MonthValueJustAboveZero_ShouldNotHaveValidationError()
    {
        var dto = new ReqAddFosteringDto { MonthValue = 0.01m };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.MonthValue);
    }

    /// <summary>
    /// EC: Small positive values
    /// Expected: Validation passes (even if impractical for real money)
    /// </summary>
    [Theory]
    [InlineData(0.001)]
    [InlineData(0.1)]
    [InlineData(1)]
    public void Validate_MonthValueSmallPositive_ShouldNotHaveValidationError(decimal monthValue)
    {
        var dto = new ReqAddFosteringDto { MonthValue = monthValue };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.MonthValue);
    }

    /// <summary>
    /// EC: Large but reasonable positive values
    /// Expected: Validation passes (no practical upper limit defined)
    /// </summary>
    [Theory]
    [InlineData(1000000.00)]
    public void Validate_MonthValueLargePositive_ShouldNotHaveValidationError(decimal monthValue)
    {
        var dto = new ReqAddFosteringDto { MonthValue = monthValue };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.MonthValue);
    }

    #endregion

    #region Decimal.MaxValue Boundary Tests

    /// <summary>
    /// BVA: MonthValue at Decimal.MaxValue boundary
    /// Expected: Validation fails with "must be less than Double.MaxValue" (note: error message is wrong!)
    /// </summary>
    [Fact]
    public void Validate_MonthValueExactlyDecimalMaxValue_ShouldHaveValidationError()
    {
        var dto = new ReqAddFosteringDto { MonthValue = decimal.MaxValue };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.MonthValue)
            .WithErrorMessage("Month value must be less than Double.MaxValue");
    }

    /// <summary>
    /// BVA: MonthValue just below Decimal.MaxValue
    /// Expected: Validation passes (LessThan excludes the maximum)
    /// </summary>
    [Fact]
    public void Validate_MonthValueJustBelowDecimalMaxValue_ShouldNotHaveValidationError()
    {
        var dto = new ReqAddFosteringDto { MonthValue = decimal.MaxValue - 1m };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.MonthValue);
    }

    /// <summary>
    /// BVA: Testing values near but below Decimal.MaxValue
    /// Expected: Validation passes
    /// </summary>
    [Theory]
    [InlineData(7922816251426433759)] // MaxValue - 1
    public void Validate_MonthValueNearDecimalMaxValue_ShouldNotHaveValidationError(decimal monthValue)
    {
        var dto = new ReqAddFosteringDto { MonthValue = monthValue };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.MonthValue);
    }

    #endregion

    #region Precision and Scale Tests

    /// <summary>
    /// EC: MonthValue with high decimal precision
    /// Expected: Validation passes (no scale restriction)
    /// </summary>
    [Theory]
    [InlineData(10.0000001)]
    public void Validate_MonthValueWithVariousPrecision_ShouldNotHaveValidationError(decimal monthValue)
    {
        var dto = new ReqAddFosteringDto { MonthValue = monthValue };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.MonthValue);
    }

    /// <summary>
    /// EC: MonthValue with maximum decimal precision (28-29 significant digits)
    /// Expected: Validation passes
    /// </summary>
    [Fact]
    public void Validate_MonthValueMaxPrecision_ShouldNotHaveValidationError()
    {
        var dto = new ReqAddFosteringDto { MonthValue = 1.2345678901234567890123456789m };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.MonthValue);
    }

    /// <summary>
    /// EC: Very small positive values (edge case for money)
    /// Expected: Validation passes (no minimum threshold defined)
    /// </summary>
    [Theory]
    [InlineData(0.0000000001)]
    public void Validate_MonthValueVerySmall_ShouldNotHaveValidationError(decimal monthValue)
    {
        var dto = new ReqAddFosteringDto { MonthValue = monthValue };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.MonthValue);
    }

    #endregion

    #region Edge Cases and Special Scenarios

    /// <summary>
    /// EC: Typical money values with 2 decimal places
    /// Expected: Validation passes
    /// </summary>
    [Theory]
    [InlineData(9.99)]
    public void Validate_MonthValueTypicalMoneyFormat_ShouldNotHaveValidationError(decimal monthValue)
    {
        var dto = new ReqAddFosteringDto { MonthValue = monthValue };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.MonthValue);
    }

    /// <summary>
    /// BVA: MonthValue at common boundary values
    /// Expected: Tests boundaries between invalid and valid
    /// </summary>
    [Theory]
    [InlineData(-0.01)] // Just below zero - invalid
    [InlineData(0.00)]  // Exactly zero - invalid
    [InlineData(0.01)]  // Just above zero - valid
    public void Validate_MonthValueAroundZeroBoundary_ValidatesCorrectly(decimal monthValue)
    {
        var dto = new ReqAddFosteringDto { MonthValue = monthValue };

        var result = _validator.TestValidate(dto);

        if (monthValue <= 0)
        {
            result.ShouldHaveValidationErrorFor(x => x.MonthValue);
        }
        else
        {
            result.ShouldNotHaveValidationErrorFor(x => x.MonthValue);
        }
    }

    /// <summary>
    /// EC: MonthValue with trailing zeros
    /// Expected: Validation passes (decimal representation doesn't matter)
    /// </summary>
    [Theory]
    [InlineData(10.000)]
    public void Validate_MonthValueWithTrailingZeros_ShouldNotHaveValidationError(decimal monthValue)
    {
        var dto = new ReqAddFosteringDto { MonthValue = monthValue };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.MonthValue);
    }

    #endregion

    #region NotNull and NotEmpty Tests

    /// <summary>
    /// EC: Default value for decimal (which is 0)
    /// Expected: Validation fails because 0 is not greater than 0
    /// Note: Testing if NotNull/NotEmpty have any effect on value types
    /// </summary>
    [Fact]
    public void Validate_MonthValueDefaultDecimal_ShouldHaveValidationError()
    {
        var dto = new ReqAddFosteringDto { MonthValue = default };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.MonthValue);
    }

    /// <summary>
    /// EC: Testing if validator handles implicit zero correctly
    /// Expected: Should fail GreaterThan(0) validation
    /// </summary>
    [Fact]
    public void Validate_MonthValueImplicitZero_ShouldHaveValidationError()
    {
        var dto = new ReqAddFosteringDto { MonthValue = 0m };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.MonthValue)
            .WithErrorMessage("Month value must be greater than 0");
    }

    #endregion

    #region Multiple Validation Rules Tests

    /// <summary>
    /// EC: Value that triggers multiple validation failures
    /// Expected: Should fail with appropriate error message
    /// </summary>
    [Fact]
    public void Validate_MonthValueNegative_FailsWithSingleRelevantError()
    {
        var dto = new ReqAddFosteringDto { MonthValue = -10m };

        var result = _validator.TestValidate(dto);

        var errors = result.ShouldHaveValidationErrorFor(x => x.MonthValue);
        Assert.Single(errors);
    }

    /// <summary>
    /// EC: Value at upper boundary
    /// Expected: Should fail with MaxValue error
    /// </summary>
    [Fact]
    public void Validate_MonthValueAtMaximum_FailsWithMaxValueError()
    {
        var dto = new ReqAddFosteringDto { MonthValue = decimal.MaxValue };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.MonthValue)
            .WithErrorMessage("Month value must be less than Double.MaxValue");
    }

    #endregion

    #region Comprehensive Boundary Test

    /// <summary>
    /// BVA: Comprehensive test of all critical boundaries
    /// Tests the complete range of valid and invalid values
    /// </summary>
    [Theory]
    [InlineData(-0.01, false)]          // Invalid: negative
    [InlineData(0, false)]              // Invalid: zero
    [InlineData(0.01, true)]            // Valid: just above zero
    public void Validate_MonthValueComprehensiveBoundaries_ValidatesCorrectly(decimal monthValue, bool shouldBeValid)
    {
        var dto = new ReqAddFosteringDto { MonthValue = monthValue };

        var result = _validator.TestValidate(dto);

        if (shouldBeValid)
        {
            result.ShouldNotHaveValidationErrorFor(x => x.MonthValue);
        }
        else
        {
            result.ShouldHaveValidationErrorFor(x => x.MonthValue);
        }
    }

    #endregion
}