using Application.Animals.Queries;
using FluentValidation.TestHelper;
using WebAPI.Validators;

namespace Tests.AnimalControllerTest.cs;

/// <summary>
/// Contains unit tests for the <see cref="GetAnimalDetailsValidator"/> class.
/// </summary>
/// <remarks>
/// Verifies that the validator correctly handles both valid and invalid GUID formats for animal IDs.
/// </remarks>
public class GetAnimalDetailsValidatorTest
{
    private readonly GetAnimalDetailsValidator _validator;

    /// <summary>
    /// Initializes the test class and creates an instance of the validator.
    /// </summary>
    public GetAnimalDetailsValidatorTest()
    {
        _validator = new GetAnimalDetailsValidator();
    }

    /// <summary>
    /// Ensures that invalid GUID formats return validation errors.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("not-a-guid")]
    [InlineData("12345")]
    [InlineData("invalid-format")]
    public void Validate_InvalidIdFormat_ShouldHaveValidationError(string invalidId)
    {
        // Arrange
        var query = new GetAnimalDetails.Query { Id = invalidId };

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage("Animal ID must be a valid GUID.");
    }

    /// <summary>
    /// Ensures that valid GUID formats pass validation without errors.
    /// </summary>
    [Theory]
    [InlineData("123e4567-e89b-12d3-a456-426614174000")]
    public void Validate_ValidGuidFormat_ShouldNotHaveValidationError(string validGuid)
    {
        // Arrange
        var query = new GetAnimalDetails.Query { Id = validGuid };

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Id);
    }
}