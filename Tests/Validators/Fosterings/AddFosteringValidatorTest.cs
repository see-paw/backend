using Application.Fosterings;
using Application.Fosterings.Commands;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Options;
using Moq;
using WebAPI.Validators.Fosterings;

namespace Tests.Validators.Fosterings;

/// <summary>
/// Test suite for AddFosteringValidator using Equivalence Class Partitioning and Boundary Value Analysis.
/// Tests validator rules for AnimalId (GUID format) and MonthValue (minimum threshold).
/// </summary>
public class AddFosteringValidatorTests
{
    private readonly AddFosteringValidator _validator;
    private const decimal MinMonthlyValue = 10.00m;

    public AddFosteringValidatorTests()
    {
        var fosteringSettings = new FosteringSettings { MinMonthlyValue = MinMonthlyValue };
        var optionsMock = new Mock<IOptions<FosteringSettings>>();
        optionsMock.Setup(x => x.Value).Returns(fosteringSettings);
        
        _validator = new AddFosteringValidator(optionsMock.Object);
    }

    #region AnimalId Validation Tests

    /// <summary>
    /// EC: AnimalId is null
    /// Expected: Validation fails with "AnimalId is required."
    /// </summary>
    [Fact]
    public void Validate_AnimalIdNull_ShouldHaveValidationError()
    {
        var command = new AddFostering.Command
        {
            AnimalId = null!,
            MonthValue = 15.00m
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.AnimalId)
            .WithErrorMessage("AnimalId is required.");
    }

    /// <summary>
    /// EC: AnimalId is empty string
    /// Expected: Validation fails with "AnimalId is required."
    /// </summary>
    [Fact]
    public void Validate_AnimalIdEmpty_ShouldHaveValidationError()
    {
        var command = new AddFostering.Command
        {
            AnimalId = string.Empty,
            MonthValue = 15.00m
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.AnimalId)
            .WithErrorMessage("AnimalId is required.");
    }

    /// <summary>
    /// EC: AnimalId is whitespace
    /// Expected: May fail depending on NotEmpty vs NotNull implementation
    /// </summary>
    [Theory]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void Validate_AnimalIdWhitespace_ShouldHaveValidationError(string whitespace)
    {
        var command = new AddFostering.Command
        {
            AnimalId = whitespace,
            MonthValue = 15.00m
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.AnimalId);
    }

    /// <summary>
    /// EC: AnimalId with invalid GUID formats
    /// Expected: Validation fails with GUID format error
    /// </summary>
    [Theory]
    [InlineData("invalid-guid")]
    [InlineData("g2345678-1234-1234-1234-123456789012")]
    public void Validate_AnimalIdInvalidGuidFormat_ShouldHaveValidationError(string invalidGuid)
    {
        var command = new AddFostering.Command
        {
            AnimalId = invalidGuid,
            MonthValue = 15.00m
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.AnimalId);
    }

    /// <summary>
    /// EC: AnimalId with valid GUID format
    /// BVA: Testing various valid GUID representations
    /// </summary>
    [Theory]
    [InlineData("12345678-1234-1234-1234-123456789012")]
    [InlineData("a1234567-890a-bcde-f012-3456789abcde")]
    [InlineData("AAAAAAAA-BBBB-CCCC-DDDD-EEEEEEEEEEEE")]
    public void Validate_AnimalIdValidGuidFormat_ShouldNotHaveValidationError(string validGuid)
    {
        var command = new AddFostering.Command
        {
            AnimalId = validGuid,
            MonthValue = 15.00m
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.AnimalId);
    }

    /// <summary>
    /// EC: Edge case - Zero GUID
    /// Expected: Should be valid as it matches GUID format
    /// </summary>
    [Fact]
    public void Validate_AnimalIdZeroGuid_ShouldNotHaveValidationError()
    {
        var command = new AddFostering.Command
        {
            AnimalId = "00000000-0000-0000-0000-000000000000",
            MonthValue = 15.00m
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.AnimalId);
    }

    #endregion

    #region MonthValue Validation Tests

    /// <summary>
    /// BVA: MonthValue with negative values
    /// EC: Below minimum threshold
    /// Expected: Validation fails
    /// </summary>
    [Theory]
    [InlineData(-0.01)]
    public void Validate_MonthValueNegative_ShouldHaveValidationError(decimal monthValue)
    {
        var command = new AddFostering.Command
        {
            AnimalId = "12345678-1234-1234-1234-123456789012",
            MonthValue = monthValue
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.MonthValue);
    }

    /// <summary>
    /// BVA: MonthValue exactly zero
    /// EC: Below minimum threshold
    /// Expected: Validation fails
    /// </summary>
    [Fact]
    public void Validate_MonthValueZero_ShouldHaveValidationError()
    {
        var command = new AddFostering.Command
        {
            AnimalId = "12345678-1234-1234-1234-123456789012",
            MonthValue = 0m
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.MonthValue);
    }

    /// <summary>
    /// BVA: MonthValue positive but below minimum
    /// EC: Below minimum threshold
    /// Expected: Validation fails
    /// </summary>
    [Theory]
    [InlineData(0.01)]
    [InlineData(9.99)]
    public void Validate_MonthValueBelowMinimum_ShouldHaveValidationError(decimal monthValue)
    {
        var command = new AddFostering.Command
        {
            AnimalId = "12345678-1234-1234-1234-123456789012",
            MonthValue = monthValue
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.MonthValue);
    }

    /// <summary>
    /// BVA: MonthValue exactly at minimum boundary
    /// Expected: Validation passes (GreaterThanOrEqualTo includes boundary)
    /// </summary>
    [Fact]
    public void Validate_MonthValueExactlyAtMinimum_ShouldNotHaveValidationError()
    {
        var command = new AddFostering.Command
        {
            AnimalId = "12345678-1234-1234-1234-123456789012",
            MonthValue = MinMonthlyValue
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.MonthValue);
    }

    /// <summary>
    /// BVA: MonthValue just above minimum boundary
    /// Expected: Validation passes
    /// </summary>
    [Fact]
    public void Validate_MonthValueJustAboveMinimum_ShouldNotHaveValidationError()
    {
        var command = new AddFostering.Command
        {
            AnimalId = "12345678-1234-1234-1234-123456789012",
            MonthValue = 10.01m
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.MonthValue);
    }

    /// <summary>
    /// EC: MonthValue well above minimum
    /// Expected: Validation passes
    /// </summary>
    [Theory]
    [InlineData(99999.99)]
    public void Validate_MonthValueAboveMinimum_ShouldNotHaveValidationError(decimal monthValue)
    {
        var command = new AddFostering.Command
        {
            AnimalId = "12345678-1234-1234-1234-123456789012",
            MonthValue = monthValue
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.MonthValue);
    }

    /// <summary>
    /// BVA: MonthValue at maximum decimal boundary
    /// Expected: Validation passes (no upper limit defined)
    /// </summary>
    [Fact]
    public void Validate_MonthValueMaxDecimal_ShouldNotHaveValidationError()
    {
        var command = new AddFostering.Command
        {
            AnimalId = "12345678-1234-1234-1234-123456789012",
            MonthValue = decimal.MaxValue
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.MonthValue);
    }

    /// <summary>
    /// EC: MonthValue with high precision
    /// Expected: Validation passes if >= minimum
    /// </summary>
    [Theory]
    [InlineData(10.123456789)]
    public void Validate_MonthValueWithPrecision_ShouldNotHaveValidationError(decimal monthValue)
    {
        var command = new AddFostering.Command
        {
            AnimalId = "12345678-1234-1234-1234-123456789012",
            MonthValue = monthValue
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.MonthValue);
    }

    #endregion

    #region Combined Validation Tests

    /// <summary>
    /// EC: Both fields invalid
    /// Expected: Both validations fail
    /// </summary>
    [Fact]
    public void Validate_BothFieldsInvalid_ShouldHaveMultipleValidationErrors()
    {
        var command = new AddFostering.Command
        {
            AnimalId = "invalid-guid",
            MonthValue = -5.00m
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.AnimalId);
        result.ShouldHaveValidationErrorFor(x => x.MonthValue);
    }

    /// <summary>
    /// EC: Both fields at boundary valid values
    /// Expected: No validation errors
    /// </summary>
    [Fact]
    public void Validate_BothFieldsAtBoundaryValid_ShouldNotHaveValidationErrors()
    {
        var command = new AddFostering.Command
        {
            AnimalId = "00000000-0000-0000-0000-000000000000",
            MonthValue = MinMonthlyValue
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// EC: AnimalId valid, MonthValue invalid
    /// Expected: Only MonthValue validation fails
    /// </summary>
    [Fact]
    public void Validate_ValidAnimalIdInvalidMonthValue_ShouldHaveMonthValueError()
    {
        var command = new AddFostering.Command
        {
            AnimalId = "12345678-1234-1234-1234-123456789012",
            MonthValue = 9.99m
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.AnimalId);
        result.ShouldHaveValidationErrorFor(x => x.MonthValue);
    }

    /// <summary>
    /// EC: AnimalId invalid, MonthValue valid
    /// Expected: Only AnimalId validation fails
    /// </summary>
    [Fact]
    public void Validate_InvalidAnimalIdValidMonthValue_ShouldHaveAnimalIdError()
    {
        var command = new AddFostering.Command
        {
            AnimalId = "not-a-guid",
            MonthValue = 15.00m
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.AnimalId);
        result.ShouldNotHaveValidationErrorFor(x => x.MonthValue);
    }

    #endregion

    #region Edge Cases and Special Scenarios

    /// <summary>
    /// EC: MonthValue just below minimum boundary by smallest increment
    /// Expected: Validation fails
    /// </summary>
    [Fact]
    public void Validate_MonthValueJustBelowMinimumBoundary_ShouldHaveValidationError()
    {
        var command = new AddFostering.Command
        {
            AnimalId = "12345678-1234-1234-1234-123456789012",
            MonthValue = 9.99m
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.MonthValue);
    }

    /// <summary>
    /// EC: AnimalId with mixed case (valid GUID)
    /// Expected: Should be valid if GUID parser is case-insensitive
    /// </summary>
    [Fact]
    public void Validate_AnimalIdMixedCase_ShouldNotHaveValidationError()
    {
        var command = new AddFostering.Command
        {
            AnimalId = "AaBbCcDd-1234-5678-90Ab-CdEf12345678",
            MonthValue = 15.00m
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.AnimalId);
    }

    /// <summary>
    /// EC: Very long string as AnimalId
    /// Expected: Should fail GUID validation
    /// </summary>
    [Fact]
    public void Validate_AnimalIdVeryLongString_ShouldHaveValidationError()
    {
        var command = new AddFostering.Command
        {
            AnimalId = new string('a', 1000),
            MonthValue = 15.00m
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.AnimalId);
    }

    /// <summary>
    /// EC: Special characters in AnimalId
    /// Expected: Should fail GUID validation
    /// </summary>
    [Theory]
    [InlineData("!@#$%^&*()")]
    public void Validate_AnimalIdWithSpecialCharacters_ShouldHaveValidationError(string specialChars)
    {
        var command = new AddFostering.Command
        {
            AnimalId = specialChars,
            MonthValue = 15.00m
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.AnimalId);
    }

    #endregion
}