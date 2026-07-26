using RoomBooking.Api.Features.Rooms.Entities;

namespace RoomBooking.Api.Features.Bookings.Entities;

public class Booking
{
    public int Id { get; set; }
    public string RoomCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int Attendees { get; set; }
    public DateTimeOffset Start { get; set; }
    public DateTimeOffset End { get; set; }
    public string Owner { get; set; } = string.Empty;

    public Room Room { get; set; } = null!;
}
