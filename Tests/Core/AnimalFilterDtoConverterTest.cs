using Application.Animals.Filters;
using WebAPI.Core;
using WebAPI.DTOs.Animals;

namespace Tests.Core;

/// <summary>
/// Tests for AnimalFilterDtoConverter using Equivalence Class Partitioning and Boundary Value Analysis.
/// Tests string trimming, null handling, boundary values for string lengths, and mapping correctness.
/// </summary>
public class AnimalFilterDtoConverterTests
{
    private readonly AnimalFilterDtoConverter _converter;

    public AnimalFilterDtoConverterTests()
    {
        _converter = new AnimalFilterDtoConverter();
    }

    [Fact]
    public void Convert_EmptyDto_ReturnsEmptyModel()
    {
        var dto = new AnimalFilterDto();

        var model = _converter.Convert(dto, new AnimalFilterModel(), null!);

        Assert.Null(model.Age);
        Assert.Null(model.Breed);
        Assert.Null(model.Name);
        Assert.Null(model.Sex);
        Assert.Null(model.ShelterName);
        Assert.Null(model.Size);
        Assert.Null(model.Species);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(-1)]
    [InlineData(50)]
    [InlineData(51)]
    [InlineData(int.MaxValue)]
    [InlineData(int.MinValue)]
    public void Convert_AnyAgeValue_MapsDirectly(int age)
    {
        var dto = new AnimalFilterDto { Age = age };

        var model = _converter.Convert(dto, new AnimalFilterModel(), null!);

        Assert.Equal(age, model.Age);
    }

    [Fact]
    public void Convert_NullAge_MapsAsNull()
    {
        var dto = new AnimalFilterDto { Age = null };

        var model = _converter.Convert(dto, new AnimalFilterModel(), null!);

        Assert.Null(model.Age);
    }

    [Theory]
    [InlineData("  Labrador  ", "Labrador")]
    [InlineData("Labrador", "Labrador")]
    [InlineData("  Lab  ", "Lab")]
    [InlineData("\tLabrador\t", "Labrador")]
    [InlineData("\nLabrador\n", "Labrador")]
    public void Convert_BreedWithWhitespace_TrimsCorrectly(string input, string expected)
    {
        var dto = new AnimalFilterDto { Breed = input };

        var model = _converter.Convert(dto, new AnimalFilterModel(), null!);

        Assert.Equal(expected, model.Breed);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Convert_NullOrEmptyBreed_MapsAsIs(string? breed)
    {
        var dto = new AnimalFilterDto { Breed = breed };

        var model = _converter.Convert(dto, new AnimalFilterModel(), null!);

        Assert.Equal(breed?.Trim(), model.Breed);
    }

    [Fact]
    public void Convert_BreedAt100CharBoundary_Preserves()
    {
        var breed = new string('a', 100);
        var dto = new AnimalFilterDto { Breed = breed };

        var model = _converter.Convert(dto, new AnimalFilterModel(), null!);

        Assert.Equal(100, model.Breed?.Length);
    }

    [Fact]
    public void Convert_BreedAbove100CharBoundary_Preserves()
    {
        var breed = new string('a', 101);
        var dto = new AnimalFilterDto { Breed = breed };

        var model = _converter.Convert(dto, new AnimalFilterModel(), null!);

        Assert.Equal(101, model.Breed?.Length);
    }

    [Theory]
    [InlineData("  Buddy  ", "Buddy")]
    [InlineData("Max", "Max")]
    [InlineData("  M  ", "M")]
    public void Convert_NameWithWhitespace_TrimsCorrectly(string input, string expected)
    {
        var dto = new AnimalFilterDto { Name = input };

        var model = _converter.Convert(dto, new AnimalFilterModel(), null!);

        Assert.Equal(expected, model.Name);
    }

    [Fact]
    public void Convert_NameAt100CharBoundary_Preserves()
    {
        var name = new string('x', 100);
        var dto = new AnimalFilterDto { Name = name };

        var model = _converter.Convert(dto, new AnimalFilterModel(), null!);

        Assert.Equal(100, model.Name?.Length);
    }

    [Fact]
    public void Convert_NameAbove100CharBoundary_Preserves()
    {
        var name = new string('x', 101);
        var dto = new AnimalFilterDto { Name = name };

        var model = _converter.Convert(dto, new AnimalFilterModel(), null!);

        Assert.Equal(101, model.Name?.Length);
    }

    [Theory]
    [InlineData("  Male  ", "Male")]
    [InlineData("Female", "Female")]
    [InlineData("  InvalidSex  ", "InvalidSex")]
    public void Convert_SexWithWhitespace_TrimsCorrectly(string input, string expected)
    {
        var dto = new AnimalFilterDto { Sex = input };

        var model = _converter.Convert(dto, new AnimalFilterModel(), null!);

        Assert.Equal(expected, model.Sex);
    }

    [Theory]
    [InlineData("  Happy Shelter  ", "Happy Shelter")]
    [InlineData("Shelter", "Shelter")]
    [InlineData("  S  ", "S")]
    public void Convert_ShelterNameWithWhitespace_TrimsCorrectly(string input, string expected)
    {
        var dto = new AnimalFilterDto { ShelterName = input };

        var model = _converter.Convert(dto, new AnimalFilterModel(), null!);

        Assert.Equal(expected, model.ShelterName);
    }

    [Fact]
    public void Convert_ShelterNameAt200CharBoundary_Preserves()
    {
        var shelterName = new string('s', 200);
        var dto = new AnimalFilterDto { ShelterName = shelterName };

        var model = _converter.Convert(dto, new AnimalFilterModel(), null!);

        Assert.Equal(200, model.ShelterName?.Length);
    }

    [Fact]
    public void Convert_ShelterNameAbove200CharBoundary_Preserves()
    {
        var shelterName = new string('s', 201);
        var dto = new AnimalFilterDto { ShelterName = shelterName };

        var model = _converter.Convert(dto, new AnimalFilterModel(), null!);

        Assert.Equal(201, model.ShelterName?.Length);
    }

    [Theory]
    [InlineData("  Small  ", "Small")]
    [InlineData("Medium", "Medium")]
    [InlineData("  Large  ", "Large")]
    public void Convert_SizeWithWhitespace_TrimsCorrectly(string input, string expected)
    {
        var dto = new AnimalFilterDto { Size = input };

        var model = _converter.Convert(dto, new AnimalFilterModel(), null!);

        Assert.Equal(expected, model.Size);
    }

    [Theory]
    [InlineData("  Dog  ", "Dog")]
    [InlineData("Cat", "Cat")]
    [InlineData("  Bird  ", "Bird")]
    public void Convert_SpeciesWithWhitespace_TrimsCorrectly(string input, string expected)
    {
        var dto = new AnimalFilterDto { Species = input };

        var model = _converter.Convert(dto, new AnimalFilterModel(), null!);

        Assert.Equal(expected, model.Species);
    }

    [Fact]
    public void Convert_AllFieldsPopulated_MapsAll()
    {
        var dto = new AnimalFilterDto
        {
            Age = 5,
            Breed = "  Labrador  ",
            Name = "  Buddy  ",
            Sex = "  Male  ",
            ShelterName = "  Happy Paws  ",
            Size = "  Medium  ",
            Species = "  Dog  "
        };

        var model = _converter.Convert(dto, new AnimalFilterModel(), null!);

        Assert.Equal(5, model.Age);
        Assert.Equal("Labrador", model.Breed);
        Assert.Equal("Buddy", model.Name);
        Assert.Equal("Male", model.Sex);
        Assert.Equal("Happy Paws", model.ShelterName);
        Assert.Equal("Medium", model.Size);
        Assert.Equal("Dog", model.Species);
    }

    [Fact]
    public void Convert_OnlyWhitespaceStrings_TrimsToEmpty()
    {
        var dto = new AnimalFilterDto
        {
            Breed = "   ",
            Name = "\t\t",
            Sex = "\n",
            ShelterName = "  ",
            Size = "   ",
            Species = "\t"
        };

        var model = _converter.Convert(dto, new AnimalFilterModel(), null!);

        Assert.Empty(model.Breed ?? "");
        Assert.Empty(model.Name ?? "");
        Assert.Empty(model.Sex ?? "");
        Assert.Empty(model.ShelterName ?? "");
        Assert.Empty(model.Size ?? "");
        Assert.Empty(model.Species ?? "");
    }

    [Fact]
    public void Convert_MixedValidAndNullFields_HandlesCorrectly()
    {
        var dto = new AnimalFilterDto
        {
            Age = 5,
            Breed = null,
            Name = "Buddy",
            Sex = null,
            ShelterName = "Shelter",
            Size = null,
            Species = "Dog"
        };

        var model = _converter.Convert(dto, new AnimalFilterModel(), null!);

        Assert.Equal(5, model.Age);
        Assert.Null(model.Breed);
        Assert.Equal("Buddy", model.Name);
        Assert.Null(model.Sex);
        Assert.Equal("Shelter", model.ShelterName);
        Assert.Null(model.Size);
        Assert.Equal("Dog", model.Species);
    }

    [Fact]
    public void Convert_SpecialCharactersInStrings_Preserves()
    {
        var dto = new AnimalFilterDto
        {
            Breed = "Labrador-Retriever",
            Name = "Buddy's Dog",
            ShelterName = "Happy & Safe Shelter"
        };

        var model = _converter.Convert(dto, new AnimalFilterModel(), null!);

        Assert.Equal("Labrador-Retriever", model.Breed);
        Assert.Equal("Buddy's Dog", model.Name);
        Assert.Equal("Happy & Safe Shelter", model.ShelterName);
    }

    [Fact]
    public void Convert_UnicodeCharacters_PreservesCorrectly()
    {
        var dto = new AnimalFilterDto
        {
            Name = "Ñoño",
            Breed = "São Bernardo",
            ShelterName = "Cão e Gato"
        };

        var model = _converter.Convert(dto, new AnimalFilterModel(), null!);

        Assert.Equal("Ñoño", model.Name);
        Assert.Equal("São Bernardo", model.Breed);
        Assert.Equal("Cão e Gato", model.ShelterName);
    }
}