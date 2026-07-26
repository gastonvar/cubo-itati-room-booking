using Microsoft.EntityFrameworkCore;
using RoomBooking.Api.Features.Bookings.Repositories;
using RoomBooking.Api.Features.Bookings.Services;
using RoomBooking.Api.Features.Rooms.Entities;
using RoomBooking.Api.Features.Rooms.Repositories;
using RoomBooking.Api.Shared.Data;
using RoomBooking.Api.Shared.Domain;
using RoomBooking.Api.Shared.Time;
using RoomBooking.Api.Tests.Shared.Time;

namespace RoomBooking.Api.Tests.Features.Bookings;

public class BookingServiceTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly BookingService _service;
    private readonly BookingClock _clock = new(new FixedTimeProvider(
        new DateTimeOffset(2026, 7, 26, 15, 0, 0, TimeSpan.Zero)));

    private DateTimeOffset Montevideo(int year, int month, int day, int hour, int minute) =>
        _clock.AtLocal(new DateOnly(year, month, day), new TimeOnly(hour, minute));

    /// <summary>Tomorrow (or later) at the given Montevideo clock time — always in the future.</summary>
    private DateTimeOffset FutureSlot(int hour, int minute, int dayOffset = 1)
    {
        var day = _clock.TodayLocal.AddDays(dayOffset);
        return _clock.AtLocal(day, new TimeOnly(hour, minute));
    }

    public BookingServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;

        _db = new AppDbContext(options);
        _db.Database.OpenConnection();
        _db.Database.EnsureCreated();

        _db.Rooms.AddRange(
            new Room { Code = "A", Capacity = 4 },
            new Room { Code = "B", Capacity = 6 });
        _db.SaveChanges();

        _service = new BookingService(
            new BookingRepository(_db),
            new RoomRepository(_db),
            _db,
            _clock);
    }

    public void Dispose()
    {
        _db.Database.CloseConnection();
        _db.Dispose();
    }

    [Fact]
    public async Task CreateBooking_ValidRequest_ReturnsBooking()
    {
        var start = FutureSlot(10, 0);
        var end = FutureSlot(11, 0);

        var result = await _service.CreateBookingAsync("A", "Test Meeting", 3, start, end, "User1");

        Assert.NotNull(result.Booking);
        Assert.Null(result.Error);
        Assert.Equal("A", result.Booking.RoomCode);
        Assert.Equal("Test Meeting", result.Booking.Title);
    }

    [Fact]
    public async Task CreateBooking_NonExistentRoom_ReturnsNotFound()
    {
        var start = FutureSlot(10, 0);
        var end = FutureSlot(11, 0);

        var result = await _service.CreateBookingAsync("Z", "Test", 3, start, end, "User1");

        Assert.Null(result.Booking);
        Assert.Equal("Room 'Z' does not exist", result.Error!.Message);
        Assert.Equal(BookingErrorKind.NotFound, result.Error.Kind);
    }

    [Fact]
    public async Task CreateBooking_RoomPresentInDatabase_SucceedsEvenIfNotInSeedCatalog()
    {
        _db.Rooms.Add(new Room { Code = "F", Capacity = 10 });
        await _db.SaveChangesAsync();
        var start = FutureSlot(10, 0);
        var end = FutureSlot(11, 0);

        var result = await _service.CreateBookingAsync("F", "Test", 3, start, end, "User1");

        Assert.NotNull(result.Booking);
        Assert.Null(result.Error);
        Assert.Equal("F", result.Booking.RoomCode);
    }

    [Fact]
    public async Task CreateBooking_ExceedsCapacity_ReturnsValidation()
    {
        var start = FutureSlot(10, 0);
        var end = FutureSlot(11, 0);

        var result = await _service.CreateBookingAsync("A", "Test", 10, start, end, "User1");

        Assert.Null(result.Booking);
        Assert.Equal("Room A capacity is 4, but 10 attendees were requested", result.Error!.Message);
        Assert.Equal(BookingErrorKind.Validation, result.Error.Kind);
    }

    [Fact]
    public async Task CreateBooking_MisalignedSlot_ReturnsValidation()
    {
        var start = FutureSlot(10, 15);
        var end = FutureSlot(11, 0);

        var result = await _service.CreateBookingAsync("A", "Test", 3, start, end, "User1");

        Assert.Null(result.Booking);
        Assert.Equal("Start and end must align to 30-minute slots", result.Error!.Message);
        Assert.Equal(BookingErrorKind.Validation, result.Error.Kind);
    }

    [Fact]
    public async Task CreateBooking_OutsideBusinessHours_ReturnsValidation()
    {
        var start = FutureSlot(7, 0);
        var end = FutureSlot(8, 0);

        var result = await _service.CreateBookingAsync("A", "Test", 3, start, end, "User1");

        Assert.Null(result.Booking);
        Assert.Contains("08:00-20:00", result.Error!.Message);
        Assert.Equal(BookingErrorKind.Validation, result.Error.Kind);
    }

    [Fact]
    public async Task CreateBooking_TooLong_ReturnsValidation()
    {
        var start = FutureSlot(10, 0);
        var end = FutureSlot(14, 0);

        var result = await _service.CreateBookingAsync("A", "Test", 3, start, end, "User1");

        Assert.Null(result.Booking);
        Assert.Equal("Booking duration must be between 30 minutes and 3 hours", result.Error!.Message);
        Assert.Equal(BookingErrorKind.Validation, result.Error.Kind);
    }

    [Fact]
    public async Task CreateBooking_PlaceholderTitle_ReturnsValidation()
    {
        var start = FutureSlot(10, 0);
        var end = FutureSlot(11, 0);

        var result = await _service.CreateBookingAsync("A", "Meeting", 3, start, end, "User1");

        Assert.Null(result.Booking);
        Assert.Contains("specific meeting title", result.Error!.Message);
        Assert.Equal(BookingErrorKind.Validation, result.Error.Kind);
    }

    [Fact]
    public async Task CreateBooking_PastStart_ReturnsValidation()
    {
        var start = Montevideo(2023, 10, 5, 9, 30);
        var end = Montevideo(2023, 10, 5, 11, 0);

        var result = await _service.CreateBookingAsync("A", "supameeting", 3, start, end, "User1");

        Assert.Null(result.Booking);
        Assert.Contains("past", result.Error!.Message);
        Assert.Contains("2023-10-05", result.Error.Message);
        Assert.Equal(BookingErrorKind.Validation, result.Error.Kind);
    }

    [Fact]
    public async Task CreateBooking_Overlapping_ReturnsConflict()
    {
        var start = FutureSlot(10, 0);
        var end = FutureSlot(11, 0);
        await _service.CreateBookingAsync("A", "First", 3, start, end, "User1");

        var start2 = FutureSlot(10, 30);
        var end2 = FutureSlot(11, 30);
        var result = await _service.CreateBookingAsync("A", "Second", 3, start2, end2, "User2");

        Assert.Null(result.Booking);
        Assert.Contains("already booked", result.Error!.Message);
        Assert.Contains("Montevideo", result.Error.Message);
        Assert.DoesNotContain("Z", result.Error.Message);
        Assert.Equal(BookingErrorKind.Conflict, result.Error.Kind);
    }

    [Fact]
    public async Task CreateBooking_UtcOffsetInput_ReturnsMontevideoDto()
    {
        var startUtc = FutureSlot(10, 0).ToUniversalTime();
        var endUtc = FutureSlot(11, 0).ToUniversalTime();

        var result = await _service.CreateBookingAsync("A", "Utc Input", 3, startUtc, endUtc, "User1");

        Assert.NotNull(result.Booking);
        Assert.Equal(TimeSpan.FromHours(-3), result.Booking.Start.Offset);
        Assert.Equal(10, result.Booking.Start.Hour);
        Assert.Equal(11, result.Booking.End.Hour);
    }

    [Fact]
    public async Task CancelBooking_AsOwner_Succeeds()
    {
        var start = FutureSlot(10, 0);
        var end = FutureSlot(11, 0);
        var created = await _service.CreateBookingAsync("A", "Test", 3, start, end, "User1");

        var result = await _service.CancelBookingAsync(created.Booking!.Id, "User1");

        Assert.True(result.Success);
        Assert.Null(result.Error);
    }

    [Fact]
    public async Task CancelBooking_AsNonOwner_ReturnsForbidden()
    {
        var start = FutureSlot(10, 0);
        var end = FutureSlot(11, 0);
        var created = await _service.CreateBookingAsync("A", "Test", 3, start, end, "User1");

        var result = await _service.CancelBookingAsync(created.Booking!.Id, "User2");

        Assert.False(result.Success);
        Assert.Equal("Only the booking owner can cancel this reservation", result.Error!.Message);
        Assert.Equal(BookingErrorKind.Forbidden, result.Error.Kind);
    }

    [Fact]
    public async Task ListByOwner_ReturnsOnlyOwnerBookings()
    {
        var start = FutureSlot(10, 0);
        var end = FutureSlot(11, 0);
        await _service.CreateBookingAsync("A", "Mine", 3, start, end, "User1");
        await _service.CreateBookingAsync("B", "Theirs", 3,
            FutureSlot(11, 0), FutureSlot(12, 0), "User2");

        var mine = await _service.ListByOwnerAsync("User1");

        Assert.Single(mine);
        Assert.Equal("Mine", mine[0].Title);
        Assert.Equal("A", mine[0].RoomCode);
    }

    [Fact]
    public async Task CancelBooking_NonExistent_ReturnsNotFound()
    {
        var result = await _service.CancelBookingAsync(999, "User1");

        Assert.False(result.Success);
        Assert.Equal("Booking not found", result.Error!.Message);
        Assert.Equal(BookingErrorKind.NotFound, result.Error.Kind);
    }
}
