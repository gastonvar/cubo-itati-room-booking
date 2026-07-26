using RoomBooking.Api.Features.Bookings.Entities;
using RoomBooking.Api.Features.Bookings.Repositories;
using RoomBooking.Api.Features.Rooms.Repositories;
using RoomBooking.Api.Shared.Domain;
using RoomBooking.Api.Shared.Time;

namespace RoomBooking.Api.Features.Rooms.Services;

public sealed class RoomService(
    IRoomRepository rooms,
    IBookingRepository bookings,
    IBookingClock clock) : IRoomService
{
    public async Task<List<RoomDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        var all = await rooms.GetAllOrderedAsync(cancellationToken);
        return all.Select(r => new RoomDto(r.Code, r.Capacity)).ToList();
    }

    public async Task<(List<RoomDto>? Rooms, string? Error)> GetAvailableAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        int attendees,
        CancellationToken cancellationToken = default)
    {
        if (attendees < 1)
            return (null, "Attendees must be at least 1");

        if (to <= from)
            return (null, "to must be after from");

        var available = await rooms.GetAvailableAsync(from, to, attendees, cancellationToken);
        return (available.Select(r => new RoomDto(r.Code, r.Capacity)).ToList(), null);
    }

    public async Task<(ScheduleResponse? Result, string? Error)> GetScheduleAsync(
        string code,
        DateOnly fromDate,
        DateOnly toDateExclusive,
        CancellationToken cancellationToken = default)
    {
        if (toDateExclusive <= fromDate)
            return (null, "toDateExclusive must be after fromDate");

        var room = await rooms.GetByCodeAsync(code, cancellationToken);
        if (room is null)
            return (null, $"Room '{code}' does not exist");

        var (from, to) = clock.ExpandScheduleRange(fromDate, toDateExclusive);
        var bookingsInRange = await bookings.ListOverlappingAsync(code, from, to, cancellationToken);

        var occupied = bookingsInRange
            .Select(b => new OccupiedSlot(
                clock.ToLocal(b.Start),
                clock.ToLocal(b.End),
                b.Title,
                b.Owner,
                b.Attendees))
            .ToList();

        // Free slots = business hours minus occupied bookings, clipped to [from, to].
        var freeSlots = BuildFreeSlots(from, to, bookingsInRange, clock.TimeZone)
            .Select(slot => new FreeSlot(
                clock.ToLocal(slot.Start),
                clock.ToLocal(slot.End)))
            .ToList();

        return (new ScheduleResponse(room.Code, occupied, freeSlots), null);
    }

    private static List<FreeSlot> BuildFreeSlots(
        DateTimeOffset from,
        DateTimeOffset to,
        List<Booking> bookingsInRange,
        TimeZoneInfo timeZone)
    {
        var occupiedTuples = bookingsInRange
            .Select(b => (b.Start, b.End))
            .ToList();

        var freeSlots = new List<FreeSlot>();
        var current = from;

        while (current < to)
        {
            var (dayStart, dayEnd) = BusinessCalendar.GetDayBounds(current, timeZone);
            var effectiveStart = from > dayStart ? from : dayStart;
            var effectiveEnd = to < dayEnd ? to : dayEnd;

            if (effectiveStart < effectiveEnd)
            {
                var dayOccupied = occupiedTuples
                    .Where(o => SlotRules.Overlaps(effectiveStart, effectiveEnd, o.Start, o.End))
                    .Select(o => (
                        Start: o.Start < effectiveStart ? effectiveStart : o.Start,
                        End: o.End > effectiveEnd ? effectiveEnd : o.End))
                    .ToList();

                var dayFree = SlotRules.GetFreeSlots(effectiveStart, effectiveEnd, dayOccupied);
                freeSlots.AddRange(dayFree.Select(f => new FreeSlot(f.Start, f.End)));
            }

            current = BusinessCalendar.NextDayStart(dayEnd, timeZone);
        }

        return freeSlots;
    }
}
