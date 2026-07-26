using RoomBooking.Api.Features.Rooms.Entities;

namespace RoomBooking.Api.Features.Rooms.Repositories;

public interface IRoomRepository
{
    Task<List<Room>> GetAllOrderedAsync(CancellationToken cancellationToken = default);
    Task<Room?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<List<Room>> GetAvailableAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        int attendees,
        CancellationToken cancellationToken = default);
    Task<bool> AnyAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Room room, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
