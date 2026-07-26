using System.ComponentModel.DataAnnotations;

namespace RoomBooking.Api.Features.Auth.Entities;

public class User
{
    [Key]
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
}
