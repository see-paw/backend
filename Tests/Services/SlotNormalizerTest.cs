using Application.Interfaces;
using Application.Services;
using Domain;
using Domain.Enums;

namespace Tests.Services;

/// <summary>
/// Tests for SlotNormalizer using equivalence class partitioning and boundary value analysis.
/// Focuses on multi-day splitting, clamping logic, overlap detection, and edge cases.
/// </summary>
public class SlotNormalizerTests
{
    private readonly ISlotNormalizer _normalizer;

    public SlotNormalizerTests()
    {
        _normalizer = new SlotNormalizer();
    }

    #region Empty/Null Input - Equivalence Classes

    [Fact]
    public void Normalize_EmptyCollection_ReturnsEmpty()
    {
        var opening = new TimeSpan(9, 0, 0);
        var closing = new TimeSpan(18, 0, 0);

        var result = _normalizer.Normalize(Enumerable.Empty<Slot>(), opening, closing);

        Assert.Empty(result.Slots);
    }

    [Fact]
    public void Normalize_NullCollection_ThrowsException()
    {
        var opening = new TimeSpan(9, 0, 0);
        var closing = new TimeSpan(18, 0, 0);

        Assert.ThrowsAny<Exception>(() => _normalizer.Normalize(null!, opening, closing));
    }

    #endregion

    #region Opening/Closing Hours - Boundary Value Analysis

    [Theory]
    [InlineData(0, 0, 0, 0, 0, 1)]
    [InlineData(0, 0, 0, 23, 59, 59)]
    [InlineData(23, 59, 59, 23, 59, 59)]
    public void Normalize_BoundaryOpeningClosingHours_ProcessesCorrectly(
        int openHour, int openMin, int openSec, int closeHour, int closeMin, int closeSec)
    {
        var opening = new TimeSpan(openHour, openMin, openSec);
        var closing = new TimeSpan(closeHour, closeMin, closeSec);
        var date = new DateOnly(2025, 1, 6);

        var slots = new List<Slot>
        {
            CreateActivitySlot(date.ToDateTime(new TimeOnly(12, 0)), 
                date.ToDateTime(new TimeOnly(14, 0)))
        };

        var result = _normalizer.Normalize(slots, opening, closing);

        Assert.NotNull(result);
    }

    [Fact]
    public void Normalize_OpeningEqualsClosing_FiltersAllSlots()
    {
        var opening = new TimeSpan(12, 0, 0);
        var closing = new TimeSpan(12, 0, 0);
        var date = new DateOnly(2025, 1, 6);

        var slots = new List<Slot>
        {
            CreateActivitySlot(date.ToDateTime(new TimeOnly(10, 0)), 
                date.ToDateTime(new TimeOnly(14, 0)))
        };

        var result = _normalizer.Normalize(slots, opening, closing);

        Assert.Empty(result.Slots);
    }

    [Fact]
    public void Normalize_OpeningAfterClosing_ProducesInvalidResults()
    {
        var opening = new TimeSpan(18, 0, 0);
        var closing = new TimeSpan(9, 0, 0);
        var date = new DateOnly(2025, 1, 6);

        var slots = new List<Slot>
        {
            CreateActivitySlot(date.ToDateTime(new TimeOnly(10, 0)), 
                date.ToDateTime(new TimeOnly(14, 0)))
        };

        var result = _normalizer.Normalize(slots, opening, closing);

        Assert.Empty(result.Slots);
    }

    #endregion

    #region Single Day Slots - Equivalence Classes

    [Fact]
    public void Normalize_SingleDaySlot_RemainsUnchanged()
    {
        var opening = new TimeSpan(9, 0, 0);
        var closing = new TimeSpan(18, 0, 0);
        var date = new DateOnly(2025, 1, 6);

        var slots = new List<Slot>
        {
            CreateActivitySlot(date.ToDateTime(new TimeOnly(10, 0)), 
                date.ToDateTime(new TimeOnly(14, 0)))
        };

        var result = _normalizer.Normalize(slots, opening, closing);

        Assert.Single(result.Slots);
        Assert.Equal(slots[0].Id, result.Slots[0].Id);
    }

    [Fact]
    public void Normalize_SingleDaySlotAtMidnight_HandlesCorrectly()
    {
        var opening = new TimeSpan(0, 0, 0);
        var closing = new TimeSpan(23, 59, 59);
        var date = new DateOnly(2025, 1, 6);

        var slots = new List<Slot>
        {
            CreateActivitySlot(date.ToDateTime(new TimeOnly(0, 0, 0)), 
                date.ToDateTime(new TimeOnly(0, 0, 1)))
        };

        var result = _normalizer.Normalize(slots, opening, closing);

        Assert.Single(result.Slots);
    }

    [Fact]
    public void Normalize_SingleDaySlotAtEndOfDay_HandlesCorrectly()
    {
        var opening = new TimeSpan(0, 0, 0);
        var closing = new TimeSpan(23, 59, 59);
        var date = new DateOnly(2025, 1, 6);

        var slots = new List<Slot>
        {
            CreateActivitySlot(date.ToDateTime(new TimeOnly(23, 59, 58)), 
                date.ToDateTime(new TimeOnly(23, 59, 59)))
        };

        var result = _normalizer.Normalize(slots, opening, closing);

        Assert.Single(result.Slots);
    }

    #endregion

    #region Multi-Day Slots - Equivalence Classes and Boundaries

    [Fact]
    public void Normalize_TwoDaySlot_SplitsIntoTwo()
    {
        var opening = new TimeSpan(9, 0, 0);
        var closing = new TimeSpan(18, 0, 0);
        var date1 = new DateOnly(2025, 1, 6);
        var date2 = new DateOnly(2025, 1, 7);

        var slots = new List<Slot>
        {
            CreateActivitySlot(date1.ToDateTime(new TimeOnly(14, 0)), 
                date2.ToDateTime(new TimeOnly(12, 0)))
        };

        var result = _normalizer.Normalize(slots, opening, closing);

        Assert.Equal(2, result.Slots.Count);
        Assert.Equal(date1, DateOnly.FromDateTime(result.Slots[0].StartDateTime));
        Assert.Equal(date2, DateOnly.FromDateTime(result.Slots[1].StartDateTime));
    }

    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(7)]
    public void Normalize_MultiDaySlot_SplitsCorrectly(int days)
    {
        var opening = new TimeSpan(9, 0, 0);
        var closing = new TimeSpan(18, 0, 0);
        var startDate = new DateOnly(2025, 1, 6);
        var endDate = startDate.AddDays(days - 1);

        var slots = new List<Slot>
        {
            CreateActivitySlot(startDate.ToDateTime(new TimeOnly(10, 0)), 
                endDate.ToDateTime(new TimeOnly(16, 0)))
        };

        var result = _normalizer.Normalize(slots, opening, closing);

        Assert.Equal(days, result.Slots.Count);
    }

    [Fact]
    public void Normalize_SlotSpanningMidnight_SplitsAtMidnight()
    {
        var opening = new TimeSpan(0, 0, 0);
        var closing = new TimeSpan(23, 59, 59);
        var date = new DateOnly(2025, 1, 6);

        var slots = new List<Slot>
        {
            CreateActivitySlot(date.ToDateTime(new TimeOnly(23, 0)), 
                date.AddDays(1).ToDateTime(new TimeOnly(1, 0)))
        };

        var result = _normalizer.Normalize(slots, opening, closing);

        Assert.Equal(2, result.Slots.Count);
        Assert.Equal(new TimeOnly(23, 59, 59), TimeOnly.FromDateTime(result.Slots[0].EndDateTime));
        Assert.Equal(new TimeOnly(0, 0, 0), TimeOnly.FromDateTime(result.Slots[1].StartDateTime));
    }

    [Fact]
    public void Normalize_SlotExactlyAtMidnight_DoesNotSplit()
    {
        var opening = new TimeSpan(0, 0, 0);
        var closing = new TimeSpan(23, 59, 59);
        var date = new DateOnly(2025, 1, 6);

        var slots = new List<Slot>
        {
            CreateActivitySlot(date.ToDateTime(new TimeOnly(0, 0, 0)), 
                date.AddDays(1).ToDateTime(new TimeOnly(0, 0, 0)))
        };

        var result = _normalizer.Normalize(slots, opening, closing);

        Assert.Single(result.Slots);
    }

    [Fact]
    public void Normalize_VeryLongSlot_SplitsIntoMultipleDays()
    {
        var opening = new TimeSpan(9, 0, 0);
        var closing = new TimeSpan(18, 0, 0);
        var startDate = new DateOnly(2025, 1, 1);
        var endDate = new DateOnly(2025, 1, 31);

        var slots = new List<Slot>
        {
            CreateActivitySlot(startDate.ToDateTime(new TimeOnly(10, 0)), 
                endDate.ToDateTime(new TimeOnly(16, 0)))
        };

        var result = _normalizer.Normalize(slots, opening, closing);

        Assert.Equal(31, result.Slots.Count);
    }

    #endregion

    #region Slot Overlap Detection - Boundary Value Analysis

    [Fact]
    public void Normalize_SlotCompletelyBeforeOpening_IsFiltered()
    {
        var opening = new TimeSpan(9, 0, 0);
        var closing = new TimeSpan(18, 0, 0);
        var date = new DateOnly(2025, 1, 6);

        var slots = new List<Slot>
        {
            CreateActivitySlot(date.ToDateTime(new TimeOnly(6, 0)), 
                date.ToDateTime(new TimeOnly(8, 0)))
        };

        var result = _normalizer.Normalize(slots, opening, closing);

        Assert.Empty(result.Slots);
    }

    [Fact]
    public void Normalize_SlotCompletelyAfterClosing_IsFiltered()
    {
        var opening = new TimeSpan(9, 0, 0);
        var closing = new TimeSpan(18, 0, 0);
        var date = new DateOnly(2025, 1, 6);

        var slots = new List<Slot>
        {
            CreateActivitySlot(date.ToDateTime(new TimeOnly(19, 0)), 
                date.ToDateTime(new TimeOnly(21, 0)))
        };

        var result = _normalizer.Normalize(slots, opening, closing);

        Assert.Empty(result.Slots);
    }

    [Fact]
    public void Normalize_SlotEndsExactlyAtOpening_IsFiltered()
    {
        var opening = new TimeSpan(9, 0, 0);
        var closing = new TimeSpan(18, 0, 0);
        var date = new DateOnly(2025, 1, 6);

        var slots = new List<Slot>
        {
            CreateActivitySlot(date.ToDateTime(new TimeOnly(7, 0)), 
                date.ToDateTime(new TimeOnly(9, 0, 0)))
        };

        var result = _normalizer.Normalize(slots, opening, closing);

        Assert.Empty(result.Slots);
    }

    [Fact]
    public void Normalize_SlotStartsExactlyAtClosing_IsFiltered()
    {
        var opening = new TimeSpan(9, 0, 0);
        var closing = new TimeSpan(18, 0, 0);
        var date = new DateOnly(2025, 1, 6);

        var slots = new List<Slot>
        {
            CreateActivitySlot(date.ToDateTime(new TimeOnly(18, 0, 0)), 
                date.ToDateTime(new TimeOnly(20, 0)))
        };

        var result = _normalizer.Normalize(slots, opening, closing);

        Assert.Empty(result.Slots);
    }

    [Fact]
    public void Normalize_SlotEndsOneTickAfterOpening_IsIncluded()
    {
        var opening = new TimeSpan(9, 0, 0);
        var closing = new TimeSpan(18, 0, 0);
        var date = new DateOnly(2025, 1, 6);

        var slots = new List<Slot>
        {
            CreateActivitySlot(date.ToDateTime(new TimeOnly(7, 0)), 
                date.ToDateTime(new TimeOnly(9, 0, 0)).AddTicks(1))
        };

        var result = _normalizer.Normalize(slots, opening, closing);

        Assert.Single(result.Slots);
    }

    [Fact]
    public void Normalize_SlotStartsOneTickBeforeClosing_IsIncluded()
    {
        var opening = new TimeSpan(9, 0, 0);
        var closing = new TimeSpan(18, 0, 0);
        var date = new DateOnly(2025, 1, 6);

        var slots = new List<Slot>
        {
            CreateActivitySlot(date.ToDateTime(new TimeOnly(18, 0, 0)).AddTicks(-1), 
                date.ToDateTime(new TimeOnly(20, 0)))
        };

        var result = _normalizer.Normalize(slots, opening, closing);

        Assert.Single(result.Slots);
    }

    #endregion

    #region Clamping Logic - Boundary Value Analysis

    [Fact]
    public void Normalize_SlotStartsBeforeOpening_IsClamped()
    {
        var opening = new TimeSpan(9, 0, 0);
        var closing = new TimeSpan(18, 0, 0);
        var date = new DateOnly(2025, 1, 6);

        var slots = new List<Slot>
        {
            CreateActivitySlot(date.ToDateTime(new TimeOnly(7, 0)), 
                date.ToDateTime(new TimeOnly(12, 0)))
        };

        var result = _normalizer.Normalize(slots, opening, closing);

        Assert.Single(result.Slots);
        Assert.Equal(opening, result.Slots[0].StartDateTime.TimeOfDay);
        Assert.Equal(new TimeSpan(12, 0, 0), result.Slots[0].EndDateTime.TimeOfDay);
    }

    [Fact]
    public void Normalize_SlotEndsAfterClosing_IsClamped()
    {
        var opening = new TimeSpan(9, 0, 0);
        var closing = new TimeSpan(18, 0, 0);
        var date = new DateOnly(2025, 1, 6);

        var slots = new List<Slot>
        {
            CreateActivitySlot(date.ToDateTime(new TimeOnly(15, 0)), 
                date.ToDateTime(new TimeOnly(20, 0)))
        };

        var result = _normalizer.Normalize(slots, opening, closing);

        Assert.Single(result.Slots);
        Assert.Equal(new TimeSpan(15, 0, 0), result.Slots[0].StartDateTime.TimeOfDay);
        Assert.Equal(closing, result.Slots[0].EndDateTime.TimeOfDay);
    }

    [Fact]
    public void Normalize_SlotExtendsOnBothSides_IsClampedBothEnds()
    {
        var opening = new TimeSpan(9, 0, 0);
        var closing = new TimeSpan(18, 0, 0);
        var date = new DateOnly(2025, 1, 6);

        var slots = new List<Slot>
        {
            CreateActivitySlot(date.ToDateTime(new TimeOnly(6, 0)), 
                date.ToDateTime(new TimeOnly(22, 0)))
        };

        var result = _normalizer.Normalize(slots, opening, closing);

        Assert.Single(result.Slots);
        Assert.Equal(opening, result.Slots[0].StartDateTime.TimeOfDay);
        Assert.Equal(closing, result.Slots[0].EndDateTime.TimeOfDay);
    }

    [Fact]
    public void Normalize_SlotStartsExactlyAtOpening_NotClamped()
    {
        var opening = new TimeSpan(9, 0, 0);
        var closing = new TimeSpan(18, 0, 0);
        var date = new DateOnly(2025, 1, 6);

        var slots = new List<Slot>
        {
            CreateActivitySlot(date.ToDateTime(new TimeOnly(9, 0, 0)), 
                date.ToDateTime(new TimeOnly(12, 0)))
        };

        var result = _normalizer.Normalize(slots, opening, closing);

        Assert.Single(result.Slots);
        Assert.Equal(opening, result.Slots[0].StartDateTime.TimeOfDay);
    }

    [Fact]
    public void Normalize_SlotEndsExactlyAtClosing_NotClamped()
    {
        var opening = new TimeSpan(9, 0, 0);
        var closing = new TimeSpan(18, 0, 0);
        var date = new DateOnly(2025, 1, 6);

        var slots = new List<Slot>
        {
            CreateActivitySlot(date.ToDateTime(new TimeOnly(15, 0)), 
                date.ToDateTime(new TimeOnly(18, 0, 0)))
        };

        var result = _normalizer.Normalize(slots, opening, closing);

        Assert.Single(result.Slots);
        Assert.Equal(closing, result.Slots[0].EndDateTime.TimeOfDay);
    }

    [Fact]
    public void Normalize_ClampingCreatesZeroDurationSlot_IsFiltered()
    {
        var opening = new TimeSpan(12, 0, 0);
        var closing = new TimeSpan(12, 0, 0);
        var date = new DateOnly(2025, 1, 6);

        var slots = new List<Slot>
        {
            CreateActivitySlot(date.ToDateTime(new TimeOnly(10, 0)), 
                date.ToDateTime(new TimeOnly(14, 0)))
        };

        var result = _normalizer.Normalize(slots, opening, closing);

        Assert.Empty(result.Slots);
    }

    [Fact]
    public void Normalize_ClampingCreatesOneTickDuration_IsIncluded()
    {
        var opening = new TimeSpan(12, 0, 0);
        var closing = new TimeSpan(12, 0, 0).Add(TimeSpan.FromTicks(1));
        var date = new DateOnly(2025, 1, 6);

        var slots = new List<Slot>
        {
            CreateActivitySlot(date.ToDateTime(new TimeOnly(10, 0)), 
                date.ToDateTime(new TimeOnly(14, 0)))
        };

        var result = _normalizer.Normalize(slots, opening, closing);

        Assert.Single(result.Slots);
    }

    #endregion

    #region Invalid Slot Filtering

    [Fact]
    public void Normalize_ZeroDurationSlot_IsFiltered()
    {
        var opening = new TimeSpan(9, 0, 0);
        var closing = new TimeSpan(18, 0, 0);
        var date = new DateOnly(2025, 1, 6);
        var time = date.ToDateTime(new TimeOnly(12, 0));

        var slots = new List<Slot>
        {
            CreateActivitySlot(time, time)
        };

        var result = _normalizer.Normalize(slots, opening, closing);

        Assert.Empty(result.Slots);
    }

    [Fact]
    public void Normalize_NegativeDurationSlot_IsFiltered()
    {
        var opening = new TimeSpan(9, 0, 0);
        var closing = new TimeSpan(18, 0, 0);
        var date = new DateOnly(2025, 1, 6);

        var slots = new List<Slot>
        {
            CreateActivitySlot(date.ToDateTime(new TimeOnly(14, 0)), 
                date.ToDateTime(new TimeOnly(12, 0)))
        };

        var result = _normalizer.Normalize(slots, opening, closing);

        Assert.Empty(result.Slots);
    }

    [Fact]
    public void Normalize_OneTickDurationSlot_IsIncluded()
    {
        var opening = new TimeSpan(9, 0, 0);
        var closing = new TimeSpan(18, 0, 0);
        var date = new DateOnly(2025, 1, 6);
        var start = date.ToDateTime(new TimeOnly(12, 0));

        var slots = new List<Slot>
        {
            CreateActivitySlot(start, start.AddTicks(1))
        };

        var result = _normalizer.Normalize(slots, opening, closing);

        Assert.Single(result.Slots);
    }

    #endregion

    #region Multiple Slots - Equivalence Classes

    [Fact]
    public void Normalize_MultipleValidSlots_AllProcessed()
    {
        var opening = new TimeSpan(9, 0, 0);
        var closing = new TimeSpan(18, 0, 0);
        var date = new DateOnly(2025, 1, 6);

        var slots = new List<Slot>
        {
            CreateActivitySlot(date.ToDateTime(new TimeOnly(10, 0)), 
                date.ToDateTime(new TimeOnly(11, 0))),
            CreateActivitySlot(date.ToDateTime(new TimeOnly(14, 0)), 
                date.ToDateTime(new TimeOnly(15, 0))),
            CreateActivitySlot(date.ToDateTime(new TimeOnly(16, 0)), 
                date.ToDateTime(new TimeOnly(17, 0)))
        };

        var result = _normalizer.Normalize(slots, opening, closing);

        Assert.Equal(3, result.Slots.Count);
    }

    [Fact]
    public void Normalize_MixedValidAndInvalidSlots_FiltersInvalid()
    {
        var opening = new TimeSpan(9, 0, 0);
        var closing = new TimeSpan(18, 0, 0);
        var date = new DateOnly(2025, 1, 6);

        var slots = new List<Slot>
        {
            CreateActivitySlot(date.ToDateTime(new TimeOnly(10, 0)), 
                date.ToDateTime(new TimeOnly(11, 0))),
            CreateActivitySlot(date.ToDateTime(new TimeOnly(6, 0)), 
                date.ToDateTime(new TimeOnly(7, 0))),
            CreateActivitySlot(date.ToDateTime(new TimeOnly(14, 0)), 
                date.ToDateTime(new TimeOnly(15, 0))),
            CreateActivitySlot(date.ToDateTime(new TimeOnly(20, 0)), 
                date.ToDateTime(new TimeOnly(22, 0)))
        };

        var result = _normalizer.Normalize(slots, opening, closing);

        Assert.Equal(2, result.Slots.Count);
    }

    [Fact]
    public void Normalize_MultipleMultiDaySlots_SplitsAll()
    {
        var opening = new TimeSpan(9, 0, 0);
        var closing = new TimeSpan(18, 0, 0);
        var date1 = new DateOnly(2025, 1, 6);
        var date2 = new DateOnly(2025, 1, 10);

        var slots = new List<Slot>
        {
            CreateActivitySlot(date1.ToDateTime(new TimeOnly(10, 0)), 
                date1.AddDays(1).ToDateTime(new TimeOnly(11, 0))),
            CreateActivitySlot(date2.ToDateTime(new TimeOnly(14, 0)), 
                date2.AddDays(2).ToDateTime(new TimeOnly(15, 0)))
        };

        var result = _normalizer.Normalize(slots, opening, closing);

        Assert.Equal(5, result.Slots.Count);
    }

    #endregion

    #region Slot Type Preservation - Equivalence Classes

    [Fact]
    public void Normalize_ActivitySlot_TypePreserved()
    {
        var opening = new TimeSpan(9, 0, 0);
        var closing = new TimeSpan(18, 0, 0);
        var date = new DateOnly(2025, 1, 6);

        var slots = new List<Slot>
        {
            CreateActivitySlot(date.ToDateTime(new TimeOnly(10, 0)), 
                date.ToDateTime(new TimeOnly(12, 0)))
        };

        var result = _normalizer.Normalize(slots, opening, closing);

        Assert.Single(result.Slots);
        Assert.IsType<ActivitySlot>(result.Slots[0]);
    }

    [Fact]
    public void Normalize_ShelterUnavailabilitySlot_TypePreserved()
    {
        var opening = new TimeSpan(9, 0, 0);
        var closing = new TimeSpan(18, 0, 0);
        var date = new DateOnly(2025, 1, 6);

        var slots = new List<Slot>
        {
            CreateUnavailabilitySlot(date.ToDateTime(new TimeOnly(10, 0)), 
                date.ToDateTime(new TimeOnly(12, 0)))
        };

        var result = _normalizer.Normalize(slots, opening, closing);

        Assert.Single(result.Slots);
        Assert.IsType<ShelterUnavailabilitySlot>(result.Slots[0]);
    }

    [Fact]
    public void Normalize_MultiDayActivitySlot_TypePreservedAfterSplit()
    {
        var opening = new TimeSpan(9, 0, 0);
        var closing = new TimeSpan(18, 0, 0);
        var date1 = new DateOnly(2025, 1, 6);
        var date2 = new DateOnly(2025, 1, 7);

        var slots = new List<Slot>
        {
            CreateActivitySlot(date1.ToDateTime(new TimeOnly(10, 0)), 
                date2.ToDateTime(new TimeOnly(12, 0)))
        };

        var result = _normalizer.Normalize(slots, opening, closing);

        Assert.Equal(2, result.Slots.Count);
        Assert.All(result.Slots, slot => Assert.IsType<ActivitySlot>(slot));
    }

    [Fact]
    public void Normalize_MultiDayUnavailabilitySlot_TypePreservedAfterSplit()
    {
        var opening = new TimeSpan(9, 0, 0);
        var closing = new TimeSpan(18, 0, 0);
        var date1 = new DateOnly(2025, 1, 6);
        var date2 = new DateOnly(2025, 1, 7);

        var slots = new List<Slot>
        {
            CreateUnavailabilitySlot(date1.ToDateTime(new TimeOnly(10, 0)), 
                date2.ToDateTime(new TimeOnly(12, 0)))
        };

        var result = _normalizer.Normalize(slots, opening, closing);

        Assert.Equal(2, result.Slots.Count);
        Assert.All(result.Slots, slot => Assert.IsType<ShelterUnavailabilitySlot>(slot));
    }

    [Fact]
    public void Normalize_MixedSlotTypes_BothTypesPreserved()
    {
        var opening = new TimeSpan(9, 0, 0);
        var closing = new TimeSpan(18, 0, 0);
        var date = new DateOnly(2025, 1, 6);

        var slots = new List<Slot>
        {
            CreateActivitySlot(date.ToDateTime(new TimeOnly(10, 0)), 
                date.ToDateTime(new TimeOnly(12, 0))),
            CreateUnavailabilitySlot(date.ToDateTime(new TimeOnly(14, 0)), 
                date.ToDateTime(new TimeOnly(16, 0)))
        };

        var result = _normalizer.Normalize(slots, opening, closing);

        Assert.Equal(2, result.Slots.Count);
        Assert.Contains(result.Slots, s => s is ActivitySlot);
        Assert.Contains(result.Slots, s => s is ShelterUnavailabilitySlot);
    }

    #endregion

    #region Result Ordering

    [Fact]
    public void Normalize_ResultsOrderedByDateThenTime()
    {
        var opening = new TimeSpan(9, 0, 0);
        var closing = new TimeSpan(18, 0, 0);
        var date1 = new DateOnly(2025, 1, 6);
        var date2 = new DateOnly(2025, 1, 8);

        var slots = new List<Slot>
        {
            CreateActivitySlot(date2.ToDateTime(new TimeOnly(10, 0)), 
                date2.ToDateTime(new TimeOnly(11, 0))),
            CreateActivitySlot(date1.ToDateTime(new TimeOnly(14, 0)), 
                date1.ToDateTime(new TimeOnly(15, 0))),
            CreateActivitySlot(date1.ToDateTime(new TimeOnly(10, 0)), 
                date1.ToDateTime(new TimeOnly(11, 0)))
        };

        var result = _normalizer.Normalize(slots, opening, closing);

        Assert.Equal(3, result.Slots.Count);
        for (int i = 0; i < result.Slots.Count - 1; i++)
        {
            var current = result.Slots[i];
            var next = result.Slots[i + 1];
            var currentDate = DateOnly.FromDateTime(current.StartDateTime);
            var nextDate = DateOnly.FromDateTime(next.StartDateTime);

            if (currentDate == nextDate)
            {
                Assert.True(current.StartDateTime.TimeOfDay <= next.StartDateTime.TimeOfDay);
            }
            else
            {
                Assert.True(currentDate < nextDate);
            }
        }
    }

    [Fact]
    public void Normalize_MultiDaySlotsWithOverlappingTimes_OrderedCorrectly()
    {
        var opening = new TimeSpan(9, 0, 0);
        var closing = new TimeSpan(18, 0, 0);
        var date1 = new DateOnly(2025, 1, 6);

        var slots = new List<Slot>
        {
            CreateActivitySlot(date1.ToDateTime(new TimeOnly(10, 0)), 
                date1.AddDays(2).ToDateTime(new TimeOnly(12, 0)))
        };

        var result = _normalizer.Normalize(slots, opening, closing);

        Assert.Equal(3, result.Slots.Count);
        for (int i = 0; i < result.Slots.Count - 1; i++)
        {
            Assert.True(DateOnly.FromDateTime(result.Slots[i].StartDateTime) <= 
                        DateOnly.FromDateTime(result.Slots[i + 1].StartDateTime));
        }
    }

    #endregion

    #region Activity and Shelter Properties Preservation

    [Fact]
    public void Normalize_ActivitySlotProperties_ArePreserved()
    {
        var opening = new TimeSpan(9, 0, 0);
        var closing = new TimeSpan(18, 0, 0);
        var date = new DateOnly(2025, 1, 6);
        var activityId = Guid.NewGuid().ToString();

        var slots = new List<Slot>
        {
            new ActivitySlot
            {
                Id = Guid.NewGuid().ToString(),
                ActivityId = activityId,
                StartDateTime = date.ToDateTime(new TimeOnly(10, 0)),
                EndDateTime = date.ToDateTime(new TimeOnly(12, 0)),
                Status = SlotStatus.Reserved,
                Type = SlotType.Activity
            }
        };

        var result = _normalizer.Normalize(slots, opening, closing);

        Assert.Single(result.Slots);
        var activitySlot = (ActivitySlot)result.Slots[0];
        Assert.Equal(activityId, activitySlot.ActivityId);
        Assert.Equal(SlotStatus.Reserved, activitySlot.Status);
        Assert.Equal(SlotType.Activity, activitySlot.Type);
    }

    [Fact]
    public void Normalize_UnavailabilitySlotProperties_ArePreserved()
    {
        var opening = new TimeSpan(9, 0, 0);
        var closing = new TimeSpan(18, 0, 0);
        var date = new DateOnly(2025, 1, 6);
        var shelterId = Guid.NewGuid().ToString();
        var reason = "Maintenance";

        var slots = new List<Slot>
        {
            new ShelterUnavailabilitySlot
            {
                Id = Guid.NewGuid().ToString(),
                ShelterId = shelterId,
                Reason = reason,
                StartDateTime = date.ToDateTime(new TimeOnly(10, 0)),
                EndDateTime = date.ToDateTime(new TimeOnly(12, 0)),
                Status = SlotStatus.Unavailable,
                Type = SlotType.ShelterUnavailable
            }
        };

        var result = _normalizer.Normalize(slots, opening, closing);

        Assert.Single(result.Slots);
        var unavailabilitySlot = (ShelterUnavailabilitySlot)result.Slots[0];
        Assert.Equal(shelterId, unavailabilitySlot.ShelterId);
        Assert.Equal(reason, unavailabilitySlot.Reason);
        Assert.Equal(SlotStatus.Unavailable, unavailabilitySlot.Status);
        Assert.Equal(SlotType.ShelterUnavailable, unavailabilitySlot.Type);
    }

    [Fact]
    public void Normalize_MultiDaySlotSplit_PropertiesPreservedInAllParts()
    {
        var opening = new TimeSpan(9, 0, 0);
        var closing = new TimeSpan(18, 0, 0);
        var date1 = new DateOnly(2025, 1, 6);
        var date2 = new DateOnly(2025, 1, 7);
        var activityId = Guid.NewGuid().ToString();

        var slots = new List<Slot>
        {
            new ActivitySlot
            {
                Id = Guid.NewGuid().ToString(),
                ActivityId = activityId,
                StartDateTime = date1.ToDateTime(new TimeOnly(10, 0)),
                EndDateTime = date2.ToDateTime(new TimeOnly(12, 0)),
                Status = SlotStatus.Reserved,
                Type = SlotType.Activity
            }
        };

        var result = _normalizer.Normalize(slots, opening, closing);

        Assert.Equal(2, result.Slots.Count);
        Assert.All(result.Slots.Cast<ActivitySlot>(), slot => 
        {
            Assert.Equal(activityId, slot.ActivityId);
            Assert.Equal(SlotStatus.Reserved, slot.Status);
            Assert.Equal(SlotType.Activity, slot.Type);
        });
    }

    #endregion

    #region Extreme Date Cases

    [Fact]
    public void Normalize_DateMinValue_ProcessesCorrectly()
    {
        var opening = new TimeSpan(9, 0, 0);
        var closing = new TimeSpan(18, 0, 0);
        var date = DateOnly.MinValue;

        var slots = new List<Slot>
        {
            CreateActivitySlot(date.ToDateTime(new TimeOnly(10, 0)), 
                date.ToDateTime(new TimeOnly(12, 0)))
        };

        var result = _normalizer.Normalize(slots, opening, closing);

        Assert.Single(result.Slots);
    }

    [Fact]
    public void Normalize_DateMaxValue_ProcessesCorrectly()
    {
        var opening = new TimeSpan(9, 0, 0);
        var closing = new TimeSpan(18, 0, 0);
        var date = DateOnly.MaxValue;

        var slots = new List<Slot>
        {
            CreateActivitySlot(date.ToDateTime(new TimeOnly(10, 0)), 
                date.ToDateTime(new TimeOnly(12, 0)))
        };

        var result = _normalizer.Normalize(slots, opening, closing);

        Assert.Single(result.Slots);
    }

    [Fact]
    public void Normalize_MultiDaySlotNearMaxValue_ThrowsOrHandlesGracefully()
    {
        var opening = new TimeSpan(9, 0, 0);
        var closing = new TimeSpan(18, 0, 0);
        var date = DateOnly.MaxValue;

        var slots = new List<Slot>
        {
            CreateActivitySlot(date.AddDays(-1).ToDateTime(new TimeOnly(10, 0)), 
                date.ToDateTime(new TimeOnly(12, 0)))
        };

        Assert.ThrowsAny<Exception>(() => _normalizer.Normalize(slots, opening, closing));
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

    #endregion
}