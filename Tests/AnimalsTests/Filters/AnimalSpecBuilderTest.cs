using Application.Animals.Filters;
using Application.Animals.Filters.Specs;
using Domain.Services;

namespace Tests.AnimalsTests.Filters;

/// <summary>
/// Tests for AnimalSpecBuilder using Equivalence Class Partitioning and Boundary Value Analysis.
/// Tests enum parsing boundaries, string handling, null cases, and specification creation logic.
/// </summary>
public class AnimalSpecBuilderTests
{
    private readonly AnimalSpecBuilder _builder;
    private readonly AnimalDomainService _domainService;

    public AnimalSpecBuilderTests()
    {
        _domainService = new AnimalDomainService();
        _builder = new AnimalSpecBuilder(_domainService);
    }

    [Fact]
    public void Build_EmptyFilter_ReturnsEmptyList()
    {
        var filter = new AnimalFilterModel();

        var specs = _builder.Build(filter);

        Assert.Empty(specs);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void Build_ValidAge_CreatesAgeSpec(int age)
    {
        var filter = new AnimalFilterModel { Age = age };

        var specs = _builder.Build(filter);

        Assert.Single(specs);
        Assert.IsType<AgeSpec>(specs[0]);
    }

    [Fact]
    public void Build_NullAge_DoesNotCreateAgeSpec()
    {
        var filter = new AnimalFilterModel { Age = null };

        var specs = _builder.Build(filter);

        Assert.Empty(specs);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(51)]
    public void Build_AgeOutsideBounds_StillCreatesSpec(int age)
    {
        var filter = new AnimalFilterModel { Age = age };

        var specs = _builder.Build(filter);

        Assert.Single(specs);
    }

    [Theory]
    [InlineData("Lab")]
    [InlineData("L")]
    public void Build_ValidBreed_CreatesBreedSpec(string breed)
    {
        var filter = new AnimalFilterModel { Breed = breed };

        var specs = _builder.Build(filter);

        Assert.Single(specs);
        Assert.IsType<BreedSpec>(specs[0]);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Build_EmptyOrNullBreed_DoesNotCreateSpec(string? breed)
    {
        var filter = new AnimalFilterModel { Breed = breed };

        var specs = _builder.Build(filter);

        Assert.Empty(specs);
    }

    [Fact]
    public void Build_BreedAt100CharBoundary_CreatesSpec()
    {
        var filter = new AnimalFilterModel { Breed = new string('a', 100) };

        var specs = _builder.Build(filter);

        Assert.Single(specs);
    }

    [Fact]
    public void Build_BreedAbove100CharBoundary_CreatesSpec()
    {
        var filter = new AnimalFilterModel { Breed = new string('a', 101) };

        var specs = _builder.Build(filter);

        Assert.Single(specs);
    }

    [Theory]
    [InlineData("Male")]
    [InlineData("Female")]
    [InlineData("male")]
    [InlineData("FEMALE")]
    public void Build_ValidSex_CreatesSexSpec(string sex)
    {
        var filter = new AnimalFilterModel { Sex = sex };

        var specs = _builder.Build(filter);

        Assert.Single(specs);
        Assert.IsType<SexSpec>(specs[0]);
    }

    [Theory]
    [InlineData("Unknown")]
    [InlineData("F")]
    [InlineData("123")]
    public void Build_InvalidSex_DoesNotCreateSpec(string sex)
    {
        var filter = new AnimalFilterModel { Sex = sex };

        var specs = _builder.Build(filter);

        Assert.Empty(specs);
    }

    [Theory]
    [InlineData("Large")]
    [InlineData("small")]
    [InlineData("LARGE")]
    public void Build_ValidSize_CreatesSizeSpec(string size)
    {
        var filter = new AnimalFilterModel { Size = size };

        var specs = _builder.Build(filter);

        Assert.Single(specs);
        Assert.IsType<SizeSpec>(specs[0]);
    }

    [Theory]
    [InlineData("XL")]
    [InlineData("")]
    public void Build_InvalidSize_DoesNotCreateSpec(string size)
    {
        var filter = new AnimalFilterModel { Size = size };

        var specs = _builder.Build(filter);

        Assert.Empty(specs);
    }

    [Theory]
    [InlineData("Dog")]
    [InlineData("dog")]
    [InlineData("CAT")]
    public void Build_ValidSpecies_CreatesSpeciesSpec(string species)
    {
        var filter = new AnimalFilterModel { Species = species };

        var specs = _builder.Build(filter);

        Assert.Single(specs);
        Assert.IsType<SpeciesSpec>(specs[0]);
    }

    [Theory]
    [InlineData("Bird")]
    [InlineData("InvalidSpecies")]
    public void Build_InvalidSpecies_DoesNotCreateSpec(string species)
    {
        var filter = new AnimalFilterModel { Species = species };

        var specs = _builder.Build(filter);

        Assert.Empty(specs);
    }

    [Theory]
    [InlineData("B")]
    [InlineData("A very long name that exceeds normal boundaries")]
    public void Build_ValidName_CreatesNameSpec(string name)
    {
        var filter = new AnimalFilterModel { Name = name };

        var specs = _builder.Build(filter);

        Assert.Single(specs);
        Assert.IsType<NameSpec>(specs[0]);
    }

    [Fact]
    public void Build_NameAt100CharBoundary_CreatesSpec()
    {
        var filter = new AnimalFilterModel { Name = new string('x', 100) };

        var specs = _builder.Build(filter);

        Assert.Single(specs);
    }

    [Fact]
    public void Build_NameAbove100CharBoundary_CreatesSpec()
    {
        var filter = new AnimalFilterModel { Name = new string('x', 101) };

        var specs = _builder.Build(filter);

        Assert.Single(specs);
    }

    [Theory]
    [InlineData("S")]
    [InlineData("A very long shelter name that goes beyond normal limits")]
    public void Build_ValidShelterName_CreatesShelterNameSpec(string shelterName)
    {
        var filter = new AnimalFilterModel { ShelterName = shelterName };

        var specs = _builder.Build(filter);

        Assert.Single(specs);
        Assert.IsType<ShelterNameSpec>(specs[0]);
    }

    [Fact]
    public void Build_ShelterNameAt200CharBoundary_CreatesSpec()
    {
        var filter = new AnimalFilterModel { ShelterName = new string('s', 200) };

        var specs = _builder.Build(filter);

        Assert.Single(specs);
    }

    [Fact]
    public void Build_ShelterNameAbove200CharBoundary_CreatesSpec()
    {
        var filter = new AnimalFilterModel { ShelterName = new string('s', 201) };

        var specs = _builder.Build(filter);

        Assert.Single(specs);
    }

    [Fact]
    public void Build_AllValidFilters_CreatesAllSpecs()
    {
        var filter = new AnimalFilterModel
        {
            Age = 5,
            Breed = "Labrador",
            Name = "Buddy",
            Sex = "Male",
            ShelterName = "Happy Paws",
            Size = "Medium",
            Species = "Dog"
        };

        var specs = _builder.Build(filter);

        Assert.Equal(7, specs.Count);
        Assert.Contains(specs, s => s is AgeSpec);
        Assert.Contains(specs, s => s is BreedSpec);
        Assert.Contains(specs, s => s is NameSpec);
        Assert.Contains(specs, s => s is SexSpec);
        Assert.Contains(specs, s => s is ShelterNameSpec);
        Assert.Contains(specs, s => s is SizeSpec);
        Assert.Contains(specs, s => s is SpeciesSpec);
    }

    [Fact]
    public void Build_MixOfValidAndInvalidFilters_CreatesOnlyValidSpecs()
    {
        var filter = new AnimalFilterModel
        {
            Age = 5,
            Sex = "InvalidSex",
            Size = "Medium",
            Species = "Bird"
        };

        var specs = _builder.Build(filter);

        Assert.Equal(2, specs.Count);
        Assert.Contains(specs, s => s is AgeSpec);
        Assert.Contains(specs, s => s is SizeSpec);
    }

    [Fact]
    public void Build_AllInvalidEnums_CreatesNoEnumSpecs()
    {
        var filter = new AnimalFilterModel
        {
            Sex = "Unknown",
            Size = "ExtraLarge",
            Species = "Bird"
        };

        var specs = _builder.Build(filter);

        Assert.Empty(specs);
    }

    [Fact]
    public void Build_OnlyStringFilters_CreatesCorrectSpecs()
    {
        var filter = new AnimalFilterModel
        {
            Breed = "Labrador",
            Name = "Buddy",
            ShelterName = "Happy"
        };

        var specs = _builder.Build(filter);

        Assert.Equal(3, specs.Count);
    }

    [Fact]
    public void Build_CaseSensitivityInEnums_HandlesCorrectly()
    {
        var filter1 = new AnimalFilterModel { Sex = "MALE" };
        var filter2 = new AnimalFilterModel { Sex = "male" };
        var filter3 = new AnimalFilterModel { Sex = "MaLe" };

        var specs1 = _builder.Build(filter1);
        var specs2 = _builder.Build(filter2);
        var specs3 = _builder.Build(filter3);

        Assert.Single(specs1);
        Assert.Single(specs2);
        Assert.Single(specs3);
    }
}