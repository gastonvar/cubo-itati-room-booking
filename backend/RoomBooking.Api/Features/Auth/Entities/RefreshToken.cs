using System.ComponentModel.DataAnnotations;

namespace RoomBooking.Api.Features.Auth.Entities;

public class RefreshToken
{
    [Key]
    public int Id { get; set; }

    public string Username { get; set; } = string.Empty;

    /// <summary>SHA-256 hex of the opaque refresh token (never store raw).</summary>
    public string TokenHash { get; set; } = string.Empty;

    public DateTimeOffset ExpiresAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? RevokedAt { get; set; }
}
