using RoomBooking.Api.Features.Bookings.Entities;

namespace RoomBooking.Api.Features.Bookings.Repositories;

public interface IBookingRepository
{
    Task<Booking?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<List<Booking>> ListByOwnerAsync(string owner, CancellationToken cancellationToken = default);
    Task<List<Booking>> ListOverlappingAsync(
        string roomCode,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken = default);
    Task<Booking?> FindOverlappingAsync(
        string roomCode,
        DateTimeOffset start,
        DateTimeOffset end,
        CancellationToken cancellationToken = default);
    Task AddAsync(Booking booking, CancellationToken cancellationToken = default);
    void Remove(Booking booking);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
