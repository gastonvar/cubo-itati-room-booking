namespace RoomBooking.Api.Shared.Domain;

public static class SlotRules
{
    public static readonly TimeSpan SlotDuration = TimeSpan.FromMinutes(30);
    public static readonly TimeSpan MinDuration = SlotDuration;
    public static readonly TimeSpan MaxDuration = TimeSpan.FromHours(3);
    public static readonly TimeOnly DayStart = new(8, 0);
    public static readonly TimeOnly DayEnd = new(20, 0);

    public static bool IsAlignedToSlot(DateTimeOffset dt, TimeZoneInfo timeZone)
    {
        var local = TimeZoneInfo.ConvertTime(dt, timeZone);
        return local.TimeOfDay.Ticks % SlotDuration.Ticks == 0;
    }

    public static bool IsWithinBusinessHours(
        DateTimeOffset start,
        DateTimeOffset end,
        TimeZoneInfo timeZone)
    {
        var localStart = TimeZoneInfo.ConvertTime(start, timeZone);
        var localEnd = TimeZoneInfo.ConvertTime(end, timeZone);

        if (localStart.Date != localEnd.Date) return false;

        var startTime = TimeOnly.FromTimeSpan(localStart.TimeOfDay);
        var endTime = TimeOnly.FromTimeSpan(localEnd.TimeOfDay);

        return startTime >= DayStart && endTime <= DayEnd;
    }

    public static bool IsValidDuration(DateTimeOffset start, DateTimeOffset end)
    {
        var duration = end - start;
        return duration >= MinDuration
            && duration <= MaxDuration
            && duration.Ticks % SlotDuration.Ticks == 0;
    }

    public static bool Overlaps(DateTimeOffset s1, DateTimeOffset e1, DateTimeOffset s2, DateTimeOffset e2)
        => s1 < e2 && s2 < e1;

    public static List<(DateTimeOffset Start, DateTimeOffset End)> GetFreeSlots(
        DateTimeOffset dayStart,
        DateTimeOffset dayEnd,
        List<(DateTimeOffset Start, DateTimeOffset End)> occupied)
    {
        var sorted = occupied.OrderBy(o => o.Start).ToList();
        var free = new List<(DateTimeOffset, DateTimeOffset)>();
        var cursor = dayStart;

        foreach (var (s, e) in sorted)
        {
            if (cursor < s)
                free.Add((cursor, s));
            if (e > cursor)
                cursor = e;
        }

        if (cursor < dayEnd)
            free.Add((cursor, dayEnd));

        return free;
    }
}
