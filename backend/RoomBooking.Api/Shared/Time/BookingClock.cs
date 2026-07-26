using System.Globalization;
using RoomBooking.Api.Shared.Domain;

namespace RoomBooking.Api.Shared.Time;

public sealed class BookingClock(TimeProvider timeProvider) : IBookingClock
{
    private static readonly string[] TimeZoneIds =
        ["America/Montevideo", "Montevideo Standard Time"];

    public TimeZoneInfo TimeZone { get; } = ResolveTimeZone();

    public DateTimeOffset UtcNow => timeProvider.GetUtcNow();

    public DateTimeOffset NowLocal => ToLocal(UtcNow);

    public DateOnly TodayLocal => DateOnly.FromDateTime(NowLocal.DateTime);

    public DateTimeOffset ToLocal(DateTimeOffset instant) =>
        TimeZoneInfo.ConvertTime(instant, TimeZone);

    public DateTimeOffset AtLocal(DateOnly date, TimeOnly time)
    {
        var local = DateTime.SpecifyKind(
            date.ToDateTime(time),
            DateTimeKind.Unspecified);
        return new DateTimeOffset(local, TimeZone.GetUtcOffset(local));
    }

    public string FormatLocal(DateTimeOffset instant, string format = "yyyy-MM-dd HH:mm") =>
        ToLocal(instant).ToString(format, CultureInfo.InvariantCulture);

    public (DateTimeOffset From, DateTimeOffset To) ExpandScheduleRange(
        DateOnly fromDate,
        DateOnly toDateExclusive) =>
        (AtLocal(fromDate, SlotRules.DayStart), AtLocal(toDateExclusive, SlotRules.DayStart));

    private static TimeZoneInfo ResolveTimeZone()
    {
        foreach (var id in TimeZoneIds)
        {
            if (TimeZoneInfo.TryFindSystemTimeZoneById(id, out var tz))
                return tz;
        }

        throw new InvalidOperationException(
            "Cannot find Montevideo time zone. Tried: " + string.Join(", ", TimeZoneIds));
    }
}
