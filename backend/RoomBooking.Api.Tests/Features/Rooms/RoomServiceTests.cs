using Microsoft.EntityFrameworkCore;
using RoomBooking.Api.Features.Bookings.Entities;
using RoomBooking.Api.Features.Bookings.Repositories;
using RoomBooking.Api.Features.Rooms.Entities;
using RoomBooking.Api.Features.Rooms.Repositories;
using RoomBooking.Api.Features.Rooms.Services;
using RoomBooking.Api.Shared.Data;
using RoomBooking.Api.Shared.Time;
using RoomBooking.Api.Tests.Shared.Time;

namespace RoomBooking.Api.Tests.Features.Rooms;

public sealed class RoomServiceTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly RoomService _service;
    private static readonly TimeSpan MontevideoOffset = TimeSpan.FromHours(-3);
    private static readonly DateOnly TestDate = new(2030, 1, 15);

    public RoomServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;

        _db = new AppDbContext(options);
        _db.Database.OpenConnection();
        _db.Database.EnsureCreated();
        _db.Rooms.AddRange(
            new Room { Code = "B", Capacity = 6 },
            new Room { Code = "A", Capacity = 4 });
        _db.SaveChanges();

        _service = new RoomService(
            new RoomRepository(_db),
            new BookingRepository(_db),
            new BookingClock(new FixedTimeProvider(
                new DateTimeOffset(2030, 1, 15, 12, 0, 0, TimeSpan.Zero))));
    }

    [Fact]
    public async Task List_ReturnsRoomsOrderedByCode()
    {
        var rooms = await _service.ListAsync();

        Assert.Collection(
            rooms,
            room => Assert.Equal(("A", 4), (room.Code, room.Capacity)),
            room => Assert.Equal(("B", 6), (room.Code, room.Capacity)));
    }

    [Fact]
    public async Task GetAvailable_ExcludesRoomWithOverlappingBooking()
    {
        var from = At(10, 0);
        var to = At(11, 0);
        await AddBookingAsync("A", from, to);

        var (rooms, error) = await _service.GetAvailableAsync(from, to, attendees: 3);

        Assert.Null(error);
        Assert.NotNull(rooms);
        Assert.Collection(rooms, room => Assert.Equal("B", room.Code));
    }

    [Fact]
    public async Task GetSchedule_ReturnsOccupiedAndFreeRanges()
    {
        var bookingStart = At(10, 0);
        var bookingEnd = At(11, 0);
        await AddBookingAsync("A", bookingStart, bookingEnd);

        var (schedule, error) = await _service.GetScheduleAsync(
            "A", TestDate, TestDate.AddDays(1));

        Assert.Null(error);
        Assert.NotNull(schedule);
        Assert.Collection(
            schedule.Occupied,
            slot => Assert.Equal((bookingStart, bookingEnd), (slot.Start, slot.End)));
        Assert.Collection(
            schedule.FreeSlots,
            slot => Assert.Equal((At(8, 0), bookingStart), (slot.Start, slot.End)),
            slot => Assert.Equal((bookingEnd, At(20, 0)), (slot.Start, slot.End)));
    }

    [Fact]
    public async Task GetSchedule_ReturnsAllSlotBoundariesInMontevideoTime()
    {
        await AddBookingAsync("A", At(10, 30), At(13, 30));
        await AddBookingAsync("A", At(13, 30), At(14, 30));
        await AddBookingAsync("A", At(18, 0), At(19, 0));

        var (schedule, error) = await _service.GetScheduleAsync(
            "A", TestDate, TestDate.AddDays(1));

        Assert.Null(error);
        Assert.NotNull(schedule);
        Assert.All(
            schedule.Occupied.SelectMany(slot => new[] { slot.Start, slot.End })
                .Concat(schedule.FreeSlots.SelectMany(slot => new[] { slot.Start, slot.End })),
            boundary => Assert.Equal(MontevideoOffset, boundary.Offset));
        Assert.Collection(
            schedule.FreeSlots,
            slot => Assert.Equal((At(8, 0), At(10, 30)), (slot.Start, slot.End)),
            slot => Assert.Equal((At(14, 30), At(18, 0)), (slot.Start, slot.End)),
            slot => Assert.Equal((At(19, 0), At(20, 0)), (slot.Start, slot.End)));
    }

    [Fact]
    public async Task GetSchedule_HalfOpenDateRange_ReturnsOnlyRequestedDates()
    {
        await AddBookingAsync("A", At(10, 0), At(11, 0));

        var (schedule, error) = await _service.GetScheduleAsync(
            "A", TestDate, TestDate.AddDays(1));

        Assert.Null(error);
        Assert.NotNull(schedule);
        Assert.All(
            schedule.FreeSlots.SelectMany(slot => new[] { slot.Start, slot.End }),
            boundary => Assert.Equal(MontevideoOffset, boundary.Offset));
        Assert.Collection(
            schedule.FreeSlots,
            slot => Assert.Equal((At(8, 0), At(10, 0)), (slot.Start, slot.End)),
            slot => Assert.Equal((At(11, 0), At(20, 0)), (slot.Start, slot.End)));
    }

    private static DateTimeOffset At(int hour, int minute) =>
        new(2030, 1, 15, hour, minute, 0, MontevideoOffset);

    private async Task AddBookingAsync(string roomCode, DateTimeOffset start, DateTimeOffset end)
    {
        _db.Bookings.Add(new Booking
        {
            RoomCode = roomCode,
            Title = "Test booking",
            Attendees = 2,
            Start = start,
            End = end,
            Owner = "User1"
        });
        await _db.SaveChangesAsync();
    }

    public void Dispose()
    {
        _db.Database.CloseConnection();
        _db.Dispose();
    }
}
