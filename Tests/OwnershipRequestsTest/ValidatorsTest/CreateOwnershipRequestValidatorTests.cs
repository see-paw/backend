using FluentValidation.TestHelper;
using WebAPI.DTOs;
using WebAPI.Validators;
using Xunit;

namespace Tests.Validators;

public class CreateOwnershipRequestValidatorTests
{
    private readonly CreateOwnershipRequestValidator _validator;

    public CreateOwnershipRequestValidatorTests()
    {
        _validator = new CreateOwnershipRequestValidator();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenAnimalIdIsNull()
    {
        var dto = new ReqCreateOwnershipRequestDto
        {
            AnimalId = null!
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.AnimalId);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenAnimalIdIsEmpty()
    {
        var dto = new ReqCreateOwnershipRequestDto
        {
            AnimalId = string.Empty
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.AnimalId);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenAnimalIdIsWhitespace()
    {
        var dto = new ReqCreateOwnershipRequestDto
        {
            AnimalId = "   "
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.AnimalId);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenAnimalIdHasLessThan36Characters()
    {
        var dto = new ReqCreateOwnershipRequestDto
        {
            AnimalId = "abc123" // 6 characters - not a valid GUID
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.AnimalId);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenAnimalIdHasMoreThan36Characters()
    {
        var dto = new ReqCreateOwnershipRequestDto
        {
            AnimalId = Guid.NewGuid().ToString() + "extra" // 36 + 5 = 41 characters - not a valid GUID
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.AnimalId);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenAnimalIdIs36CharactersButNotValidGuid()
    {
        var dto = new ReqCreateOwnershipRequestDto
        {
            AnimalId = "123456789012345678901234567890123456" // 36 characters but not valid GUID format
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.AnimalId);
    }

    [Fact]
    public void Validate_ShouldNotHaveError_WhenAnimalIdIsValidGuid()
    {
        var dto = new ReqCreateOwnershipRequestDto
        {
            AnimalId = Guid.NewGuid().ToString() // Valid GUID format
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.AnimalId);
    }

    [Fact]
    public void Validate_ShouldNotHaveError_WhenAnimalIdIsValidGuidWithDifferentFormat()
    {
        var dto = new ReqCreateOwnershipRequestDto
        {
            AnimalId = Guid.NewGuid().ToString("N") // Valid GUID without hyphens (32 chars)
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.AnimalId);
    }
}