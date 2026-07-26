using RoomBooking.Api.Features.Chat.Tools;
using RoomBooking.Api.Shared.Time;
using RoomBooking.Api.Tests.Shared.Time;

namespace RoomBooking.Api.Tests.Features.Chat.Tools;

public sealed class ToolDateTimeTests
{
    private readonly BookingClock _clock = new(new FixedTimeProvider(
        new DateTimeOffset(2026, 7, 26, 15, 0, 0, TimeSpan.Zero)));
    private readonly ToolDateTimeNormalizer _normalizer;

    public ToolDateTimeTests()
    {
        _normalizer = new ToolDateTimeNormalizer(_clock);
    }

    private DateTimeOffset Local(int year, int month, int day, int hour, int minute) =>
        _clock.AtLocal(new DateOnly(year, month, day), new TimeOnly(hour, minute));

    [Fact]
    public void Parse_WithoutOffset_UsesMontevideo()
    {
        var parsed = _normalizer.Parse("2026-07-27T09:30:00");

        Assert.Equal(Local(2026, 7, 27, 9, 30), parsed);
    }

    [Fact]
    public void Parse_WithZ_ConvertsInstantToMontevideo()
    {
        var parsed = _normalizer.Parse("2026-07-27T12:30:00Z");

        Assert.Equal(Local(2026, 7, 27, 9, 30), parsed);
        Assert.Equal(TimeSpan.FromHours(-3), parsed.Offset);
    }

    [Fact]
    public void SnapRangeToFuture_AlreadyFuture_IsUnchanged()
    {
        var start = Local(2026, 7, 27, 10, 0);
        var end = Local(2026, 7, 27, 11, 0);

        var (actualStart, actualEnd, note) =
            _normalizer.SnapRangeToFuture(start, end);

        Assert.Equal(start, actualStart);
        Assert.Equal(end, actualEnd);
        Assert.Null(note);
    }

    [Fact]
    public void SnapRangeToFuture_PastDate_SuggestsTodayAtSameWallTime()
    {
        var start = Local(2023, 10, 5, 19, 0);
        var end = Local(2023, 10, 5, 20, 0);

        var (actualStart, actualEnd, note) =
            _normalizer.SnapRangeToFuture(start, end);

        Assert.Equal(Local(2026, 7, 26, 19, 0), actualStart);
        Assert.Equal(TimeSpan.FromHours(1), actualEnd - actualStart);
        Assert.NotNull(note);
    }

    [Fact]
    public void NormalizeAvailabilityRange_PastStart_ClampsToNextHalfHourSameDay()
    {
        var start = Local(2026, 7, 26, 8, 0);
        var end = Local(2026, 7, 26, 19, 0);

        var (actualStart, actualEnd, note, rejected) =
            _normalizer.NormalizeAvailabilityRange(start, end);

        Assert.False(rejected);
        Assert.Equal(Local(2026, 7, 26, 12, 0), actualStart);
        Assert.Equal(end, actualEnd);
        Assert.Contains("same day", note, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void NormalizeAvailabilityRange_EntirelyPast_IsRejected()
    {
        var start = Local(2026, 7, 26, 8, 0);
        var end = Local(2026, 7, 26, 11, 30);

        var (_, _, note, rejected) =
            _normalizer.NormalizeAvailabilityRange(start, end);

        Assert.True(rejected);
        Assert.Contains("entirely in the past", note);
    }

    [Fact]
    public void SnapPastCalendarToToday_PreservesWallClockRange()
    {
        var start = Local(2023, 10, 5, 8, 0);
        var end = Local(2023, 10, 5, 20, 0);

        var (actualStart, actualEnd, note) =
            _normalizer.SnapPastCalendarToToday(start, end);

        Assert.Equal(Local(2026, 7, 26, 8, 0), actualStart);
        Assert.Equal(Local(2026, 7, 26, 20, 0), actualEnd);
        Assert.NotNull(note);
    }
}
