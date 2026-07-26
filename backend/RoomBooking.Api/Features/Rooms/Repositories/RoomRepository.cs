using Microsoft.EntityFrameworkCore;
using RoomBooking.Api.Features.Rooms.Entities;
using RoomBooking.Api.Shared.Data;

namespace RoomBooking.Api.Features.Rooms.Repositories;

public sealed class RoomRepository(AppDbContext db) : IRoomRepository
{
    public Task<List<Room>> GetAllOrderedAsync(CancellationToken cancellationToken = default) =>
        db.Rooms.AsNoTracking().OrderBy(r => r.Code).ToListAsync(cancellationToken);

    public Task<Room?> GetByCodeAsync(string code, CancellationToken cancellationToken = default) =>
        db.Rooms.AsNoTracking().FirstOrDefaultAsync(r => r.Code == code, cancellationToken);

    public async Task<List<Room>> GetAvailableAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        int attendees,
        CancellationToken cancellationToken = default)
    {
        var candidates = await db.Rooms
            .AsNoTracking()
            .Where(r => r.Capacity >= attendees)
            .OrderBy(r => r.Code)
            .ToListAsync(cancellationToken);

        if (candidates.Count == 0)
            return candidates;

        var codes = candidates.Select(r => r.Code).ToList();

        // SQLite cannot translate DateTimeOffset overlap predicates — load candidate-room bookings, filter in-process.
        var bookings = await db.Bookings
            .AsNoTracking()
            .Where(b => codes.Contains(b.RoomCode))
            .ToListAsync(cancellationToken);

        var busy = bookings
            .Where(b => b.Start < to && b.End > from)
            .Select(b => b.RoomCode)
            .ToHashSet(StringComparer.Ordinal);

        return candidates.Where(r => !busy.Contains(r.Code)).ToList();
    }

    public Task<bool> AnyAsync(CancellationToken cancellationToken = default) =>
        db.Rooms.AnyAsync(cancellationToken);

    public async Task AddAsync(Room room, CancellationToken cancellationToken = default)
    {
        await db.Rooms.AddAsync(room, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        db.SaveChangesAsync(cancellationToken);
}
