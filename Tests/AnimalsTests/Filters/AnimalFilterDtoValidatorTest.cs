using FluentValidation.TestHelper;
using WebAPI.DTOs.Animals;
using WebAPI.Validators.Animals;

namespace Tests.AnimalsTests.Filters;

/// <summary>
/// Unit tests for AnimalFilterDtoValidator.
/// Tests validation rules for species, age, size, sex, and string length constraints.
/// </summary>
public class AnimalFilterDtoValidatorTests
{
    private readonly AnimalFilterDtoValidator _validator;

    public AnimalFilterDtoValidatorTests()
    {
        _validator = new AnimalFilterDtoValidator();
    }

    [Fact]
    public void Validate_WithValidSpecies_ShouldNotHaveError()
    {
        var dto = new AnimalFilterDto { Species = "Dog" };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.Species);
    }

    [Theory]
    [InlineData("Dog")]
    [InlineData("Cat")]
    [InlineData("dog")]
    [InlineData("CAT")]
    public void Validate_WithValidSpeciesCaseInsensitive_ShouldNotHaveError(string species)
    {
        var dto = new AnimalFilterDto { Species = species };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.Species);
    }

    [Fact]
    public void Validate_WithInvalidSpecies_ShouldHaveError()
    {
        var dto = new AnimalFilterDto { Species = "InvalidSpecies" };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Species)
            .WithErrorMessage("Species must be one of: Dog, Cat, Bird, Rabbit, Other");
    }

    [Fact]
    public void Validate_WithNullSpecies_ShouldNotHaveError()
    {
        var dto = new AnimalFilterDto { Species = null };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.Species);
    }

    [Fact]
    public void Validate_WithEmptySpecies_ShouldNotHaveError()
    {
        var dto = new AnimalFilterDto { Species = "" };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.Species);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(50)]
    public void Validate_WithValidAge_ShouldNotHaveError(int age)
    {
        var dto = new AnimalFilterDto { Age = age };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.Age);
    }

    [Fact]
    public void Validate_WithNegativeAge_ShouldHaveError()
    {
        var dto = new AnimalFilterDto { Age = -1 };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Age)
            .WithErrorMessage("Age must be a non-negative number");
    }

    [Fact]
    public void Validate_WithAgeTooHigh_ShouldHaveError()
    {
        var dto = new AnimalFilterDto { Age = 51 };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Age)
            .WithErrorMessage("Age must be less than or equal to 50 years");
    }

    [Fact]
    public void Validate_WithNullAge_ShouldNotHaveError()
    {
        var dto = new AnimalFilterDto { Age = null };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.Age);
    }

    [Theory]
    [InlineData("Small")]
    [InlineData("Medium")]
    [InlineData("Large")]
    [InlineData("small")]
    [InlineData("MEDIUM")]
    public void Validate_WithValidSize_ShouldNotHaveError(string size)
    {
        var dto = new AnimalFilterDto { Size = size };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.Size);
    }

    [Fact]
    public void Validate_WithInvalidSize_ShouldHaveError()
    {
        var dto = new AnimalFilterDto { Size = "ExtraLarge" };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Size)
            .WithErrorMessage("Size must be one of: Small, Medium, Large");
    }

    [Theory]
    [InlineData("Male")]
    [InlineData("Female")]
    [InlineData("male")]
    [InlineData("FEMALE")]
    public void Validate_WithValidSex_ShouldNotHaveError(string sex)
    {
        var dto = new AnimalFilterDto { Sex = sex };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.Sex);
    }

    [Fact]
    public void Validate_WithInvalidSex_ShouldHaveError()
    {
        var dto = new AnimalFilterDto { Sex = "Unknown" };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Sex)
            .WithErrorMessage("Sex must be one of: Male, Female");
    }

    [Fact]
    public void Validate_WithValidName_ShouldNotHaveError()
    {
        var dto = new AnimalFilterDto { Name = "Buddy" };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_WithNameTooLong_ShouldHaveError()
    {
        var dto = new AnimalFilterDto { Name = new string('a', 101) };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Name filter must not exceed 100 characters");
    }

    [Fact]
    public void Validate_WithNameExactly100Characters_ShouldNotHaveError()
    {
        var dto = new AnimalFilterDto { Name = new string('a', 100) };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_WithValidShelterName_ShouldNotHaveError()
    {
        var dto = new AnimalFilterDto { ShelterName = "Happy Paws Shelter" };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.ShelterName);
    }

    [Fact]
    public void Validate_WithShelterNameTooLong_ShouldHaveError()
    {
        var dto = new AnimalFilterDto { ShelterName = new string('a', 201) };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.ShelterName)
            .WithErrorMessage("Shelter name filter must not exceed 200 characters");
    }

    [Fact]
    public void Validate_WithShelterNameExactly200Characters_ShouldNotHaveError()
    {
        var dto = new AnimalFilterDto { ShelterName = new string('a', 200) };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.ShelterName);
    }

    [Fact]
    public void Validate_WithValidBreed_ShouldNotHaveError()
    {
        var dto = new AnimalFilterDto { Breed = "Labrador" };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.Breed);
    }

    [Fact]
    public void Validate_WithBreedTooLong_ShouldHaveError()
    {
        var dto = new AnimalFilterDto { Breed = new string('a', 101) };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Breed)
            .WithErrorMessage("Breed filter must not exceed 100 characters");
    }

    [Fact]
    public void Validate_WithBreedExactly100Characters_ShouldNotHaveError()
    {
        var dto = new AnimalFilterDto { Breed = new string('a', 100) };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.Breed);
    }

    [Fact]
    public void Validate_WithAllValidFilters_ShouldNotHaveAnyErrors()
    {
        var dto = new AnimalFilterDto
        {
            Species = "Dog",
            Age = 5,
            Size = "Medium",
            Sex = "Male",
            Name = "Buddy",
            ShelterName = "Happy Paws",
            Breed = "Labrador"
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyDto_ShouldNotHaveAnyErrors()
    {
        var dto = new AnimalFilterDto();

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithMultipleInvalidFields_ShouldHaveMultipleErrors()
    {
        var dto = new AnimalFilterDto
        {
            Species = "InvalidSpecies",
            Age = -5,
            Size = "InvalidSize",
            Sex = "InvalidSex"
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Species);
        result.ShouldHaveValidationErrorFor(x => x.Age);
        result.ShouldHaveValidationErrorFor(x => x.Size);
        result.ShouldHaveValidationErrorFor(x => x.Sex);
    }
}