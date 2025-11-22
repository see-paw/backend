using Application.Activities.Commands;

using FluentValidation.TestHelper;

using WebAPI.Validators.Activities.Ownership;

namespace Tests.ActivitiesTests.ActivitiesOwnershipTest.ValidatorsTest;

public class CancelOwnershipActivityValidatorTests
{
    private readonly CancelOwnershipActivityValidator _validator;

    public CancelOwnershipActivityValidatorTests()
    {
        _validator = new CancelOwnershipActivityValidator();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenActivityIdIsNull()
    {
        var command = new CancelOwnershipActivity.Command
        {
            ActivityId = null!
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.ActivityId);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenActivityIdIsEmpty()
    {
        var command = new CancelOwnershipActivity.Command
        {
            ActivityId = string.Empty
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.ActivityId);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenActivityIdIsWhitespace()
    {
        var command = new CancelOwnershipActivity.Command
        {
            ActivityId = "   "
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.ActivityId);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenActivityIdHasLessThan36Characters()
    {
        var command = new CancelOwnershipActivity.Command
        {
            ActivityId = "abc123" // 6 characters - not a valid GUID
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.ActivityId);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenActivityIdHasMoreThan36Characters()
    {
        var command = new CancelOwnershipActivity.Command
        {
            ActivityId = Guid.NewGuid().ToString() + "extra" // 36 + 5 = 41 characters - not a valid GUID
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.ActivityId);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenActivityIdIs36CharactersButNotValidGuid()
    {
        var command = new CancelOwnershipActivity.Command
        {
            ActivityId = "123456789012345678901234567890123456" // 36 characters but not valid GUID format
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.ActivityId);
    }

    [Fact]
    public void Validate_ShouldNotHaveError_WhenActivityIdIsValidGuid()
    {
        var command = new CancelOwnershipActivity.Command
        {
            ActivityId = Guid.NewGuid().ToString() // Valid GUID format
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.ActivityId);
    }

    [Fact]
    public void Validate_ShouldNotHaveError_WhenActivityIdIsValidGuidWithDifferentFormat()
    {
        var command = new CancelOwnershipActivity.Command
        {
            ActivityId = Guid.NewGuid().ToString("N") // Valid GUID without hyphens (32 chars)
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.ActivityId);
    }
}