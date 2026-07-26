namespace RoomBooking.Api.Features.Bookings.Services;

public record BookingDto(int Id, string RoomCode, string Title, int Attendees, DateTimeOffset Start, DateTimeOffset End);

public enum BookingErrorKind
{
    NotFound,
    Validation,
    Conflict,
    Forbidden
}

public sealed record BookingError(BookingErrorKind Kind, string Message);

public sealed record CreateBookingResult(BookingDto? Booking, BookingError? Error)
{
    public static CreateBookingResult Ok(BookingDto booking) => new(booking, null);
    public static CreateBookingResult Fail(BookingErrorKind kind, string message) =>
        new(null, new BookingError(kind, message));
}

public sealed record CancelBookingResult(bool Success, BookingError? Error)
{
    public static CancelBookingResult Ok() => new(true, null);
    public static CancelBookingResult Fail(BookingErrorKind kind, string message) =>
        new(false, new BookingError(kind, message));
}

public interface IBookingService
{
    Task<CreateBookingResult> CreateBookingAsync(
        string roomCode,
        string title,
        int attendees,
        DateTimeOffset start,
        DateTimeOffset end,
        string owner,
        CancellationToken cancellationToken = default);

    Task<CancelBookingResult> CancelBookingAsync(
        int id,
        string username,
        CancellationToken cancellationToken = default);

    Task<List<BookingDto>> ListByOwnerAsync(
        string owner,
        CancellationToken cancellationToken = default);
}
