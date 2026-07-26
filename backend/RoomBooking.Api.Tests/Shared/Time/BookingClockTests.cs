using RoomBooking.Api.Shared.Domain;
using RoomBooking.Api.Shared.Time;

namespace RoomBooking.Api.Tests.Shared.Time;

public sealed class BookingClockTests
{
    private static readonly DateTimeOffset FixedUtcNow =
        new(2026, 7, 26, 15, 0, 0, TimeSpan.Zero);

    private readonly BookingClock _clock = new(new FixedTimeProvider(FixedUtcNow));

    [Fact]
    public void CurrentTime_UsesInjectedTimeProviderAndMontevideoZone()
    {
        Assert.Equal(new DateOnly(2026, 7, 26), _clock.TodayLocal);
        Assert.Equal(12, _clock.NowLocal.Hour);
        Assert.Equal(TimeSpan.FromHours(-3), _clock.NowLocal.Offset);
    }

    [Fact]
    public void ToLocalAndFormatLocal_ConvertTheSameInstant()
    {
        var utc = new DateTimeOffset(2026, 7, 21, 12, 30, 0, TimeSpan.Zero);

        var local = _clock.ToLocal(utc);

        Assert.Equal(TimeSpan.FromHours(-3), local.Offset);
        Assert.Equal(9, local.Hour);
        Assert.Equal("2026-07-21 09:30", _clock.FormatLocal(utc));
    }

    [Fact]
    public void ExpandScheduleRange_UsesHalfOpenBusinessDateBounds()
    {
        var (from, to) = _clock.ExpandScheduleRange(
            new DateOnly(2026, 7, 26),
            new DateOnly(2026, 7, 28));

        Assert.Equal(SlotRules.DayStart, TimeOnly.FromDateTime(from.DateTime));
        Assert.Equal(new DateOnly(2026, 7, 26), DateOnly.FromDateTime(from.DateTime));
        Assert.Equal(SlotRules.DayStart, TimeOnly.FromDateTime(to.DateTime));
        Assert.Equal(new DateOnly(2026, 7, 28), DateOnly.FromDateTime(to.DateTime));
        Assert.Equal(TimeSpan.FromHours(-3), from.Offset);
        Assert.Equal(TimeSpan.FromHours(-3), to.Offset);
    }
}
