namespace RoomBooking.Api.Shared.Domain;

public static class BusinessCalendar
{
    public static (DateTimeOffset Start, DateTimeOffset End) GetDayBounds(
        DateTimeOffset instant,
        TimeZoneInfo timeZone)
    {
        var localDate = DateOnly.FromDateTime(
            TimeZoneInfo.ConvertTime(instant, timeZone).DateTime);
        return (
            AtLocal(localDate, SlotRules.DayStart, timeZone),
            AtLocal(localDate, SlotRules.DayEnd, timeZone));
    }

    public static DateTimeOffset NextDayStart(
        DateTimeOffset dayEnd,
        TimeZoneInfo timeZone)
    {
        var localDate = DateOnly.FromDateTime(
            TimeZoneInfo.ConvertTime(dayEnd, timeZone).DateTime);
        return AtLocal(localDate.AddDays(1), SlotRules.DayStart, timeZone);
    }

    public static DateTimeOffset AtLocal(
        DateOnly date,
        TimeOnly time,
        TimeZoneInfo timeZone)
    {
        var local = DateTime.SpecifyKind(
            date.ToDateTime(time),
            DateTimeKind.Unspecified);
        return new DateTimeOffset(local, timeZone.GetUtcOffset(local));
    }
}
