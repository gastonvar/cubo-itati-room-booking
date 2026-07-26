using System.ComponentModel.DataAnnotations;
using RoomBooking.Api.Features.Bookings.Entities;

namespace RoomBooking.Api.Features.Rooms.Entities;

public class Room
{
    [Key]
    public string Code { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public List<Booking> Bookings { get; set; } = [];
}
