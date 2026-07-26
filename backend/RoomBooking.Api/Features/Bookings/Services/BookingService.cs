using RoomBooking.Api.Features.Bookings.Entities;
using RoomBooking.Api.Features.Bookings.Repositories;
using RoomBooking.Api.Features.Rooms.Repositories;
using RoomBooking.Api.Shared.Data;
using RoomBooking.Api.Shared.Domain;
using RoomBooking.Api.Shared.Time;

namespace RoomBooking.Api.Features.Bookings.Services;

public sealed class BookingService(
    IBookingRepository bookings,
    IRoomRepository rooms,
    AppDbContext db,
    IBookingClock clock) : IBookingService
{
    public async Task<CreateBookingResult> CreateBookingAsync(
        string roomCode,
        string title,
        int attendees,
        DateTimeOffset start,
        DateTimeOffset end,
        string owner,
        CancellationToken cancellationToken = default)
    {
        if (attendees < 1)
            return CreateBookingResult.Fail(BookingErrorKind.Validation, "Attendees must be at least 1");

        if (string.IsNullOrWhiteSpace(title))
            return CreateBookingResult.Fail(BookingErrorKind.Validation, "Title is required");

        title = title.Trim();
        if (IsPlaceholderTitle(title))
        {
            return CreateBookingResult.Fail(
                BookingErrorKind.Validation,
                "Please provide a specific meeting title (not a generic placeholder like \"Meeting\")");
        }

        if (!SlotRules.IsAlignedToSlot(start, clock.TimeZone)
            || !SlotRules.IsAlignedToSlot(end, clock.TimeZone))
            return CreateBookingResult.Fail(BookingErrorKind.Validation, "Start and end must align to 30-minute slots");

        if (!SlotRules.IsWithinBusinessHours(start, end, clock.TimeZone))
        {
            return CreateBookingResult.Fail(
                BookingErrorKind.Validation,
                "Bookings must be within 08:00-20:00 America/Montevideo on the same day");
        }

        if (!SlotRules.IsValidDuration(start, end))
        {
            return CreateBookingResult.Fail(
                BookingErrorKind.Validation,
                "Booking duration must be between 30 minutes and 3 hours");
        }

        var now = clock.NowLocal;
        if (start < now)
        {
            return CreateBookingResult.Fail(
                BookingErrorKind.Validation,
                $"Booking start {clock.FormatLocal(start)} is in the past; current Montevideo time is {clock.FormatLocal(now)}. " +
                "Re-call with today's date from the system prompt (do not use an older year).");
        }

        start = clock.ToLocal(start);
        end = clock.ToLocal(end);

        await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);

        var room = await rooms.GetByCodeAsync(roomCode, cancellationToken);
        if (room is null)
            return CreateBookingResult.Fail(BookingErrorKind.NotFound, $"Room '{roomCode}' does not exist");

        if (attendees > room.Capacity)
        {
            return CreateBookingResult.Fail(
                BookingErrorKind.Validation,
                $"Room {roomCode} capacity is {room.Capacity}, but {attendees} attendees were requested");
        }

        // Re-check inside the transaction so concurrent creates cannot both pass.
        var conflict = await bookings.FindOverlappingAsync(roomCode, start, end, cancellationToken);
        if (conflict is not null)
        {
            return CreateBookingResult.Fail(
                BookingErrorKind.Conflict,
                $"Room {roomCode} is already booked by {conflict.Owner} ({conflict.Title}) from " +
                $"{clock.FormatLocal(conflict.Start)} to {clock.FormatLocal(conflict.End)} Montevideo");
        }

        var booking = new Booking
        {
            RoomCode = roomCode,
            Title = title,
            Attendees = attendees,
            Start = start,
            End = end,
            Owner = owner
        };

        await bookings.AddAsync(booking, cancellationToken);
        await bookings.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);

        return CreateBookingResult.Ok(ToDto(booking));
    }

    public async Task<CancelBookingResult> CancelBookingAsync(
        int id,
        string username,
        CancellationToken cancellationToken = default)
    {
        var booking = await bookings.GetByIdAsync(id, cancellationToken);

        if (booking is null)
            return CancelBookingResult.Fail(BookingErrorKind.NotFound, "Booking not found");

        if (booking.Owner != username)
        {
            return CancelBookingResult.Fail(
                BookingErrorKind.Forbidden,
                "Only the booking owner can cancel this reservation");
        }

        bookings.Remove(booking);
        await bookings.SaveChangesAsync(cancellationToken);

        return CancelBookingResult.Ok();
    }

    public async Task<List<BookingDto>> ListByOwnerAsync(
        string owner,
        CancellationToken cancellationToken = default)
    {
        var list = await bookings.ListByOwnerAsync(owner, cancellationToken);
        return list.Select(ToDto).ToList();
    }

    private BookingDto ToDto(Booking b) =>
        new(b.Id, b.RoomCode, b.Title, b.Attendees, clock.ToLocal(b.Start), clock.ToLocal(b.End));

    private static bool IsPlaceholderTitle(string title) =>
        title.Equals("Meeting", StringComparison.OrdinalIgnoreCase)
        || title.Equals("Untitled", StringComparison.OrdinalIgnoreCase)
        || title.Equals("Title", StringComparison.OrdinalIgnoreCase);
}
