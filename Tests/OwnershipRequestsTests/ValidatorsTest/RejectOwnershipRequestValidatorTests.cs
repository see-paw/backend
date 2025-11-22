using FluentValidation.TestHelper;

using WebAPI.DTOs.Ownership;
using WebAPI.Validators.Ownership;

namespace Tests.OwnershipRequestsTests.ValidatorsTest;

public class RejectOwnershipRequestValidatorTests
{
    private readonly RejectOwnershipRequestValidator _validator;

    public RejectOwnershipRequestValidatorTests()
    {
        _validator = new RejectOwnershipRequestValidator();
    }

    [Fact]
    public void Validate_ShouldNotHaveError_WhenRejectionReasonIsNull()
    {
        var dto = new ReqRejectOwnershipRequestDto
        {
            RejectionReason = null
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.RejectionReason);
    }

    [Fact]
    public void Validate_ShouldNotHaveError_WhenRejectionReasonIsEmpty()
    {
        var dto = new ReqRejectOwnershipRequestDto
        {
            RejectionReason = string.Empty
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.RejectionReason);
    }

    [Fact]
    public void Validate_ShouldNotHaveError_WhenRejectionReasonHasExactly500Characters()
    {
        var dto = new ReqRejectOwnershipRequestDto
        {
            RejectionReason = new string('a', 500)
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.RejectionReason);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenRejectionReasonExceeds500Characters()
    {
        var dto = new ReqRejectOwnershipRequestDto
        {
            RejectionReason = new string('a', 501)
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.RejectionReason);
    }

    [Fact]
    public void Validate_ShouldNotHaveError_WhenRejectionReasonHasLessThan500Characters()
    {
        var dto = new ReqRejectOwnershipRequestDto
        {
            RejectionReason = "This is a valid rejection reason"
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.RejectionReason);
    }
}