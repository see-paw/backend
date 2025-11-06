using Application.Animals.Filters.Specs;
using Domain;
using Domain.Enums;
using Domain.Services;

namespace Tests.Animals.Filters;

/// <summary>
/// Comprehensive tests for all Specification classes using Equivalence Class Partitioning and Boundary Value Analysis.
/// Tests both IsSatisfied and ToExpression methods for consistency and correctness at boundaries.
/// </summary>
public class AllSpecificationsTests
{
    private readonly AnimalDomainService _domainService;

    public AllSpecificationsTests()
    {
        _domainService = new AnimalDomainService();
    }

    private Animal CreateAnimal()
    {
        return new Animal
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Buddy",
            Species = Species.Dog,
            Size = SizeType.Medium,
            Sex = SexType.Male,
            Colour = "Brown",
            BirthDate = new DateOnly(2020, 1, 1),
            Sterilized = true,
            BreedId = "breed-id",
            Cost = 100m,
            ShelterId = "shelter-id",
            Breed = new Breed { Id = "breed-id", Name = "Labrador" },
            Shelter = new Shelter
            {
                Id = "shelter-id",
                Name = "Happy Shelter",
                Street = "Main St",
                City = "Porto",
                PostalCode = "4000-000",
                Phone = "912345678",
                NIF = "123456789",
                OpeningTime = new TimeOnly(9, 0),
                ClosingTime = new TimeOnly(18, 0)
            }
        };
    }

    #region AgeSpec Tests

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void AgeSpec_IsSatisfied_MatchingAge_ReturnsTrue(int age)
    {
        var animal = CreateAnimal();
        animal.BirthDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-age));
        var spec = new AgeSpec(_domainService) { Age = age };

        var result = spec.IsSatisfied(animal);

        Assert.True(result);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(5, 6)]
    public void AgeSpec_IsSatisfied_NonMatchingAge_ReturnsFalse(int actualAge, int filterAge)
    {
        var animal = CreateAnimal();
        animal.BirthDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-actualAge));
        var spec = new AgeSpec(_domainService) { Age = filterAge };

        var result = spec.IsSatisfied(animal);

        Assert.False(result);
    }

    [Fact]
    public void AgeSpec_ToExpression_CanCompile()
    {
        var spec = new AgeSpec(_domainService) { Age = 5 };

        var expression = spec.ToExpression();
        var compiled = expression.Compile();

        Assert.NotNull(compiled);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    public void AgeSpec_ToExpression_MatchesIsSatisfied(int age)
    {
        var animal = CreateAnimal();
        animal.BirthDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-age));
        var spec = new AgeSpec(_domainService) { Age = age };

        var isSatisfiedResult = spec.IsSatisfied(animal);
        var expressionResult = spec.ToExpression().Compile()(animal);

        Assert.Equal(isSatisfiedResult, expressionResult);
    }

    #endregion

    #region BreedSpec Tests

    [Theory]
    [InlineData("Labrador", "Labrador")]
    [InlineData("Labrador", "Lab")]
    [InlineData("Labrador", "labrador")]
    [InlineData("Labrador", "LABRADOR")]
    [InlineData("Golden Retriever", "Golden")]
    public void BreedSpec_IsSatisfied_MatchingBreed_ReturnsTrue(string actualBreed, string filterBreed)
    {
        var animal = CreateAnimal();
        animal.Breed.Name = actualBreed;
        var spec = new BreedSpec { BreedName = filterBreed };

        var result = spec.IsSatisfied(animal);

        Assert.True(result);
    }

    [Theory]
    [InlineData("Labrador", "Golden")]
    [InlineData("Labrador", "xyz")]
    public void BreedSpec_IsSatisfied_NonMatchingBreed_ReturnsFalse(string actualBreed, string filterBreed)
    {
        var animal = CreateAnimal();
        animal.Breed.Name = actualBreed;
        var spec = new BreedSpec { BreedName = filterBreed };

        var result = spec.IsSatisfied(animal);

        Assert.False(result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void BreedSpec_IsSatisfied_EmptyFilter_ReturnsTrue(string? filterBreed)
    {
        var animal = CreateAnimal();
        var spec = new BreedSpec { BreedName = filterBreed! };

        var result = spec.IsSatisfied(animal);

        Assert.True(result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void BreedSpec_ToExpression_EmptyFilter_ReturnsNull(string? filterBreed)
    {
        var spec = new BreedSpec { BreedName = filterBreed! };

        var expression = spec.ToExpression();

        Assert.Null(expression);
    }

    [Fact]
    public void BreedSpec_BreedNameAt100CharBoundary_Works()
    {
        var animal = CreateAnimal();
        animal.Breed.Name = new string('a', 100);
        var spec = new BreedSpec { BreedName = new string('a', 50) };

        var result = spec.IsSatisfied(animal);

        Assert.True(result);
    }

    [Theory]
    [InlineData("Labrador", "Lab")]
    public void BreedSpec_ToExpression_MatchesIsSatisfied(string actualBreed, string filterBreed)
    {
        var animal = CreateAnimal();
        animal.Breed.Name = actualBreed;
        var spec = new BreedSpec { BreedName = filterBreed };

        var isSatisfiedResult = spec.IsSatisfied(animal);
        var expression = spec.ToExpression();
        var expressionResult = expression != null && expression.Compile()(animal);

        Assert.Equal(isSatisfiedResult, expressionResult);
    }

    #endregion

    #region NameSpec Tests

    [Theory]
    [InlineData("Buddy", "Buddy")]
    [InlineData("Max The Great", "Max")]
    public void NameSpec_IsSatisfied_MatchingName_ReturnsTrue(string actualName, string filterName)
    {
        var animal = CreateAnimal();
        animal.Name = actualName;
        var spec = new NameSpec { Name = filterName };

        var result = spec.IsSatisfied(animal);

        Assert.True(result);
    }

    [Theory]
    [InlineData("Buddy", "Max")]
    [InlineData("Buddy", "xyz")]
    public void NameSpec_IsSatisfied_NonMatchingName_ReturnsFalse(string actualName, string filterName)
    {
        var animal = CreateAnimal();
        animal.Name = actualName;
        var spec = new NameSpec { Name = filterName };

        var result = spec.IsSatisfied(animal);

        Assert.False(result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void NameSpec_IsSatisfied_EmptyFilter_ReturnsTrue(string? filterName)
    {
        var animal = CreateAnimal();
        var spec = new NameSpec { Name = filterName! };

        var result = spec.IsSatisfied(animal);

        Assert.True(result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void NameSpec_ToExpression_EmptyFilter_ReturnsNull(string? filterName)
    {
        var spec = new NameSpec { Name = filterName! };

        var expression = spec.ToExpression();

        Assert.Null(expression);
    }

    [Fact]
    public void NameSpec_NameAt100CharBoundary_Works()
    {
        var animal = CreateAnimal();
        animal.Name = new string('a', 100);
        var spec = new NameSpec { Name = new string('a', 50) };

        var result = spec.IsSatisfied(animal);

        Assert.True(result);
    }

    #endregion

    #region SexSpec Tests

    [Theory]
    [InlineData(SexType.Male)]
    [InlineData(SexType.Female)]
    public void SexSpec_IsSatisfied_MatchingSex_ReturnsTrue(SexType sex)
    {
        var animal = CreateAnimal();
        animal.Sex = sex;
        var spec = new SexSpec { Sex = sex };

        var result = spec.IsSatisfied(animal);

        Assert.True(result);
    }

    [Fact]
    public void SexSpec_IsSatisfied_NonMatchingSex_ReturnsFalse()
    {
        var animal = CreateAnimal();
        animal.Sex = SexType.Male;
        var spec = new SexSpec { Sex = SexType.Female };

        var result = spec.IsSatisfied(animal);

        Assert.False(result);
    }

    [Theory]
    [InlineData(SexType.Male)]
    [InlineData(SexType.Female)]
    public void SexSpec_ToExpression_MatchesIsSatisfied(SexType sex)
    {
        var animal = CreateAnimal();
        animal.Sex = sex;
        var spec = new SexSpec { Sex = sex };

        var isSatisfiedResult = spec.IsSatisfied(animal);
        var expressionResult = spec.ToExpression()!.Compile()(animal);

        Assert.Equal(isSatisfiedResult, expressionResult);
    }

    #endregion

    #region ShelterNameSpec Tests

    [Theory]
    [InlineData("Happy Shelter", "Happy")]
    [InlineData("Happy Shelter", "HAPPY SHELTER")]
    public void ShelterNameSpec_IsSatisfied_MatchingShelter_ReturnsTrue(string actualName, string filterName)
    {
        var animal = CreateAnimal();
        animal.Shelter.Name = actualName;
        var spec = new ShelterNameSpec { ShelterName = filterName };

        var result = spec.IsSatisfied(animal);

        Assert.True(result);
    }

    [Theory]
    [InlineData("Happy Shelter", "Sad")]
    [InlineData("Happy Shelter", "Different")]
    public void ShelterNameSpec_IsSatisfied_NonMatchingShelter_ReturnsFalse(string actualName, string filterName)
    {
        var animal = CreateAnimal();
        animal.Shelter.Name = actualName;
        var spec = new ShelterNameSpec { ShelterName = filterName };

        var result = spec.IsSatisfied(animal);

        Assert.False(result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ShelterNameSpec_IsSatisfied_EmptyFilter_ReturnsTrue(string? filterName)
    {
        var animal = CreateAnimal();
        var spec = new ShelterNameSpec { ShelterName = filterName! };

        var result = spec.IsSatisfied(animal);

        Assert.True(result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ShelterNameSpec_ToExpression_EmptyFilter_ReturnsNull(string? filterName)
    {
        var spec = new ShelterNameSpec { ShelterName = filterName! };

        var expression = spec.ToExpression();

        Assert.Null(expression);
    }

    [Fact]
    public void ShelterNameSpec_NameAt200CharBoundary_Works()
    {
        var animal = CreateAnimal();
        animal.Shelter.Name = new string('s', 200);
        var spec = new ShelterNameSpec { ShelterName = new string('s', 100) };

        var result = spec.IsSatisfied(animal);

        Assert.True(result);
    }

    #endregion

    #region SizeSpec Tests

    [Theory]
    [InlineData(SizeType.Small)]
    public void SizeSpec_IsSatisfied_MatchingSize_ReturnsTrue(SizeType size)
    {
        var animal = CreateAnimal();
        animal.Size = size;
        var spec = new SizeSpec { Size = size };

        var result = spec.IsSatisfied(animal);

        Assert.True(result);
    }

    [Theory]
    [InlineData(SizeType.Small, SizeType.Large)]
    public void SizeSpec_IsSatisfied_NonMatchingSize_ReturnsFalse(SizeType actualSize, SizeType filterSize)
    {
        var animal = CreateAnimal();
        animal.Size = actualSize;
        var spec = new SizeSpec { Size = filterSize };

        var result = spec.IsSatisfied(animal);

        Assert.False(result);
    }

    [Theory]
    [InlineData(SizeType.Small)]
    public void SizeSpec_ToExpression_MatchesIsSatisfied(SizeType size)
    {
        var animal = CreateAnimal();
        animal.Size = size;
        var spec = new SizeSpec { Size = size };

        var isSatisfiedResult = spec.IsSatisfied(animal);
        var expressionResult = spec.ToExpression()!.Compile()(animal);

        Assert.Equal(isSatisfiedResult, expressionResult);
    }

    #endregion

    #region SpeciesSpec Tests

    [Theory]
    [InlineData(Species.Dog)]
    public void SpeciesSpec_IsSatisfied_MatchingSpecies_ReturnsTrue(Species species)
    {
        var animal = CreateAnimal();
        animal.Species = species;
        var spec = new SpeciesSpec { Species = species };

        var result = spec.IsSatisfied(animal);

        Assert.True(result);
    }

    [Fact]
    public void SpeciesSpec_IsSatisfied_NonMatchingSpecies_ReturnsFalse()
    {
        var animal = CreateAnimal();
        animal.Species = Species.Dog;
        var spec = new SpeciesSpec { Species = Species.Cat };

        var result = spec.IsSatisfied(animal);

        Assert.False(result);
    }

    [Theory]
    [InlineData(Species.Dog)]
    public void SpeciesSpec_ToExpression_MatchesIsSatisfied(Species species)
    {
        var animal = CreateAnimal();
        animal.Species = species;
        var spec = new SpeciesSpec { Species = species };

        var isSatisfiedResult = spec.IsSatisfied(animal);
        var expressionResult = spec.ToExpression()!.Compile()(animal);

        Assert.Equal(isSatisfiedResult, expressionResult);
    }

    #endregion

    #region Cross-Spec Consistency Tests

    [Fact]
    public void AllSpecs_ToExpressionAndIsSatisfied_ProduceSameResults()
    {
        var animal = CreateAnimal();
        animal.Name = "Buddy";
        animal.Breed.Name = "Labrador";
        animal.Size = SizeType.Medium;
        animal.Sex = SexType.Male;
        animal.Species = Species.Dog;
        animal.Shelter.Name = "Happy Shelter";
        animal.BirthDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-5));

        var nameSpec = new NameSpec { Name = "Bud" };
        var breedSpec = new BreedSpec { BreedName = "Lab" };
        var sizeSpec = new SizeSpec { Size = SizeType.Medium };
        var sexSpec = new SexSpec { Sex = SexType.Male };
        var speciesSpec = new SpeciesSpec { Species = Species.Dog };
        var shelterSpec = new ShelterNameSpec { ShelterName = "Happy" };
        var ageSpec = new AgeSpec(_domainService) { Age = 5 };

        Assert.Equal(nameSpec.IsSatisfied(animal), nameSpec.ToExpression()!.Compile()(animal));
        Assert.Equal(breedSpec.IsSatisfied(animal), breedSpec.ToExpression()!.Compile()(animal));
        Assert.Equal(sizeSpec.IsSatisfied(animal), sizeSpec.ToExpression()!.Compile()(animal));
        Assert.Equal(sexSpec.IsSatisfied(animal), sexSpec.ToExpression()!.Compile()(animal));
        Assert.Equal(speciesSpec.IsSatisfied(animal), speciesSpec.ToExpression()!.Compile()(animal));
        Assert.Equal(shelterSpec.IsSatisfied(animal), shelterSpec.ToExpression()!.Compile()(animal));
        Assert.Equal(ageSpec.IsSatisfied(animal), ageSpec.ToExpression()!.Compile()(animal));
    }

    #endregion
}