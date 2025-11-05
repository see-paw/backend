using Application.Interfaces;
using Application.Services;
using Domain;
using Domain.Enums;

namespace Tests.Services;

/// <summary>
/// Tests for TimeRangeCalculator using equivalence class partitioning and boundary value analysis.
/// Focuses on finding defects in gap calculation, slot filtering, and edge cases.
/// </summary>
public class TimeRangeCalculatorTests
{
    private readonly ITimeRangeCalculator _calculator = new TimeRangeCalculator();

    #region Empty Slots - Equivalence Classes

    [Fact]
    public void CalculateWeeklyAvailableRanges_NoSlots_ReturnsFullWeek()
    {
        var weekStart = new DateOnly(2025, 1, 6);
        var opening = new TimeSpan(9, 0, 0);
        var closing = new TimeSpan(18, 0, 0);

        var result = _calculator.CalculateWeeklyAvailableRanges(
            Enumerable.Empty<Slot>(), opening, closing, weekStart);

        Assert.Equal(7, result.Count);
        Assert.All(result, block =>
        {
            Assert.Equal(opening, block.Start);
            Assert.Equal(closing, block.End);
        });
    }

    [Fact]
    public void CalculateWeeklyAvailableRanges_OnlyAvailableSlots_ReturnsFullWeek()
    {
        var weekStart = new DateOnly(2025, 1, 6);
        var opening = new TimeSpan(9, 0, 0);
        var closing = new TimeSpan(18, 0, 0);

        var slots = new List<Slot>
        {
            CreateActivitySlot(weekStart.ToDateTime(TimeOnly.MinValue).AddHours(10), 
                weekStart.ToDateTime(TimeOnly.MinValue).AddHours(12), SlotStatus.Available)
        };

        var result = _calculator.CalculateWeeklyAvailableRanges(slots, opening, closing, weekStart);

        Assert.Equal(7, result.Count);
    }

    [Fact]
    public void CalculateWeeklyAvailableRanges_NullSlots_ThrowsOrHandlesGracefully()
    {
        var weekStart = new DateOnly(2025, 1, 6);
        var opening = new TimeSpan(9, 0, 0);
        var closing = new TimeSpan(18, 0, 0);

        Assert.ThrowsAny<Exception>(() => 
            _calculator.CalculateWeeklyAvailableRanges(null!, opening, closing, weekStart));
    }

    #endregion

    #region Opening/Closing Hours - Boundary Value Analysis

    [Theory]
    [InlineData(0, 0, 0, 0, 0, 1)]
    [InlineData(23, 59, 59, 23, 59, 59)]
    [InlineData(0, 0, 0, 23, 59, 59)]
    public void CalculateWeeklyAvailableRanges_BoundaryHours_ProcessesCorrectly(
        int openHour, int openMin, int openSec, int closeHour, int closeMin, int closeSec)
    {
        var weekStart = new DateOnly(2025, 1, 6);
        var opening = new TimeSpan(openHour, openMin, openSec);
        var closing = new TimeSpan(closeHour, closeMin, closeSec);

        var result = _calculator.CalculateWeeklyAvailableRanges(
            Enumerable.Empty<Slot>(), opening, closing, weekStart);

        Assert.Equal(7, result.Count);
        Assert.All(result, block =>
        {
            Assert.Equal(opening, block.Start);
            Assert.Equal(closing, block.End);
        });
    }

    [Fact]
    public void CalculateWeeklyAvailableRanges_OpeningEqualsClosing_ReturnsEmptyBlocks()
    {
        var weekStart = new DateOnly(2025, 1, 6);
        var opening = new TimeSpan(9, 0, 0);
        var closing = new TimeSpan(9, 0, 0);

        var result = _calculator.CalculateWeeklyAvailableRanges(
            Enumerable.Empty<Slot>(), opening, closing, weekStart);

        Assert.All(result, block => Assert.Equal(block.Start, block.End));
    }

    [Fact]
    public void CalculateWeeklyAvailableRanges_OpeningAfterClosing_ProducesInvalidBlocks()
    {
        var weekStart = new DateOnly(2025, 1, 6);
        var opening = new TimeSpan(18, 0, 0);
        var closing = new TimeSpan(9, 0, 0);

        var result = _calculator.CalculateWeeklyAvailableRanges(
            Enumerable.Empty<Slot>(), opening, closing, weekStart);

        Assert.All(result, block => Assert.True(block.Start >= block.End));
    }

    #endregion

    #region Week Start Date - Boundary Value Analysis

    [Fact]
    public void CalculateWeeklyAvailableRanges_WeekStartMinValue_ProcessesCorrectly()
    {
        var weekStart = DateOnly.MinValue;
        var opening = new TimeSpan(9, 0, 0);
        var closing = new TimeSpan(18, 0, 0);

        var result = _calculator.CalculateWeeklyAvailableRanges(
            Enumerable.Empty<Slot>(), opening, closing, weekStart);

        Assert.Equal(7, result.Count);
        Assert.Equal(weekStart, result[0].Date);
        Assert.Equal(weekStart.AddDays(6), result[6].Date);
    }

    [Fact]
    public void CalculateWeeklyAvailableRanges_WeekStartMaxValue_ThrowsOrOverflows()
    {
        var weekStart = DateOnly.MaxValue;
        var opening = new TimeSpan(9, 0, 0);
        var closing = new TimeSpan(18, 0, 0);

        Assert.ThrowsAny<Exception>(() => 
            _calculator.CalculateWeeklyAvailableRanges(
                Enumerable.Empty<Slot>(), opening, closing, weekStart));
    }

    [Fact]
    public void CalculateWeeklyAvailableRanges_WeekStartNearMaxValue_ThrowsOrOverflows()
    {
        var weekStart = DateOnly.MaxValue.AddDays(-6);
        var opening = new TimeSpan(9, 0, 0);
        var closing = new TimeSpan(18, 0, 0);

        Assert.ThrowsAny<Exception>(() => 
            _calculator.CalculateWeeklyAvailableRanges(
                Enumerable.Empty<Slot>(), opening, closing, weekStart));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(1)]
    public void CalculateWeeklyAvailableRanges_WeekStartVariousDates_ProcessesCorrectly(int daysOffset)
    {
        var weekStart = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(daysOffset);
        var opening = new TimeSpan(9, 0, 0);
        var closing = new TimeSpan(18, 0, 0);

        var result = _calculator.CalculateWeeklyAvailableRanges(
            Enumerable.Empty<Slot>(), opening, closing, weekStart);

        Assert.Equal(7, result.Count);
    }

    #endregion

    #region Single Slot Positioning - Equivalence Classes

    [Fact]
    public void CalculateWeeklyAvailableRanges_SlotAtDayStart_CreatesGapAfter()
    {
        var weekStart = new DateOnly(2025, 1, 6);
        var opening = new TimeSpan(9, 0, 0);
        var closing = new TimeSpan(18, 0, 0);

        var slots = new List<Slot>
        {
            CreateActivitySlot(weekStart.ToDateTime(new TimeOnly(9, 0)), 
                weekStart.ToDateTime(new TimeOnly(12, 0)), SlotStatus.Reserved)
        };

        var result = _calculator.CalculateWeeklyAvailableRanges(slots, opening, closing, weekStart);

        var dayBlocks = result.Where(b => b.Date == weekStart).ToList();
        Assert.Single(dayBlocks);
        Assert.Equal(new TimeSpan(12, 0, 0), dayBlocks[0].Start);
        Assert.Equal(closing, dayBlocks[0].End);
    }

    [Fact]
    public void CalculateWeeklyAvailableRanges_SlotAtDayEnd_CreatesGapBefore()
    {
        var weekStart = new DateOnly(2025, 1, 6);
        var opening = new TimeSpan(9, 0, 0);
        var closing = new TimeSpan(18, 0, 0);

        var slots = new List<Slot>
        {
            CreateActivitySlot(weekStart.ToDateTime(new TimeOnly(15, 0)), 
                weekStart.ToDateTime(new TimeOnly(18, 0)), SlotStatus.Reserved)
        };

        var result = _calculator.CalculateWeeklyAvailableRanges(slots, opening, closing, weekStart);

        var dayBlocks = result.Where(b => b.Date == weekStart).ToList();
        Assert.Single(dayBlocks);
        Assert.Equal(opening, dayBlocks[0].Start);
        Assert.Equal(new TimeSpan(15, 0, 0), dayBlocks[0].End);
    }

    [Fact]
    public void CalculateWeeklyAvailableRanges_SlotInMiddle_CreatesTwoGaps()
    {
        var weekStart = new DateOnly(2025, 1, 6);
        var opening = new TimeSpan(9, 0, 0);
        var closing = new TimeSpan(18, 0, 0);

        var slots = new List<Slot>
        {
            CreateActivitySlot(weekStart.ToDateTime(new TimeOnly(12, 0)), 
                weekStart.ToDateTime(new TimeOnly(14, 0)), SlotStatus.Reserved)
        };

        var result = _calculator.CalculateWeeklyAvailableRanges(slots, opening, closing, weekStart);

        var dayBlocks = result.Where(b => b.Date == weekStart).ToList();
        Assert.Equal(2, dayBlocks.Count);
        Assert.Equal(opening, dayBlocks[0].Start);
        Assert.Equal(new TimeSpan(12, 0, 0), dayBlocks[0].End);
        Assert.Equal(new TimeSpan(14, 0, 0), dayBlocks[1].Start);
        Assert.Equal(closing, dayBlocks[1].End);
    }

    [Fact]
    public void CalculateWeeklyAvailableRanges_SlotCoversEntireDay_NoGaps()
    {
        var weekStart = new DateOnly(2025, 1, 6);
        var opening = new TimeSpan(9, 0, 0);
        var closing = new TimeSpan(18, 0, 0);

        var slots = new List<Slot>
        {
            CreateActivitySlot(weekStart.ToDateTime(new TimeOnly(9, 0)), 
                weekStart.ToDateTime(new TimeOnly(18, 0)), SlotStatus.Reserved)
        };

        var result = _calculator.CalculateWeeklyAvailableRanges(slots, opening, closing, weekStart);

        var dayBlocks = result.Where(b => b.Date == weekStart).ToList();
        Assert.Empty(dayBlocks);
    }

    #endregion

    #region Slot Boundary Alignment - Boundary Value Analysis

    [Fact]
    public void CalculateWeeklyAvailableRanges_SlotStartsExactlyAtOpening_NoGapBefore()
    {
        var weekStart = new DateOnly(2025, 1, 6);
        var opening = new TimeSpan(9, 0, 0);
        var closing = new TimeSpan(18, 0, 0);

        var slots = new List<Slot>
        {
            CreateActivitySlot(weekStart.ToDateTime(new TimeOnly(9, 0, 0)), 
                weekStart.ToDateTime(new TimeOnly(12, 0)), SlotStatus.Reserved)
        };

        var result = _calculator.CalculateWeeklyAvailableRanges(slots, opening, closing, weekStart);

        var dayBlocks = result.Where(b => b.Date == weekStart).ToList();
        Assert.Single(dayBlocks);
        Assert.Equal(new TimeSpan(12, 0, 0), dayBlocks[0].Start);
    }

    [Fact]
    public void CalculateWeeklyAvailableRanges_SlotEndsExactlyAtClosing_NoGapAfter()
    {
        var weekStart = new DateOnly(2025, 1, 6);
        var opening = new TimeSpan(9, 0, 0);
        var closing = new TimeSpan(18, 0, 0);

        var slots = new List<Slot>
        {
            CreateActivitySlot(weekStart.ToDateTime(new TimeOnly(15, 0)), 
                weekStart.ToDateTime(new TimeOnly(18, 0, 0)), SlotStatus.Reserved)
        };

        var result = _calculator.CalculateWeeklyAvailableRanges(slots, opening, closing, weekStart);

        var dayBlocks = result.Where(b => b.Date == weekStart).ToList();
        Assert.Single(dayBlocks);
        Assert.Equal(opening, dayBlocks[0].Start);
    }

    [Fact]
    public void CalculateWeeklyAvailableRanges_SlotStartsOneTickBeforeOpening_HandlesCorrectly()
    {
        var weekStart = new DateOnly(2025, 1, 6);
        var opening = new TimeSpan(9, 0, 0);
        var closing = new TimeSpan(18, 0, 0);

        var slots = new List<Slot>
        {
            CreateActivitySlot(weekStart.ToDateTime(new TimeOnly(8, 59, 59)), 
                weekStart.ToDateTime(new TimeOnly(12, 0)), SlotStatus.Reserved)
        };

        var result = _calculator.CalculateWeeklyAvailableRanges(slots, opening, closing, weekStart);

        var dayBlocks = result.Where(b => b.Date == weekStart).ToList();
        Assert.Single(dayBlocks);
    }

    [Fact]
    public void CalculateWeeklyAvailableRanges_SlotEndsOneTickAfterClosing_HandlesCorrectly()
    {
        var weekStart = new DateOnly(2025, 1, 6);
        var opening = new TimeSpan(9, 0, 0);
        var closing = new TimeSpan(18, 0, 0);

        var slots = new List<Slot>
        {
            CreateActivitySlot(weekStart.ToDateTime(new TimeOnly(15, 0)), 
                weekStart.ToDateTime(new TimeOnly(18, 0, 1)), SlotStatus.Reserved)
        };

        var result = _calculator.CalculateWeeklyAvailableRanges(slots, opening, closing, weekStart);

        var dayBlocks = result.Where(b => b.Date == weekStart).ToList();
        Assert.Single(dayBlocks);
    }

    [Fact]
    public void CalculateWeeklyAvailableRanges_SlotStartsOneTickAfterOpening_CreatesSmallGap()
    {
        var weekStart = new DateOnly(2025, 1, 6);
        var opening = new TimeSpan(9, 0, 0);
        var closing = new TimeSpan(18, 0, 0);

        var slots = new List<Slot>
        {
            CreateActivitySlot(weekStart.ToDateTime(new TimeOnly(9, 0, 0)).AddTicks(1), 
                weekStart.ToDateTime(new TimeOnly(12, 0)), SlotStatus.Reserved)
        };

        var result = _calculator.CalculateWeeklyAvailableRanges(slots, opening, closing, weekStart);

        var dayBlocks = result.Where(b => b.Date == weekStart).ToList();
        Assert.Equal(2, dayBlocks.Count);
        Assert.True(dayBlocks[0].End - dayBlocks[0].Start == TimeSpan.FromTicks(1));
    }

    [Fact]
    public void CalculateWeeklyAvailableRanges_SlotEndsOneTickBeforeClosing_CreatesSmallGap()
    {
        var weekStart = new DateOnly(2025, 1, 6);
        var opening = new TimeSpan(9, 0, 0);
        var closing = new TimeSpan(18, 0, 0);

        var slots = new List<Slot>
        {
            CreateActivitySlot(weekStart.ToDateTime(new TimeOnly(15, 0)), 
                weekStart.ToDateTime(new TimeOnly(18, 0, 0)).AddTicks(-1), SlotStatus.Reserved)
        };

        var result = _calculator.CalculateWeeklyAvailableRanges(slots, opening, closing, weekStart);

        var dayBlocks = result.Where(b => b.Date == weekStart).ToList();
        Assert.Equal(2, dayBlocks.Count);
        Assert.True(dayBlocks[1].End - dayBlocks[1].Start == TimeSpan.FromTicks(1));
    }

    #endregion

    #region Slot Outside Operating Hours - Equivalence Classes

    [Fact]
    public void CalculateWeeklyAvailableRanges_SlotCompletelyBeforeOpening_IgnoresSlot()
    {
        var weekStart = new DateOnly(2025, 1, 6);
        var opening = new TimeSpan(9, 0, 0);
        var closing = new TimeSpan(18, 0, 0);

        var slots = new List<Slot>
        {
            CreateActivitySlot(weekStart.ToDateTime(new TimeOnly(6, 0)), 
                weekStart.ToDateTime(new TimeOnly(8, 0)), SlotStatus.Reserved)
        };

        var result = _calculator.CalculateWeeklyAvailableRanges(slots, opening, closing, weekStart);

        var dayBlocks = result.Where(b => b.Date == weekStart).ToList();
        Assert.Single(dayBlocks);
        Assert.Equal(opening, dayBlocks[0].Start);
        Assert.Equal(closing, dayBlocks[0].End);
    }

    [Fact]
    public void CalculateWeeklyAvailableRanges_SlotCompletelyAfterClosing_IgnoresSlot()
    {
        var weekStart = new DateOnly(2025, 1, 6);
        var opening = new TimeSpan(9, 0, 0);
        var closing = new TimeSpan(18, 0, 0);

        var slots = new List<Slot>
        {
            CreateActivitySlot(weekStart.ToDateTime(new TimeOnly(19, 0)), 
                weekStart.ToDateTime(new TimeOnly(21, 0)), SlotStatus.Reserved)
        };

        var result = _calculator.CalculateWeeklyAvailableRanges(slots, opening, closing, weekStart);

        var dayBlocks = result.Where(b => b.Date == weekStart).ToList();
        Assert.Single(dayBlocks);
        Assert.Equal(opening, dayBlocks[0].Start);
        Assert.Equal(closing, dayBlocks[0].End);
    }

    [Fact]
    public void CalculateWeeklyAvailableRanges_SlotPartiallyBeforeOpening_ConsidersOverlap()
    {
        var weekStart = new DateOnly(2025, 1, 6);
        var opening = new TimeSpan(9, 0, 0);
        var closing = new TimeSpan(18, 0, 0);

        var slots = new List<Slot>
        {
            CreateActivitySlot(weekStart.ToDateTime(new TimeOnly(8, 0)), 
                weekStart.ToDateTime(new TimeOnly(10, 0)), SlotStatus.Reserved)
        };

        var result = _calculator.CalculateWeeklyAvailableRanges(slots, opening, closing, weekStart);

        var dayBlocks = result.Where(b => b.Date == weekStart).ToList();
        Assert.Single(dayBlocks);
    }

    [Fact]
    public void CalculateWeeklyAvailableRanges_SlotPartiallyAfterClosing_ConsidersOverlap()
    {
        var weekStart = new DateOnly(2025, 1, 6);
        var opening = new TimeSpan(9, 0, 0);
        var closing = new TimeSpan(18, 0, 0);

        var slots = new List<Slot>
        {
            CreateActivitySlot(weekStart.ToDateTime(new TimeOnly(17, 0)), 
                weekStart.ToDateTime(new TimeOnly(19, 0)), SlotStatus.Reserved)
        };

        var result = _calculator.CalculateWeeklyAvailableRanges(slots, opening, closing, weekStart);

        var dayBlocks = result.Where(b => b.Date == weekStart).ToList();
        Assert.Single(dayBlocks);
    }

    #endregion

    #region Multiple Slots Same Day - Equivalence Classes

    [Fact]
    public void CalculateWeeklyAvailableRanges_TwoSeparateSlots_CreatesThreeGaps()
    {
        var weekStart = new DateOnly(2025, 1, 6);
        var opening = new TimeSpan(9, 0, 0);
        var closing = new TimeSpan(18, 0, 0);

        var slots = new List<Slot>
        {
            CreateActivitySlot(weekStart.ToDateTime(new TimeOnly(10, 0)), 
                weekStart.ToDateTime(new TimeOnly(11, 0)), SlotStatus.Reserved),
            CreateActivitySlot(weekStart.ToDateTime(new TimeOnly(14, 0)), 
                weekStart.ToDateTime(new TimeOnly(15, 0)), SlotStatus.Reserved)
        };

        var result = _calculator.CalculateWeeklyAvailableRanges(slots, opening, closing, weekStart);

        var dayBlocks = result.Where(b => b.Date == weekStart).ToList();
        Assert.Equal(3, dayBlocks.Count);
    }

    [Fact]
    public void CalculateWeeklyAvailableRanges_TwoAdjacentSlots_CreatesTwoGaps()
    {
        var weekStart = new DateOnly(2025, 1, 6);
        var opening = new TimeSpan(9, 0, 0);
        var closing = new TimeSpan(18, 0, 0);

        var slots = new List<Slot>
        {
            CreateActivitySlot(weekStart.ToDateTime(new TimeOnly(10, 0)), 
                weekStart.ToDateTime(new TimeOnly(12, 0)), SlotStatus.Reserved),
            CreateActivitySlot(weekStart.ToDateTime(new TimeOnly(12, 0)), 
                weekStart.ToDateTime(new TimeOnly(14, 0)), SlotStatus.Reserved)
        };

        var result = _calculator.CalculateWeeklyAvailableRanges(slots, opening, closing, weekStart);

        var dayBlocks = result.Where(b => b.Date == weekStart).ToList();
        Assert.Equal(2, dayBlocks.Count);
    }

    [Fact]
    public void CalculateWeeklyAvailableRanges_TwoOverlappingSlots_MergesOverlap()
    {
        var weekStart = new DateOnly(2025, 1, 6);
        var opening = new TimeSpan(9, 0, 0);
        var closing = new TimeSpan(18, 0, 0);

        var slots = new List<Slot>
        {
            CreateActivitySlot(weekStart.ToDateTime(new TimeOnly(10, 0)), 
                weekStart.ToDateTime(new TimeOnly(13, 0)), SlotStatus.Reserved),
            CreateActivitySlot(weekStart.ToDateTime(new TimeOnly(12, 0)), 
                weekStart.ToDateTime(new TimeOnly(15, 0)), SlotStatus.Reserved)
        };

        var result = _calculator.CalculateWeeklyAvailableRanges(slots, opening, closing, weekStart);

        var dayBlocks = result.Where(b => b.Date == weekStart).ToList();
        Assert.Equal(2, dayBlocks.Count);
    }

    [Fact]
    public void CalculateWeeklyAvailableRanges_SlotCompletelyInsideAnother_HandlesCorrectly()
    {
        var weekStart = new DateOnly(2025, 1, 6);
        var opening = new TimeSpan(9, 0, 0);
        var closing = new TimeSpan(18, 0, 0);

        var slots = new List<Slot>
        {
            CreateActivitySlot(weekStart.ToDateTime(new TimeOnly(10, 0)), 
                weekStart.ToDateTime(new TimeOnly(16, 0)), SlotStatus.Reserved),
            CreateActivitySlot(weekStart.ToDateTime(new TimeOnly(12, 0)), 
                weekStart.ToDateTime(new TimeOnly(14, 0)), SlotStatus.Reserved)
        };

        var result = _calculator.CalculateWeeklyAvailableRanges(slots, opening, closing, weekStart);

        var dayBlocks = result.Where(b => b.Date == weekStart).ToList();
        Assert.Equal(2, dayBlocks.Count);
    }

    [Fact]
    public void CalculateWeeklyAvailableRanges_UnsortedSlots_HandlesCorrectly()
    {
        var weekStart = new DateOnly(2025, 1, 6);
        var opening = new TimeSpan(9, 0, 0);
        var closing = new TimeSpan(18, 0, 0);

        var slots = new List<Slot>
        {
            CreateActivitySlot(weekStart.ToDateTime(new TimeOnly(14, 0)), 
                weekStart.ToDateTime(new TimeOnly(15, 0)), SlotStatus.Reserved),
            CreateActivitySlot(weekStart.ToDateTime(new TimeOnly(10, 0)), 
                weekStart.ToDateTime(new TimeOnly(11, 0)), SlotStatus.Reserved)
        };

        var result = _calculator.CalculateWeeklyAvailableRanges(slots, opening, closing, weekStart);

        var dayBlocks = result.Where(b => b.Date == weekStart).ToList();
        Assert.Equal(3, dayBlocks.Count);
        Assert.True(dayBlocks[0].Start < dayBlocks[1].Start);
        Assert.True(dayBlocks[1].Start < dayBlocks[2].Start);
    }

    #endregion

    #region Multiple Days - Equivalence Classes

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(6)]
    public void CalculateWeeklyAvailableRanges_SlotOnSpecificDay_OtherDaysFull(int dayOffset)
    {
        var weekStart = new DateOnly(2025, 1, 6);
        var opening = new TimeSpan(9, 0, 0);
        var closing = new TimeSpan(18, 0, 0);
        var targetDay = weekStart.AddDays(dayOffset);

        var slots = new List<Slot>
        {
            CreateActivitySlot(targetDay.ToDateTime(new TimeOnly(10, 0)), 
                targetDay.ToDateTime(new TimeOnly(12, 0)), SlotStatus.Reserved)
        };

        var result = _calculator.CalculateWeeklyAvailableRanges(slots, opening, closing, weekStart);

        Assert.Equal(8, result.Count);
        var fullDays = result.Where(b => b.Date != targetDay).ToList();
        Assert.Equal(6, fullDays.Count);
        Assert.All(fullDays, block =>
        {
            Assert.Equal(opening, block.Start);
            Assert.Equal(closing, block.End);
        });
    }

    [Fact]
    public void CalculateWeeklyAvailableRanges_SlotsOnAllDays_ProcessesAllCorrectly()
    {
        var weekStart = new DateOnly(2025, 1, 6);
        var opening = new TimeSpan(9, 0, 0);
        var closing = new TimeSpan(18, 0, 0);

        var slots = new List<Slot>();
        for (int i = 0; i < 7; i++)
        {
            var day = weekStart.AddDays(i);
            slots.Add(CreateActivitySlot(day.ToDateTime(new TimeOnly(12, 0)), 
                day.ToDateTime(new TimeOnly(14, 0)), SlotStatus.Reserved));
        }

        var result = _calculator.CalculateWeeklyAvailableRanges(slots, opening, closing, weekStart);

        Assert.Equal(14, result.Count);
    }

    [Fact]
    public void CalculateWeeklyAvailableRanges_AllDaysFullyOccupied_ReturnsEmpty()
    {
        var weekStart = new DateOnly(2025, 1, 6);
        var opening = new TimeSpan(9, 0, 0);
        var closing = new TimeSpan(18, 0, 0);

        var slots = new List<Slot>();
        for (int i = 0; i < 7; i++)
        {
            var day = weekStart.AddDays(i);
            slots.Add(CreateActivitySlot(day.ToDateTime(new TimeOnly(9, 0)), 
                day.ToDateTime(new TimeOnly(18, 0)), SlotStatus.Reserved));
        }

        var result = _calculator.CalculateWeeklyAvailableRanges(slots, opening, closing, weekStart);

        Assert.Empty(result);
    }

    #endregion

    #region Slot Status Filtering - Equivalence Classes

    [Theory]
    [InlineData(SlotStatus.Reserved)]
    [InlineData(SlotStatus.Unavailable)]
    public void CalculateWeeklyAvailableRanges_NonAvailableStatus_IsConsidered(SlotStatus status)
    {
        var weekStart = new DateOnly(2025, 1, 6);
        var opening = new TimeSpan(9, 0, 0);
        var closing = new TimeSpan(18, 0, 0);

        var slots = new List<Slot>
        {
            CreateActivitySlot(weekStart.ToDateTime(new TimeOnly(12, 0)), 
                weekStart.ToDateTime(new TimeOnly(14, 0)), status)
        };

        var result = _calculator.CalculateWeeklyAvailableRanges(slots, opening, closing, weekStart);

        var dayBlocks = result.Where(b => b.Date == weekStart).ToList();
        Assert.Equal(2, dayBlocks.Count);
    }

    [Fact]
    public void CalculateWeeklyAvailableRanges_MixedStatuses_FiltersCorrectly()
    {
        var weekStart = new DateOnly(2025, 1, 6);
        var opening = new TimeSpan(9, 0, 0);
        var closing = new TimeSpan(18, 0, 0);

        var slots = new List<Slot>
        {
            CreateActivitySlot(weekStart.ToDateTime(new TimeOnly(10, 0)), 
                weekStart.ToDateTime(new TimeOnly(11, 0)), SlotStatus.Available),
            CreateActivitySlot(weekStart.ToDateTime(new TimeOnly(12, 0)), 
                weekStart.ToDateTime(new TimeOnly(13, 0)), SlotStatus.Reserved),
            CreateActivitySlot(weekStart.ToDateTime(new TimeOnly(14, 0)), 
                weekStart.ToDateTime(new TimeOnly(15, 0)), SlotStatus.Unavailable)
        };

        var result = _calculator.CalculateWeeklyAvailableRanges(slots, opening, closing, weekStart);

        var dayBlocks = result.Where(b => b.Date == weekStart).ToList();
        Assert.Equal(3, dayBlocks.Count);
    }

    #endregion

    #region Result Ordering - Equivalence Classes

    [Fact]
    public void CalculateWeeklyAvailableRanges_ResultsOrderedByDateThenTime()
    {
        var weekStart = new DateOnly(2025, 1, 6);
        var opening = new TimeSpan(9, 0, 0);
        var closing = new TimeSpan(18, 0, 0);

        var slots = new List<Slot>
        {
            CreateActivitySlot(weekStart.AddDays(2).ToDateTime(new TimeOnly(12, 0)), 
                weekStart.AddDays(2).ToDateTime(new TimeOnly(13, 0)), SlotStatus.Reserved),
            CreateActivitySlot(weekStart.AddDays(0).ToDateTime(new TimeOnly(14, 0)), 
                weekStart.AddDays(0).ToDateTime(new TimeOnly(15, 0)), SlotStatus.Reserved),
            CreateActivitySlot(weekStart.AddDays(0).ToDateTime(new TimeOnly(10, 0)), 
                weekStart.AddDays(0).ToDateTime(new TimeOnly(11, 0)), SlotStatus.Reserved)
        };

        var result = _calculator.CalculateWeeklyAvailableRanges(slots, opening, closing, weekStart);

        for (int i = 0; i < result.Count - 1; i++)
        {
            var current = result[i];
            var next = result[i + 1];

            if (current.Date == next.Date)
            {
                Assert.True(current.Start <= next.Start);
            }
            else
            {
                Assert.True(current.Date < next.Date);
            }
        }
    }

    #endregion

    #region Edge Cases - Zero Duration Slots

    [Fact]
    public void CalculateWeeklyAvailableRanges_ZeroDurationSlot_HandlesCorrectly()
    {
        var weekStart = new DateOnly(2025, 1, 6);
        var opening = new TimeSpan(9, 0, 0);
        var closing = new TimeSpan(18, 0, 0);

        var slotTime = weekStart.ToDateTime(new TimeOnly(12, 0));
        var slots = new List<Slot>
        {
            CreateActivitySlot(slotTime, slotTime, SlotStatus.Reserved)
        };

        var result = _calculator.CalculateWeeklyAvailableRanges(slots, opening, closing, weekStart);

        var dayBlocks = result.Where(b => b.Date == weekStart).ToList();
        Assert.True(dayBlocks.Count >= 1);
    }

    [Fact]
    public void CalculateWeeklyAvailableRanges_NegativeDurationSlot_HandlesOrThrows()
    {
        var weekStart = new DateOnly(2025, 1, 6);
        var opening = new TimeSpan(9, 0, 0);
        var closing = new TimeSpan(18, 0, 0);

        var slots = new List<Slot>
        {
            CreateActivitySlot(weekStart.ToDateTime(new TimeOnly(14, 0)), 
                weekStart.ToDateTime(new TimeOnly(12, 0)), SlotStatus.Reserved)
        };

        var result = _calculator.CalculateWeeklyAvailableRanges(slots, opening, closing, weekStart);

        Assert.NotNull(result);
    }

    #endregion

    #region Slots Outside Week Range - Equivalence Classes

    [Fact]
    public void CalculateWeeklyAvailableRanges_SlotBeforeWeekStart_IsIgnored()
    {
        var weekStart = new DateOnly(2025, 1, 6);
        var opening = new TimeSpan(9, 0, 0);
        var closing = new TimeSpan(18, 0, 0);

        var slots = new List<Slot>
        {
            CreateActivitySlot(weekStart.AddDays(-1).ToDateTime(new TimeOnly(10, 0)), 
                weekStart.AddDays(-1).ToDateTime(new TimeOnly(12, 0)), SlotStatus.Reserved)
        };

        var result = _calculator.CalculateWeeklyAvailableRanges(slots, opening, closing, weekStart);

        Assert.All(result, block => Assert.True(block.Date >= weekStart));
        Assert.Equal(7, result.Count);
    }

    [Fact]
    public void CalculateWeeklyAvailableRanges_SlotAfterWeekEnd_IsIgnored()
    {
        var weekStart = new DateOnly(2025, 1, 6);
        var opening = new TimeSpan(9, 0, 0);
        var closing = new TimeSpan(18, 0, 0);

        var slots = new List<Slot>
        {
            CreateActivitySlot(weekStart.AddDays(7).ToDateTime(new TimeOnly(10, 0)), 
                weekStart.AddDays(7).ToDateTime(new TimeOnly(12, 0)), SlotStatus.Reserved)
        };

        var result = _calculator.CalculateWeeklyAvailableRanges(slots, opening, closing, weekStart);

        Assert.All(result, block => Assert.True(block.Date < weekStart.AddDays(7)));
        Assert.Equal(7, result.Count);
    }

    #endregion

    #region Helper Methods

    private static ActivitySlot CreateActivitySlot(DateTime start, DateTime end, SlotStatus status)
    {
        return new ActivitySlot
        {
            Id = Guid.NewGuid().ToString(),
            ActivityId = Guid.NewGuid().ToString(),
            StartDateTime = start,
            EndDateTime = end,
            Status = status,
            Type = SlotType.Activity
        };
    }

    #endregion
}