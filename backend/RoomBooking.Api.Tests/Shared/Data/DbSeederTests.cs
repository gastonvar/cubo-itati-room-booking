using Microsoft.EntityFrameworkCore;
using RoomBooking.Api.Features.Auth.Repositories;
using RoomBooking.Api.Features.Rooms.Entities;
using RoomBooking.Api.Features.Rooms.Repositories;
using RoomBooking.Api.Shared.Data;
using RoomBooking.Api.Shared.Domain;

namespace RoomBooking.Api.Tests.Shared.Data;

public sealed class DbSeederTests : IDisposable
{
    private readonly AppDbContext _db;

    public DbSeederTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;

        _db = new AppDbContext(options);
        _db.Database.OpenConnection();
        _db.Database.EnsureCreated();
    }

    [Fact]
    public async Task Seed_InsertsDefaultRoomsWhenEmpty()
    {
        await DbSeeder.SeedAsync(new RoomRepository(_db), new UserRepository(_db));

        var rooms = await _db.Rooms
            .AsNoTracking()
            .OrderBy(room => room.Code)
            .Select(room => new { room.Code, room.Capacity })
            .ToListAsync();

        Assert.Equal(
            RoomCatalog.Defaults.Select(room => new { room.Code, room.Capacity }),
            rooms);
    }

    [Fact]
    public async Task Seed_DoesNotOverwriteExistingRooms()
    {
        _db.Rooms.Add(new Room { Code = "A", Capacity = 99 });
        await _db.SaveChangesAsync();

        await DbSeeder.SeedAsync(new RoomRepository(_db), new UserRepository(_db));

        var rooms = await _db.Rooms.AsNoTracking().ToListAsync();
        Assert.Single(rooms);
        Assert.Equal(99, rooms[0].Capacity);
    }

    public void Dispose()
    {
        _db.Database.CloseConnection();
        _db.Dispose();
    }
}
