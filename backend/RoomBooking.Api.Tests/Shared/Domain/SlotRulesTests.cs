using RoomBooking.Api.Shared.Domain;

namespace RoomBooking.Api.Tests.Shared.Domain;

public class SlotRulesTests
{
    private static readonly TimeZoneInfo Tz = TimeZoneInfo.CreateCustomTimeZone(
        "Test Montevideo", TimeSpan.FromHours(-3), "Test Montevideo", "Test Montevideo");

    private static DateTimeOffset Montevideo(int year, int month, int day, int hour, int minute)
    {
        var dt = new DateTime(year, month, day, hour, minute, 0);
        var offset = Tz.GetUtcOffset(dt);
        return new DateTimeOffset(dt, offset);
    }

    [Fact]
    public void IsAlignedToSlot_OnTheHour_ReturnsTrue()
    {
        var dt = Montevideo(2025, 7, 15, 10, 0);
        Assert.True(SlotRules.IsAlignedToSlot(dt, Tz));
    }

    [Fact]
    public void IsAlignedToSlot_OnHalfHour_ReturnsTrue()
    {
        var dt = Montevideo(2025, 7, 15, 10, 30);
        Assert.True(SlotRules.IsAlignedToSlot(dt, Tz));
    }

    [Fact]
    public void IsAlignedToSlot_At15Minutes_ReturnsFalse()
    {
        var dt = Montevideo(2025, 7, 15, 10, 15);
        Assert.False(SlotRules.IsAlignedToSlot(dt, Tz));
    }

    [Theory]
    [InlineData(8, 0, 9, 0, true)]
    [InlineData(19, 0, 20, 0, true)]
    [InlineData(7, 30, 8, 30, false)]
    [InlineData(19, 30, 20, 30, false)]
    [InlineData(8, 0, 20, 0, true)]
    public void IsWithinBusinessHours_VariousTimes(int sh, int sm, int eh, int em, bool expected)
    {
        var start = Montevideo(2025, 7, 15, sh, sm);
        var end = Montevideo(2025, 7, 15, eh, em);
        Assert.Equal(expected, SlotRules.IsWithinBusinessHours(start, end, Tz));
    }

    [Fact]
    public void IsWithinBusinessHours_DifferentDays_ReturnsFalse()
    {
        var start = Montevideo(2025, 7, 15, 19, 0);
        var end = Montevideo(2025, 7, 16, 8, 0);
        Assert.False(SlotRules.IsWithinBusinessHours(start, end, Tz));
    }

    [Theory]
    [InlineData(30, true)]
    [InlineData(60, true)]
    [InlineData(90, true)]
    [InlineData(120, true)]
    [InlineData(150, true)]
    [InlineData(180, true)]
    [InlineData(20, false)]
    [InlineData(45, false)]
    [InlineData(210, false)]
    public void IsValidDuration_VariousDurations(int minutes, bool expected)
    {
        var start = Montevideo(2025, 7, 15, 10, 0);
        var end = start.AddMinutes(minutes);
        Assert.Equal(expected, SlotRules.IsValidDuration(start, end));
    }

    [Fact]
    public void Overlaps_OverlappingRanges_ReturnsTrue()
    {
        var s1 = Montevideo(2025, 7, 15, 10, 0);
        var e1 = Montevideo(2025, 7, 15, 11, 0);
        var s2 = Montevideo(2025, 7, 15, 10, 30);
        var e2 = Montevideo(2025, 7, 15, 11, 30);

        Assert.True(SlotRules.Overlaps(s1, e1, s2, e2));
    }

    [Fact]
    public void Overlaps_TouchingRanges_ReturnsFalse()
    {
        var s1 = Montevideo(2025, 7, 15, 10, 0);
        var e1 = Montevideo(2025, 7, 15, 11, 0);
        var s2 = Montevideo(2025, 7, 15, 11, 0);
        var e2 = Montevideo(2025, 7, 15, 12, 0);

        Assert.False(SlotRules.Overlaps(s1, e1, s2, e2));
    }

    [Fact]
    public void Overlaps_NonOverlapping_ReturnsFalse()
    {
        var s1 = Montevideo(2025, 7, 15, 10, 0);
        var e1 = Montevideo(2025, 7, 15, 11, 0);
        var s2 = Montevideo(2025, 7, 15, 12, 0);
        var e2 = Montevideo(2025, 7, 15, 13, 0);

        Assert.False(SlotRules.Overlaps(s1, e1, s2, e2));
    }

    [Fact]
    public void GetFreeSlots_WithOneBooking_ReturnsTwoFreeSlots()
    {
        var dayStart = Montevideo(2025, 7, 15, 8, 0);
        var dayEnd = Montevideo(2025, 7, 15, 20, 0);
        var occupied = new List<(DateTimeOffset Start, DateTimeOffset End)>
        {
            (Montevideo(2025, 7, 15, 10, 0), Montevideo(2025, 7, 15, 11, 0))
        };

        var free = SlotRules.GetFreeSlots(dayStart, dayEnd, occupied);

        Assert.Equal(2, free.Count);
        Assert.Equal(dayStart, free[0].Start);
        Assert.Equal(Montevideo(2025, 7, 15, 10, 0), free[0].End);
        Assert.Equal(Montevideo(2025, 7, 15, 11, 0), free[1].Start);
        Assert.Equal(dayEnd, free[1].End);
    }

    [Fact]
    public void GetFreeSlots_NoBookings_ReturnsFullDay()
    {
        var dayStart = Montevideo(2025, 7, 15, 8, 0);
        var dayEnd = Montevideo(2025, 7, 15, 20, 0);
        var occupied = new List<(DateTimeOffset Start, DateTimeOffset End)>();

        var free = SlotRules.GetFreeSlots(dayStart, dayEnd, occupied);

        Assert.Single(free);
        Assert.Equal(dayStart, free[0].Start);
        Assert.Equal(dayEnd, free[0].End);
    }
}
