namespace RoomBooking.Api.Shared.Time;

public interface IBookingClock
{
    TimeZoneInfo TimeZone { get; }
    DateTimeOffset UtcNow { get; }
    DateTimeOffset NowLocal { get; }
    DateOnly TodayLocal { get; }

    DateTimeOffset ToLocal(DateTimeOffset instant);
    DateTimeOffset AtLocal(DateOnly date, TimeOnly time);
    string FormatLocal(DateTimeOffset instant, string format = "yyyy-MM-dd HH:mm");
    (DateTimeOffset From, DateTimeOffset To) ExpandScheduleRange(
        DateOnly fromDate,
        DateOnly toDateExclusive);
}
