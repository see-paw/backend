using FluentValidation.TestHelper;
using WebAPI.DTOs.Ownership;
using WebAPI.Validators.Ownership;
using Xunit;

namespace Tests.Validators;

public class UpdateOwnershipStatusValidatorTests
{
    private readonly UpdateOwnershipStatusValidator _validator;

    public UpdateOwnershipStatusValidatorTests()
    {
        _validator = new UpdateOwnershipStatusValidator();
    }

    [Fact]
    public void Validate_ShouldNotHaveError_WhenRequestInfoIsNull()
    {
        var dto = new ReqUpdateOwnershipStatusDto
        {
            RequestInfo = null
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.RequestInfo);
    }

    [Fact]
    public void Validate_ShouldNotHaveError_WhenRequestInfoIsEmpty()
    {
        var dto = new ReqUpdateOwnershipStatusDto
        {
            RequestInfo = string.Empty
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.RequestInfo);
    }

    [Fact]
    public void Validate_ShouldNotHaveError_WhenRequestInfoHasExactly500Characters()
    {
        var dto = new ReqUpdateOwnershipStatusDto
        {
            RequestInfo = new string('a', 500)
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.RequestInfo);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenRequestInfoExceeds500Characters()
    {
        var dto = new ReqUpdateOwnershipStatusDto
        {
            RequestInfo = new string('a', 501)
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.RequestInfo);
    }

    [Fact]
    public void Validate_ShouldNotHaveError_WhenRequestInfoHasLessThan500Characters()
    {
        var dto = new ReqUpdateOwnershipStatusDto
        {
            RequestInfo = "Request is under review and requires additional documentation"
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.RequestInfo);
    }
}