using Microsoft.EntityFrameworkCore;
using RoomBooking.Api.Features.Bookings.Entities;
using RoomBooking.Api.Shared.Data;

namespace RoomBooking.Api.Features.Bookings.Repositories;

public sealed class BookingRepository(AppDbContext db) : IBookingRepository
{
    public Task<Booking?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        db.Bookings.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

    public async Task<List<Booking>> ListByOwnerAsync(string owner, CancellationToken cancellationToken = default)
    {
        var list = await db.Bookings
            .AsNoTracking()
            .Where(b => b.Owner == owner)
            .ToListAsync(cancellationToken);

        // SQLite cannot ORDER BY DateTimeOffset in SQL.
        return list.OrderBy(b => b.Start).ToList();
    }

    public async Task<List<Booking>> ListOverlappingAsync(
        string roomCode,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken = default)
    {
        var overlaps = await LoadOverlappingAsync(roomCode, from, to, track: false, cancellationToken);
        return overlaps;
    }

    public async Task<Booking?> FindOverlappingAsync(
        string roomCode,
        DateTimeOffset start,
        DateTimeOffset end,
        CancellationToken cancellationToken = default)
    {
        var overlaps = await LoadOverlappingAsync(roomCode, start, end, track: true, cancellationToken);
        return overlaps.FirstOrDefault();
    }

    public async Task AddAsync(Booking booking, CancellationToken cancellationToken = default)
    {
        await db.Bookings.AddAsync(booking, cancellationToken);
    }

    public void Remove(Booking booking) => db.Bookings.Remove(booking);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        db.SaveChangesAsync(cancellationToken);

    /// <summary>
    /// SQLite cannot translate DateTimeOffset range compares — load by room in SQL, filter overlap in-process.
    /// </summary>
    private async Task<List<Booking>> LoadOverlappingAsync(
        string roomCode,
        DateTimeOffset from,
        DateTimeOffset to,
        bool track,
        CancellationToken cancellationToken)
    {
        IQueryable<Booking> query = db.Bookings.Where(b => b.RoomCode == roomCode);
        if (!track)
            query = query.AsNoTracking();

        var bookings = await query.ToListAsync(cancellationToken);
        return bookings
            .Where(b => b.Start < to && b.End > from)
            .OrderBy(b => b.Start)
            .ToList();
    }
}
