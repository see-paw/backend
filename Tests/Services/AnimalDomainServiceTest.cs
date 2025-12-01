using Domain;
using Domain.Enums;
using Domain.Services;

namespace Tests.Services;

/// <summary>
/// Tests for AnimalDomainService using Equivalence Class Partitioning and Boundary Value Analysis.
/// Tests age calculation boundaries including same day, day before, day after, month boundaries, and year boundaries.
/// </summary>
public class AnimalDomainServiceTests
{
    private readonly AnimalDomainService _service;

    public AnimalDomainServiceTests()
    {
        _service = new AnimalDomainService();
    }

    private Animal CreateAnimal(DateOnly birthDate)
    {
        return new Animal
        {
            BirthDate = birthDate,
            Name = "Test",
            Species = Species.Dog,
            Size = SizeType.Medium,
            Sex = SexType.Male,
            Colour = "Brown",
            Sterilized = true,
            BreedId = "breed-id",
            Cost = 100m,
            ShelterId = "shelter-id"
        };
    }

    [Fact]
    public void GetAge_BornToday_ReturnsZero()
    {
        var animal = CreateAnimal(DateOnly.FromDateTime(DateTime.Now));

        var age = _service.GetAge(animal);

        Assert.Equal(0, age);
    }

    [Fact]
    public void GetAge_BornYesterday_ReturnsZero()
    {
        var animal = CreateAnimal(DateOnly.FromDateTime(DateTime.Now.AddDays(-1)));

        var age = _service.GetAge(animal);

        Assert.Equal(0, age);
    }

    [Fact]
    public void GetAge_BornExactlyOneYearAgo_ReturnsOne()
    {
        var animal = CreateAnimal(DateOnly.FromDateTime(DateTime.Now.AddYears(-1)));

        var age = _service.GetAge(animal);

        Assert.Equal(1, age);
    }

    [Fact]
    public void GetAge_BornOneYearAgoMinusOneDay_ReturnsZero()
    {
        var animal = CreateAnimal(DateOnly.FromDateTime(DateTime.Now.AddYears(-1).AddDays(1)));

        var age = _service.GetAge(animal);

        Assert.Equal(0, age);
    }

    [Fact]
    public void GetAge_BornOneYearAgoPlusOneDay_ReturnsOne()
    {
        var animal = CreateAnimal(DateOnly.FromDateTime(DateTime.Now.AddYears(-1).AddDays(-1)));

        var age = _service.GetAge(animal);

        Assert.Equal(1, age);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void GetAge_BornExactlyNYearsAgo_ReturnsN(int years)
    {
        var animal = CreateAnimal(DateOnly.FromDateTime(DateTime.Now.AddYears(-years)));

        var age = _service.GetAge(animal);

        Assert.Equal(years, age);
    }


    [Fact]
    public void GetAge_BirthdayAlreadyPassedThisYear_ReturnsCorrectAge()
    {
        var today = DateTime.Now;
        var birthDate = new DateOnly(today.Year - 5, today.Month - 1, today.Day);
        if (today.Month == 1)
        {
            birthDate = new DateOnly(today.Year - 5, 12, today.Day);
        }

        var animal = CreateAnimal(birthDate);

        var age = _service.GetAge(animal);

        Assert.Equal(5, age);
    }

    [Fact]
    public void GetAge_BornOnSameMonthButLaterDay_SubtractsOneYear()
    {
        var today = DateTime.Now;
        if (today.Day == DateTime.DaysInMonth(today.Year, today.Month))
        {
            return;
        }

        var birthDate = new DateOnly(today.Year - 5, today.Month, today.Day + 1);
        var animal = CreateAnimal(birthDate);

        var age = _service.GetAge(animal);

        Assert.Equal(4, age);
    }

    [Fact]
    public void GetAge_BornOnSameMonthButEarlierDay_DoesNotSubtractYear()
    {
        var today = DateTime.Now;
        if (today.Day == 1)
        {
            return;
        }

        var birthDate = new DateOnly(today.Year - 5, today.Month, today.Day - 1);
        var animal = CreateAnimal(birthDate);

        var age = _service.GetAge(animal);

        Assert.Equal(5, age);
    }

    [Fact]
    public void GetAge_BornOnSameMonthAndDay_ReturnsExactYears()
    {
        var today = DateTime.Now;
        var birthDate = new DateOnly(today.Year - 7, today.Month, today.Day);
        var animal = CreateAnimal(birthDate);

        var age = _service.GetAge(animal);

        Assert.Equal(7, age);
    }

    [Fact]
    public void GetAge_BornOnFirstDayOfYear_HandlesCorrectly()
    {
        var animal = CreateAnimal(new DateOnly(DateTime.Now.Year - 3, 1, 1));

        var age = _service.GetAge(animal);

        Assert.True(age == 3 || age == 2);
    }

    [Fact]
    public void GetAge_BornOnLastDayOfYear_HandlesCorrectly()
    {
        var animal = CreateAnimal(new DateOnly(DateTime.Now.Year - 3, 12, 31));

        var age = _service.GetAge(animal);

        Assert.True(age == 3 || age == 2);
    }

    [Fact]
    public void GetAge_BornOnLeapDay_HandlesCorrectly()
    {
        var animal = CreateAnimal(new DateOnly(2020, 2, 29));

        var age = _service.GetAge(animal);

        var expectedAge = DateTime.Now.Year - 2020;
        if (DateTime.Now.Month < 2 || (DateTime.Now.Month == 2 && DateTime.Now.Day < 29))
        {
            expectedAge--;
        }

        Assert.Equal(expectedAge, age);
    }

    [Fact]
    public void GetAge_VeryOldAnimal_CalculatesCorrectly()
    {
        var animal = CreateAnimal(new DateOnly(1990, 1, 1));

        var age = _service.GetAge(animal);

        Assert.True(age >= 34);
    }

    [Fact]
    public void GetAge_BornIn100YearsAgo_HandlesLargeAge()
    {
        var animal = CreateAnimal(new DateOnly(DateTime.Now.Year - 100, DateTime.Now.Month, DateTime.Now.Day));

        var age = _service.GetAge(animal);

        Assert.Equal(100, age);
    }

    [Fact]
    public void GetAge_BornOnEndOfMonth_HandlesCorrectly()
    {
        var animal = CreateAnimal(new DateOnly(2020, 1, 31));

        var age = _service.GetAge(animal);

        var expectedAge = DateTime.Now.Year - 2020;
        if (DateTime.Now.Month == 1 && DateTime.Now.Day < 31)
        {
            expectedAge--;
        }

        Assert.True(age >= expectedAge - 1 && age <= expectedAge);
    }

    [Fact]
    public void GetAge_BornInFuture_ReturnsNegativeAge()
    {
        var animal = CreateAnimal(DateOnly.FromDateTime(DateTime.Now.AddYears(1)));

        var age = _service.GetAge(animal);

        Assert.True(age <= 0);
    }

    [Theory]
    [InlineData(1, 15)]
    public void GetAge_BornOnSpecificMonthAndDay_CalculatesCorrectly(int month, int day)
    {
        var today = DateTime.Now;
        var birthYear = today.Year - 5;

        var animal = CreateAnimal(new DateOnly(birthYear, month, day));

        var age = _service.GetAge(animal);

        var expectedAge = 5;
        if (today.Month < month || (today.Month == month && today.Day < day))
        {
            expectedAge = 4;
        }

        Assert.Equal(expectedAge, age);
    }

    [Fact]
    public void GetAge_CalledMultipleTimes_ReturnsConsistentResults()
    {
        var animal = CreateAnimal(new DateOnly(2020, 6, 15));

        var age1 = _service.GetAge(animal);
        var age2 = _service.GetAge(animal);
        var age3 = _service.GetAge(animal);

        Assert.Equal(age1, age2);
        Assert.Equal(age2, age3);
    }
}
