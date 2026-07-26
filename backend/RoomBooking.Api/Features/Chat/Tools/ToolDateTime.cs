using System.Globalization;
using RoomBooking.Api.Shared.Domain;
using RoomBooking.Api.Shared.Time;

namespace RoomBooking.Api.Features.Chat.Tools;

/// <summary>
/// Parses LLM-supplied datetimes. Strings without an offset are treated as America/Montevideo
/// local time so UTC server clocks do not shift 09:30 into the wrong slot.
/// </summary>
public interface IToolDateTimeNormalizer
{
    DateTimeOffset Parse(string value);
    (DateTimeOffset Start, DateTimeOffset End, string? AdjustmentNote) SnapRangeToFuture(
        DateTimeOffset start,
        DateTimeOffset end);
    (DateTimeOffset Start, DateTimeOffset End, string? AdjustmentNote, bool Rejected)
        NormalizeAvailabilityRange(DateTimeOffset start, DateTimeOffset end);
    (DateTimeOffset Start, DateTimeOffset End, string? AdjustmentNote) SnapPastCalendarToToday(
        DateTimeOffset start,
        DateTimeOffset end);
}

public sealed class ToolDateTimeNormalizer(IBookingClock clock) : IToolDateTimeNormalizer
{
    public DateTimeOffset Parse(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Datetime value is required");

        var trimmed = value.Trim();
        DateTimeOffset parsed;
        if (HasExplicitOffset(trimmed))
        {
            parsed = DateTimeOffset.Parse(trimmed, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
        }
        else
        {
            var local = DateTime.Parse(trimmed, CultureInfo.InvariantCulture, DateTimeStyles.None);
            var unspecified = DateTime.SpecifyKind(local, DateTimeKind.Unspecified);
            parsed = new DateTimeOffset(unspecified, clock.TimeZone.GetUtcOffset(unspecified));
        }

        // Always return Montevideo offset so LLM/API wall-clock displays stay consistent.
        return clock.ToLocal(parsed);
    }

    // create_booking guard: detect past/wrong-year tool datetimes. Caller must NOT book the
    // suggested slot — only use the note to ask the user for a valid time.
    public (DateTimeOffset Start, DateTimeOffset End, string? AdjustmentNote) SnapRangeToFuture(
        DateTimeOffset start,
        DateTimeOffset end)
    {
        var now = clock.NowLocal;
        start = clock.ToLocal(start);
        end = clock.ToLocal(end);
        if (start >= now)
            return (start, end, null);

        if (end <= start)
            return (start, end, null);

        var tz = clock.TimeZone;
        var localStart = clock.ToLocal(start);
        var localEnd = clock.ToLocal(end);
        var duration = localEnd - localStart;
        var startTod = localStart.TimeOfDay;
        var today = clock.TodayLocal.ToDateTime(TimeOnly.MinValue);

        for (var day = today; ; day = day.AddDays(1))
        {
            var candidateLocal = DateTime.SpecifyKind(day.Add(startTod), DateTimeKind.Unspecified);
            var startOffset = tz.GetUtcOffset(candidateLocal);
            var candidateStart = new DateTimeOffset(candidateLocal, startOffset);
            if (candidateStart < now)
                continue;

            var endLocal = DateTime.SpecifyKind(candidateLocal + duration, DateTimeKind.Unspecified);
            var endOffset = tz.GetUtcOffset(endLocal);
            var candidateEnd = new DateTimeOffset(endLocal, endOffset);

            var note =
                $"Corrected past tool datetime {clock.FormatLocal(start)} → {clock.FormatLocal(candidateStart)} Montevideo. " +
                $"Always use the system prompt's current date; do not invent older years.";
            return (candidateStart, candidateEnd, note);
        }
    }

    /// <summary>
    /// Availability lookups: never invent another calendar day.
    /// If start is past but end is still future, clamp start forward on the same day.
    /// If the whole range is past, returns <c>rejected: true</c>.
    /// </summary>
    public (DateTimeOffset Start, DateTimeOffset End, string? AdjustmentNote, bool Rejected)
        NormalizeAvailabilityRange(DateTimeOffset start, DateTimeOffset end)
    {
        var now = clock.NowLocal;
        start = clock.ToLocal(start);
        end = clock.ToLocal(end);

        if (end <= start)
            return (start, end, null, false);

        if (end <= now)
        {
            var note =
                $"Requested range {clock.FormatLocal(start)}–{clock.FormatLocal(end, "HH:mm")} Montevideo is entirely in the past " +
                $"(now {clock.FormatLocal(now)}). Ask the user for a future time; do not invent another day.";
            return (start, end, note, true);
        }

        if (start >= now)
            return (start, end, null, false);

        var tz = clock.TimeZone;
        var localNow = clock.ToLocal(now).DateTime;
        var ticks = SlotRules.SlotDuration.Ticks;
        var remainder = localNow.Ticks % ticks;
        var alignedLocal = DateTime.SpecifyKind(
            remainder == 0
                ? localNow
                : new DateTime(localNow.Ticks + (ticks - remainder)),
            DateTimeKind.Unspecified);
        var candidateStart = new DateTimeOffset(alignedLocal, tz.GetUtcOffset(alignedLocal));

        if (candidateStart >= end)
        {
            var note =
                $"Requested range {clock.FormatLocal(start)}–{clock.FormatLocal(end, "HH:mm")} Montevideo has no remaining future time " +
                $"(now {clock.FormatLocal(now)}). Ask the user for a future time; do not invent another day.";
            return (start, end, note, true);
        }

        var adjustmentNote =
            $"Start was in the past ({clock.FormatLocal(start)}); clamped availability search to " +
            $"{clock.FormatLocal(candidateStart)}–{clock.FormatLocal(end, "HH:mm")} Montevideo (same day). " +
            "Tell the user the start was adjusted forward because it had already passed; do not move to another day.";
        return (candidateStart, end, adjustmentNote, false);
    }

    // Schedule lookups: if the range is on a past calendar day, move it to today
    // (same wall-clock times) so morning occupancy is still visible.
    public (DateTimeOffset Start, DateTimeOffset End, string? AdjustmentNote) SnapPastCalendarToToday(
        DateTimeOffset start,
        DateTimeOffset end)
    {
        var tz = clock.TimeZone;
        var localStart = clock.ToLocal(start);
        var localEnd = clock.ToLocal(end);
        var today = clock.TodayLocal.ToDateTime(TimeOnly.MinValue);

        if (localStart.Date >= today)
            return (localStart, localEnd, null);

        if (end <= start)
            return (localStart, localEnd, null);

        var duration = localEnd - localStart;
        var todayStartLocal = DateTime.SpecifyKind(today.Add(localStart.TimeOfDay), DateTimeKind.Unspecified);
        var todayEndLocal = DateTime.SpecifyKind(todayStartLocal + duration, DateTimeKind.Unspecified);
        var candidateStart = new DateTimeOffset(todayStartLocal, tz.GetUtcOffset(todayStartLocal));
        var candidateEnd = new DateTimeOffset(todayEndLocal, tz.GetUtcOffset(todayEndLocal));

        var note =
            $"Corrected past schedule range {clock.FormatLocal(start, "yyyy-MM-dd")} → {clock.FormatLocal(candidateStart, "yyyy-MM-dd")} Montevideo. " +
            "Always use the system prompt's current date; do not invent older years.";
        return (candidateStart, candidateEnd, note);
    }

    private static bool HasExplicitOffset(string value)
    {
        if (value.EndsWith('Z') || value.EndsWith('z'))
            return true;

        var tIndex = value.IndexOf('T');
        if (tIndex < 0)
            return false;

        return value.IndexOf('+', tIndex) >= 0 || value.LastIndexOf('-') > tIndex;
    }
}
