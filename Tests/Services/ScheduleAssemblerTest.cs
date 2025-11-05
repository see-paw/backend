using Application.Interfaces;
using Application.Scheduling;
using Application.Services;
using Domain;
using Domain.Enums;

namespace Tests.Services;

/// <summary>
/// Tests for ScheduleAssembler using equivalence class partitioning and boundary value analysis.
/// Focuses on grouping logic, date handling, null/empty collections, and edge cases.
/// </summary>
public class ScheduleAssemblerTests
{
    private readonly IScheduleAssembler _assembler;
    private readonly Animal _testAnimal;
    private readonly Shelter _testShelter;

    public ScheduleAssemblerTests()
    {
        _assembler = new ScheduleAssembler();

        _testShelter = new Shelter
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test Shelter",
            Street = "Test St",
            City = "Test City",
            PostalCode = "1234-567",
            Phone = "912345678",
            NIF = "123456789",
            OpeningTime = new TimeOnly(9, 0),
            ClosingTime = new TimeOnly(18, 0)
        };

        _testAnimal = new Animal
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test Animal",
            Species = Species.Dog,
            Size = SizeType.Medium,
            Sex = SexType.Male,
            Colour = "Brown",
            BirthDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-2)),
            Sterilized = true,
            Cost = 50m,
            ShelterId = _testShelter.Id,
            Shelter = _testShelter,
            BreedId = Guid.NewGuid().ToString()
        };
    }

    #region Empty Collections - Equivalence Classes

    [Fact]
    public void AssembleWeekSchedule_AllCollectionsEmpty_ReturnsSevenEmptyDays()
    {
        var startDate = new DateOnly(2025, 1, 6);

        var result = _assembler.AssembleWeekSchedule(
            Array.Empty<ActivitySlot>(),
            Array.Empty<ShelterUnavailabilitySlot>(),
            Array.Empty<TimeBlock>(),
            _testAnimal,
            startDate);

        Assert.Equal(7, result.WeekSchedule.Count);
        Assert.All(result.WeekSchedule, day =>
        {
            Assert.Empty(day.AvailableSlots);
            Assert.Empty(day.ReservedSlots);
            Assert.Empty(day.UnavailableSlots);
        });
    }

    [Fact]
    public void AssembleWeekSchedule_NullReservedSlots_ThrowsException()
    {
        var startDate = new DateOnly(2025, 1, 6);

        Assert.ThrowsAny<Exception>(() => _assembler.AssembleWeekSchedule(
            null!,
            Array.Empty<ShelterUnavailabilitySlot>(),
            Array.Empty<TimeBlock>(),
            _testAnimal,
            startDate));
    }

    [Fact]
    public void AssembleWeekSchedule_NullUnavailableSlots_ThrowsException()
    {
        var startDate = new DateOnly(2025, 1, 6);

        Assert.ThrowsAny<Exception>(() => _assembler.AssembleWeekSchedule(
            Array.Empty<ActivitySlot>(),
            null!,
            Array.Empty<TimeBlock>(),
            _testAnimal,
            startDate));
    }

    [Fact]
    public void AssembleWeekSchedule_NullAvailableSlots_ThrowsException()
    {
        var startDate = new DateOnly(2025, 1, 6);

        Assert.ThrowsAny<Exception>(() => _assembler.AssembleWeekSchedule(
            Array.Empty<ActivitySlot>(),
            Array.Empty<ShelterUnavailabilitySlot>(),
            null!,
            _testAnimal,
            startDate));
    }

    [Fact]
    public void AssembleWeekSchedule_NullAnimal_ThrowsOrProducesInvalidSchedule()
    {
        var startDate = new DateOnly(2025, 1, 6);

        Assert.ThrowsAny<Exception>(() => _assembler.AssembleWeekSchedule(
            Array.Empty<ActivitySlot>(),
            Array.Empty<ShelterUnavailabilitySlot>(),
            Array.Empty<TimeBlock>(),
            null!,
            startDate));
    }

    #endregion

    #region Start Date - Boundary Value Analysis

    [Fact]
    public void AssembleWeekSchedule_StartDateMinValue_ReturnsSevenDays()
    {
        var startDate = DateOnly.MinValue;

        var result = _assembler.AssembleWeekSchedule(
            Array.Empty<ActivitySlot>(),
            Array.Empty<ShelterUnavailabilitySlot>(),
            Array.Empty<TimeBlock>(),
            _testAnimal,
            startDate);

        Assert.Equal(7, result.WeekSchedule.Count);
        Assert.Equal(startDate, result.WeekSchedule[0].Date);
        Assert.Equal(startDate.AddDays(6), result.WeekSchedule[6].Date);
    }

    [Fact]
    public void AssembleWeekSchedule_StartDateMaxValue_ThrowsOrOverflows()
    {
        var startDate = DateOnly.MaxValue;

        Assert.ThrowsAny<Exception>(() => _assembler.AssembleWeekSchedule(
            Array.Empty<ActivitySlot>(),
            Array.Empty<ShelterUnavailabilitySlot>(),
            Array.Empty<TimeBlock>(),
            _testAnimal,
            startDate));
    }

    [Fact]
    public void AssembleWeekSchedule_StartDateNearMaxValue_ThrowsOrOverflows()
    {
        var startDate = DateOnly.MaxValue.AddDays(-6);

        Assert.ThrowsAny<Exception>(() => _assembler.AssembleWeekSchedule(
            Array.Empty<ActivitySlot>(),
            Array.Empty<ShelterUnavailabilitySlot>(),
            Array.Empty<TimeBlock>(),
            _testAnimal,
            startDate));
    }

    [Theory]
    [InlineData(-365)]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(365)]
    public void AssembleWeekSchedule_VariousStartDates_ReturnsSevenDays(int daysOffset)
    {
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(daysOffset);

        var result = _assembler.AssembleWeekSchedule(
            Array.Empty<ActivitySlot>(),
            Array.Empty<ShelterUnavailabilitySlot>(),
            Array.Empty<TimeBlock>(),
            _testAnimal,
            startDate);

        Assert.Equal(7, result.WeekSchedule.Count);
        Assert.Equal(startDate, result.StartDate);
    }

    #endregion

    #region Week Schedule Structure

    [Fact]
    public void AssembleWeekSchedule_AlwaysReturnsExactlySevenDays()
    {
        var startDate = new DateOnly(2025, 1, 6);

        var result = _assembler.AssembleWeekSchedule(
            Array.Empty<ActivitySlot>(),
            Array.Empty<ShelterUnavailabilitySlot>(),
            Array.Empty<TimeBlock>(),
            _testAnimal,
            startDate);

        Assert.Equal(7, result.WeekSchedule.Count);
    }

    [Fact]
    public void AssembleWeekSchedule_DaysAreConsecutive()
    {
        var startDate = new DateOnly(2025, 1, 6);

        var result = _assembler.AssembleWeekSchedule(
            Array.Empty<ActivitySlot>(),
            Array.Empty<ShelterUnavailabilitySlot>(),
            Array.Empty<TimeBlock>(),
            _testAnimal,
            startDate);

        for (int i = 0; i < 7; i++)
        {
            Assert.Equal(startDate.AddDays(i), result.WeekSchedule[i].Date);
        }
    }

    [Fact]
    public void AssembleWeekSchedule_DaysAreInOrder()
    {
        var startDate = new DateOnly(2025, 1, 6);

        var result = _assembler.AssembleWeekSchedule(
            Array.Empty<ActivitySlot>(),
            Array.Empty<ShelterUnavailabilitySlot>(),
            Array.Empty<TimeBlock>(),
            _testAnimal,
            startDate);

        for (int i = 0; i < 6; i++)
        {
            Assert.True(result.WeekSchedule[i].Date < result.WeekSchedule[i + 1].Date);
        }
    }

    #endregion

    #region Animal and Shelter Assignment

    [Fact]
    public void AssembleWeekSchedule_AnimalIsAssigned()
    {
        var startDate = new DateOnly(2025, 1, 6);

        var result = _assembler.AssembleWeekSchedule(
            Array.Empty<ActivitySlot>(),
            Array.Empty<ShelterUnavailabilitySlot>(),
            Array.Empty<TimeBlock>(),
            _testAnimal,
            startDate);

        Assert.Same(_testAnimal, result.Animal);
    }

    [Fact]
    public void AssembleWeekSchedule_ShelterIsAssignedFromAnimal()
    {
        var startDate = new DateOnly(2025, 1, 6);

        var result = _assembler.AssembleWeekSchedule(
            Array.Empty<ActivitySlot>(),
            Array.Empty<ShelterUnavailabilitySlot>(),
            Array.Empty<TimeBlock>(),
            _testAnimal,
            startDate);

        Assert.Same(_testAnimal.Shelter, result.Shelter);
    }

    [Fact]
    public void AssembleWeekSchedule_AnimalWithNullShelter_AllowsNullShelter()
    {
        var startDate = new DateOnly(2025, 1, 6);
        var animalWithoutShelter = new Animal
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test",
            Species = Species.Dog,
            Size = SizeType.Medium,
            Sex = SexType.Male,
            Colour = "Test",
            BirthDate = DateOnly.FromDateTime(DateTime.UtcNow),
            Sterilized = false,
            Cost = 0,
            ShelterId = Guid.NewGuid().ToString(),
            Shelter = null!,
            BreedId = Guid.NewGuid().ToString()
        };

        var result = _assembler.AssembleWeekSchedule(
            Array.Empty<ActivitySlot>(),
            Array.Empty<ShelterUnavailabilitySlot>(),
            Array.Empty<TimeBlock>(),
            animalWithoutShelter,
            startDate);

        Assert.Null(result.Shelter);
    }

    #endregion

    #region Reserved Slots Distribution - Equivalence Classes

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(6)]
    public void AssembleWeekSchedule_ReservedSlotOnSpecificDay_AppearsInCorrectDay(int dayOffset)
    {
        var startDate = new DateOnly(2025, 1, 6);
        var targetDate = startDate.AddDays(dayOffset);

        var reservedSlots = new List<ActivitySlot>
        {
            CreateActivitySlot(targetDate.ToDateTime(new TimeOnly(10, 0)),
                targetDate.ToDateTime(new TimeOnly(11, 0)))
        };

        var result = _assembler.AssembleWeekSchedule(
            reservedSlots,
            Array.Empty<ShelterUnavailabilitySlot>(),
            Array.Empty<TimeBlock>(),
            _testAnimal,
            startDate);

        Assert.Single(result.WeekSchedule[dayOffset].ReservedSlots);
        Assert.All(result.WeekSchedule.Where((_, i) => i != dayOffset),
            day => Assert.Empty(day.ReservedSlots));
    }

    [Fact]
    public void AssembleWeekSchedule_MultipleSlotsOnSameDay_AllAppearInThatDay()
    {
        var startDate = new DateOnly(2025, 1, 6);
        var targetDate = startDate.AddDays(2);

        var reservedSlots = new List<ActivitySlot>
        {
            CreateActivitySlot(targetDate.ToDateTime(new TimeOnly(10, 0)),
                targetDate.ToDateTime(new TimeOnly(11, 0))),
            CreateActivitySlot(targetDate.ToDateTime(new TimeOnly(14, 0)),
                targetDate.ToDateTime(new TimeOnly(15, 0))),
            CreateActivitySlot(targetDate.ToDateTime(new TimeOnly(16, 0)),
                targetDate.ToDateTime(new TimeOnly(17, 0)))
        };

        var result = _assembler.AssembleWeekSchedule(
            reservedSlots,
            Array.Empty<ShelterUnavailabilitySlot>(),
            Array.Empty<TimeBlock>(),
            _testAnimal,
            startDate);

        Assert.Equal(3, result.WeekSchedule[2].ReservedSlots.Count);
    }

    [Fact]
    public void AssembleWeekSchedule_SlotsOnAllDays_DistributedCorrectly()
    {
        var startDate = new DateOnly(2025, 1, 6);
        var reservedSlots = new List<ActivitySlot>();

        for (int i = 0; i < 7; i++)
        {
            var date = startDate.AddDays(i);
            reservedSlots.Add(CreateActivitySlot(
                date.ToDateTime(new TimeOnly(10, 0)),
                date.ToDateTime(new TimeOnly(11, 0))));
        }

        var result = _assembler.AssembleWeekSchedule(
            reservedSlots,
            Array.Empty<ShelterUnavailabilitySlot>(),
            Array.Empty<TimeBlock>(),
            _testAnimal,
            startDate);

        Assert.All(result.WeekSchedule, day => Assert.Single(day.ReservedSlots));
    }

    [Fact]
    public void AssembleWeekSchedule_SlotBeforeWeekStart_NotIncluded()
    {
        var startDate = new DateOnly(2025, 1, 6);

        var reservedSlots = new List<ActivitySlot>
        {
            CreateActivitySlot(startDate.AddDays(-1).ToDateTime(new TimeOnly(10, 0)),
                startDate.AddDays(-1).ToDateTime(new TimeOnly(11, 0)))
        };

        var result = _assembler.AssembleWeekSchedule(
            reservedSlots,
            Array.Empty<ShelterUnavailabilitySlot>(),
            Array.Empty<TimeBlock>(),
            _testAnimal,
            startDate);

        Assert.All(result.WeekSchedule, day => Assert.Empty(day.ReservedSlots));
    }

    [Fact]
    public void AssembleWeekSchedule_SlotAfterWeekEnd_NotIncluded()
    {
        var startDate = new DateOnly(2025, 1, 6);

        var reservedSlots = new List<ActivitySlot>
        {
            CreateActivitySlot(startDate.AddDays(7).ToDateTime(new TimeOnly(10, 0)),
                startDate.AddDays(7).ToDateTime(new TimeOnly(11, 0)))
        };

        var result = _assembler.AssembleWeekSchedule(
            reservedSlots,
            Array.Empty<ShelterUnavailabilitySlot>(),
            Array.Empty<TimeBlock>(),
            _testAnimal,
            startDate);

        Assert.All(result.WeekSchedule, day => Assert.Empty(day.ReservedSlots));
    }

    [Fact]
    public void AssembleWeekSchedule_SlotOnLastDayOfWeek_IsIncluded()
    {
        var startDate = new DateOnly(2025, 1, 6);
        var lastDay = startDate.AddDays(6);

        var reservedSlots = new List<ActivitySlot>
        {
            CreateActivitySlot(lastDay.ToDateTime(new TimeOnly(10, 0)),
                lastDay.ToDateTime(new TimeOnly(11, 0)))
        };

        var result = _assembler.AssembleWeekSchedule(
            reservedSlots,
            Array.Empty<ShelterUnavailabilitySlot>(),
            Array.Empty<TimeBlock>(),
            _testAnimal,
            startDate);

        Assert.Single(result.WeekSchedule[6].ReservedSlots);
    }

    #endregion

    #region Unavailable Slots Distribution - Equivalence Classes

    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    [InlineData(6)]
    public void AssembleWeekSchedule_UnavailableSlotOnSpecificDay_AppearsInCorrectDay(int dayOffset)
    {
        var startDate = new DateOnly(2025, 1, 6);
        var targetDate = startDate.AddDays(dayOffset);

        var unavailableSlots = new List<ShelterUnavailabilitySlot>
        {
            CreateUnavailabilitySlot(targetDate.ToDateTime(new TimeOnly(10, 0)),
                targetDate.ToDateTime(new TimeOnly(11, 0)))
        };

        var result = _assembler.AssembleWeekSchedule(
            Array.Empty<ActivitySlot>(),
            unavailableSlots,
            Array.Empty<TimeBlock>(),
            _testAnimal,
            startDate);

        Assert.Single(result.WeekSchedule[dayOffset].UnavailableSlots);
        Assert.All(result.WeekSchedule.Where((_, i) => i != dayOffset),
            day => Assert.Empty(day.UnavailableSlots));
    }

    [Fact]
    public void AssembleWeekSchedule_MultipleUnavailableSlotsOnSameDay_AllAppear()
    {
        var startDate = new DateOnly(2025, 1, 6);
        var targetDate = startDate.AddDays(4);

        var unavailableSlots = new List<ShelterUnavailabilitySlot>
        {
            CreateUnavailabilitySlot(targetDate.ToDateTime(new TimeOnly(10, 0)),
                targetDate.ToDateTime(new TimeOnly(11, 0))),
            CreateUnavailabilitySlot(targetDate.ToDateTime(new TimeOnly(15, 0)),
                targetDate.ToDateTime(new TimeOnly(16, 0)))
        };

        var result = _assembler.AssembleWeekSchedule(
            Array.Empty<ActivitySlot>(),
            unavailableSlots,
            Array.Empty<TimeBlock>(),
            _testAnimal,
            startDate);

        Assert.Equal(2, result.WeekSchedule[4].UnavailableSlots.Count);
    }

    [Fact]
    public void AssembleWeekSchedule_UnavailableSlotsOutsideWeek_NotIncluded()
    {
        var startDate = new DateOnly(2025, 1, 6);

        var unavailableSlots = new List<ShelterUnavailabilitySlot>
        {
            CreateUnavailabilitySlot(startDate.AddDays(-5).ToDateTime(new TimeOnly(10, 0)),
                startDate.AddDays(-5).ToDateTime(new TimeOnly(11, 0))),
            CreateUnavailabilitySlot(startDate.AddDays(10).ToDateTime(new TimeOnly(10, 0)),
                startDate.AddDays(10).ToDateTime(new TimeOnly(11, 0)))
        };

        var result = _assembler.AssembleWeekSchedule(
            Array.Empty<ActivitySlot>(),
            unavailableSlots,
            Array.Empty<TimeBlock>(),
            _testAnimal,
            startDate);

        Assert.All(result.WeekSchedule, day => Assert.Empty(day.UnavailableSlots));
    }

    #endregion

    #region Available Slots Distribution - Equivalence Classes

    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    [InlineData(6)]
    public void AssembleWeekSchedule_AvailableSlotOnSpecificDay_AppearsInCorrectDay(int dayOffset)
    {
        var startDate = new DateOnly(2025, 1, 6);
        var targetDate = startDate.AddDays(dayOffset);

        var availableSlots = new List<TimeBlock>
        {
            CreateTimeBlock(targetDate, new TimeSpan(10, 0, 0), new TimeSpan(11, 0, 0))
        };

        var result = _assembler.AssembleWeekSchedule(
            Array.Empty<ActivitySlot>(),
            Array.Empty<ShelterUnavailabilitySlot>(),
            availableSlots,
            _testAnimal,
            startDate);

        Assert.Single(result.WeekSchedule[dayOffset].AvailableSlots);
        Assert.All(result.WeekSchedule.Where((_, i) => i != dayOffset),
            day => Assert.Empty(day.AvailableSlots));
    }

    [Fact]
    public void AssembleWeekSchedule_MultipleAvailableSlotsOnSameDay_AllAppear()
    {
        var startDate = new DateOnly(2025, 1, 6);
        var targetDate = startDate.AddDays(1);

        var availableSlots = new List<TimeBlock>
        {
            CreateTimeBlock(targetDate, new TimeSpan(9, 0, 0), new TimeSpan(12, 0, 0)),
            CreateTimeBlock(targetDate, new TimeSpan(14, 0, 0), new TimeSpan(18, 0, 0))
        };

        var result = _assembler.AssembleWeekSchedule(
            Array.Empty<ActivitySlot>(),
            Array.Empty<ShelterUnavailabilitySlot>(),
            availableSlots,
            _testAnimal,
            startDate);

        Assert.Equal(2, result.WeekSchedule[1].AvailableSlots.Count);
    }

    [Fact]
    public void AssembleWeekSchedule_AvailableSlotsOutsideWeek_NotIncluded()
    {
        var startDate = new DateOnly(2025, 1, 6);

        var availableSlots = new List<TimeBlock>
        {
            CreateTimeBlock(startDate.AddDays(-1), new TimeSpan(10, 0, 0), new TimeSpan(11, 0, 0)),
            CreateTimeBlock(startDate.AddDays(7), new TimeSpan(10, 0, 0), new TimeSpan(11, 0, 0))
        };

        var result = _assembler.AssembleWeekSchedule(
            Array.Empty<ActivitySlot>(),
            Array.Empty<ShelterUnavailabilitySlot>(),
            availableSlots,
            _testAnimal,
            startDate);

        Assert.All(result.WeekSchedule, day => Assert.Empty(day.AvailableSlots));
    }

    #endregion

    #region Mixed Collections

    [Fact]
    public void AssembleWeekSchedule_AllThreeTypesOnSameDay_AllDistributed()
    {
        var startDate = new DateOnly(2025, 1, 6);
        var targetDate = startDate.AddDays(2);

        var reservedSlots = new List<ActivitySlot>
        {
            CreateActivitySlot(targetDate.ToDateTime(new TimeOnly(10, 0)),
                targetDate.ToDateTime(new TimeOnly(11, 0)))
        };

        var unavailableSlots = new List<ShelterUnavailabilitySlot>
        {
            CreateUnavailabilitySlot(targetDate.ToDateTime(new TimeOnly(14, 0)),
                targetDate.ToDateTime(new TimeOnly(15, 0)))
        };

        var availableSlots = new List<TimeBlock>
        {
            CreateTimeBlock(targetDate, new TimeSpan(9, 0, 0), new TimeSpan(10, 0, 0)),
            CreateTimeBlock(targetDate, new TimeSpan(11, 0, 0), new TimeSpan(14, 0, 0)),
            CreateTimeBlock(targetDate, new TimeSpan(15, 0, 0), new TimeSpan(18, 0, 0))
        };

        var result = _assembler.AssembleWeekSchedule(
            reservedSlots,
            unavailableSlots,
            availableSlots,
            _testAnimal,
            startDate);

        Assert.Single(result.WeekSchedule[2].ReservedSlots);
        Assert.Single(result.WeekSchedule[2].UnavailableSlots);
        Assert.Equal(3, result.WeekSchedule[2].AvailableSlots.Count);
    }

    [Fact]
    public void AssembleWeekSchedule_AllThreeTypesOnDifferentDays_CorrectlyDistributed()
    {
        var startDate = new DateOnly(2025, 1, 6);

        var reservedSlots = new List<ActivitySlot>
        {
            CreateActivitySlot(startDate.AddDays(0).ToDateTime(new TimeOnly(10, 0)),
                startDate.AddDays(0).ToDateTime(new TimeOnly(11, 0)))
        };

        var unavailableSlots = new List<ShelterUnavailabilitySlot>
        {
            CreateUnavailabilitySlot(startDate.AddDays(3).ToDateTime(new TimeOnly(14, 0)),
                startDate.AddDays(3).ToDateTime(new TimeOnly(15, 0)))
        };

        var availableSlots = new List<TimeBlock>
        {
            CreateTimeBlock(startDate.AddDays(6), new TimeSpan(9, 0, 0), new TimeSpan(18, 0, 0))
        };

        var result = _assembler.AssembleWeekSchedule(
            reservedSlots,
            unavailableSlots,
            availableSlots,
            _testAnimal,
            startDate);

        Assert.Single(result.WeekSchedule[0].ReservedSlots);
        Assert.Empty(result.WeekSchedule[0].UnavailableSlots);
        Assert.Empty(result.WeekSchedule[0].AvailableSlots);

        Assert.Empty(result.WeekSchedule[3].ReservedSlots);
        Assert.Single(result.WeekSchedule[3].UnavailableSlots);
        Assert.Empty(result.WeekSchedule[3].AvailableSlots);

        Assert.Empty(result.WeekSchedule[6].ReservedSlots);
        Assert.Empty(result.WeekSchedule[6].UnavailableSlots);
        Assert.Single(result.WeekSchedule[6].AvailableSlots);
    }

    [Fact]
    public void AssembleWeekSchedule_DenseSchedule_HandlesCorrectly()
    {
        var startDate = new DateOnly(2025, 1, 6);
        var reservedSlots = new List<ActivitySlot>();
        var unavailableSlots = new List<ShelterUnavailabilitySlot>();
        var availableSlots = new List<TimeBlock>();

        for (int day = 0; day < 7; day++)
        {
            var date = startDate.AddDays(day);
            reservedSlots.Add(CreateActivitySlot(
                date.ToDateTime(new TimeOnly(10, 0)),
                date.ToDateTime(new TimeOnly(11, 0))));
            unavailableSlots.Add(CreateUnavailabilitySlot(
                date.ToDateTime(new TimeOnly(14, 0)),
                date.ToDateTime(new TimeOnly(15, 0))));
            availableSlots.Add(CreateTimeBlock(date, new TimeSpan(9, 0, 0), new TimeSpan(10, 0, 0)));
        }

        var result = _assembler.AssembleWeekSchedule(
            reservedSlots,
            unavailableSlots,
            availableSlots,
            _testAnimal,
            startDate);

        Assert.All(result.WeekSchedule, day =>
        {
            Assert.Single(day.ReservedSlots);
            Assert.Single(day.UnavailableSlots);
            Assert.Single(day.AvailableSlots);
        });
    }

    #endregion

    #region Slot Time Boundaries

    [Fact]
    public void AssembleWeekSchedule_SlotAtMidnight_GroupedCorrectly()
    {
        var startDate = new DateOnly(2025, 1, 6);
        var targetDate = startDate.AddDays(3);

        var reservedSlots = new List<ActivitySlot>
        {
            CreateActivitySlot(targetDate.ToDateTime(new TimeOnly(0, 0, 0)),
                targetDate.ToDateTime(new TimeOnly(1, 0, 0)))
        };

        var result = _assembler.AssembleWeekSchedule(
            reservedSlots,
            Array.Empty<ShelterUnavailabilitySlot>(),
            Array.Empty<TimeBlock>(),
            _testAnimal,
            startDate);

        Assert.Single(result.WeekSchedule[3].ReservedSlots);
    }

    [Fact]
    public void AssembleWeekSchedule_SlotAtEndOfDay_GroupedCorrectly()
    {
        var startDate = new DateOnly(2025, 1, 6);
        var targetDate = startDate.AddDays(2);

        var reservedSlots = new List<ActivitySlot>
        {
            CreateActivitySlot(targetDate.ToDateTime(new TimeOnly(23, 0, 0)),
                targetDate.ToDateTime(new TimeOnly(23, 59, 59)))
        };

        var result = _assembler.AssembleWeekSchedule(
            reservedSlots,
            Array.Empty<ShelterUnavailabilitySlot>(),
            Array.Empty<TimeBlock>(),
            _testAnimal,
            startDate);

        Assert.Single(result.WeekSchedule[2].ReservedSlots);
    }

    [Fact]
    public void AssembleWeekSchedule_SlotSpanningMidnight_GroupedByStartDate()
    {
        var startDate = new DateOnly(2025, 1, 6);
        var date1 = startDate.AddDays(2);
        var date2 = startDate.AddDays(3);

        var reservedSlots = new List<ActivitySlot>
        {
            CreateActivitySlot(date1.ToDateTime(new TimeOnly(23, 0, 0)),
                date2.ToDateTime(new TimeOnly(1, 0, 0)))
        };

        var result = _assembler.AssembleWeekSchedule(
            reservedSlots,
            Array.Empty<ShelterUnavailabilitySlot>(),
            Array.Empty<TimeBlock>(),
            _testAnimal,
            startDate);

        Assert.Single(result.WeekSchedule[2].ReservedSlots);
        Assert.Empty(result.WeekSchedule[3].ReservedSlots);
    }

    #endregion

    #region Large Collections

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    public void AssembleWeekSchedule_ManyReservedSlots_HandlesCorrectly(int count)
    {
        var startDate = new DateOnly(2025, 1, 6);
        var reservedSlots = new List<ActivitySlot>();

        for (int i = 0; i < count; i++)
        {
            var dayIndex = i % 7;
            var date = startDate.AddDays(dayIndex);
            reservedSlots.Add(CreateActivitySlot(
                date.ToDateTime(new TimeOnly(10, 0)).AddMinutes(i),
                date.ToDateTime(new TimeOnly(10, 30)).AddMinutes(i)));
        }

        var result = _assembler.AssembleWeekSchedule(
            reservedSlots,
            Array.Empty<ShelterUnavailabilitySlot>(),
            Array.Empty<TimeBlock>(),
            _testAnimal,
            startDate);

        var totalSlots = result.WeekSchedule.Sum(d => d.ReservedSlots.Count);
        Assert.Equal(count, totalSlots);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    public void AssembleWeekSchedule_ManyAvailableSlots_HandlesCorrectly(int count)
    {
        var startDate = new DateOnly(2025, 1, 6);
        var availableSlots = new List<TimeBlock>();

        for (int i = 0; i < count; i++)
        {
            var dayIndex = i % 7;
            var date = startDate.AddDays(dayIndex);
            availableSlots.Add(CreateTimeBlock(date,
                new TimeSpan(10, 0, 0).Add(TimeSpan.FromMinutes(i)),
                new TimeSpan(10, 30, 0).Add(TimeSpan.FromMinutes(i))));
        }

        var result = _assembler.AssembleWeekSchedule(
            Array.Empty<ActivitySlot>(),
            Array.Empty<ShelterUnavailabilitySlot>(),
            availableSlots,
            _testAnimal,
            startDate);

        var totalSlots = result.WeekSchedule.Sum(d => d.AvailableSlots.Count);
        Assert.Equal(count, totalSlots);
    }

    #endregion

    #region Duplicate Dates

    [Fact]
    public void AssembleWeekSchedule_DuplicateReservedSlotsOnSameDay_BothIncluded()
    {
        var startDate = new DateOnly(2025, 1, 6);
        var targetDate = startDate.AddDays(3);

        var reservedSlots = new List<ActivitySlot>
        {
            CreateActivitySlot(targetDate.ToDateTime(new TimeOnly(10, 0)),
                targetDate.ToDateTime(new TimeOnly(11, 0))),
            CreateActivitySlot(targetDate.ToDateTime(new TimeOnly(10, 0)),
                targetDate.ToDateTime(new TimeOnly(11, 0)))
        };

        var result = _assembler.AssembleWeekSchedule(
            reservedSlots,
            Array.Empty<ShelterUnavailabilitySlot>(),
            Array.Empty<TimeBlock>(),
            _testAnimal,
            startDate);

        Assert.Equal(2, result.WeekSchedule[3].ReservedSlots.Count);
    }

    [Fact]
    public void AssembleWeekSchedule_DuplicateAvailableSlotsOnSameDay_BothIncluded()
    {
        var startDate = new DateOnly(2025, 1, 6);
        var targetDate = startDate.AddDays(1);

        var availableSlots = new List<TimeBlock>
        {
            CreateTimeBlock(targetDate, new TimeSpan(10, 0, 0), new TimeSpan(11, 0, 0)),
            CreateTimeBlock(targetDate, new TimeSpan(10, 0, 0), new TimeSpan(11, 0, 0))
        };

        var result = _assembler.AssembleWeekSchedule(
            Array.Empty<ActivitySlot>(),
            Array.Empty<ShelterUnavailabilitySlot>(),
            availableSlots,
            _testAnimal,
            startDate);

        Assert.Equal(2, result.WeekSchedule[1].AvailableSlots.Count);
    }

    #endregion

    #region Helper Methods

    private static ActivitySlot CreateActivitySlot(DateTime start, DateTime end)
    {
        return new ActivitySlot
        {
            Id = Guid.NewGuid().ToString(),
            ActivityId = Guid.NewGuid().ToString(),
            StartDateTime = start,
            EndDateTime = end,
            Status = SlotStatus.Reserved,
            Type = SlotType.Activity
        };
    }

    private static ShelterUnavailabilitySlot CreateUnavailabilitySlot(DateTime start, DateTime end)
    {
        return new ShelterUnavailabilitySlot
        {
            Id = Guid.NewGuid().ToString(),
            ShelterId = Guid.NewGuid().ToString(),
            Reason = "Test",
            StartDateTime = start,
            EndDateTime = end,
            Status = SlotStatus.Unavailable,
            Type = SlotType.ShelterUnavailable
        };
    }

    private static TimeBlock CreateTimeBlock(DateOnly date, TimeSpan start, TimeSpan end)
    {
        return new TimeBlock
        {
            Date = date,
            Start = start,
            End = end
        };
    }

    #endregion
}