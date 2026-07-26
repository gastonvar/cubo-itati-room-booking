using Microsoft.EntityFrameworkCore;
using RoomBooking.Api.Features.Auth.Entities;
using RoomBooking.Api.Features.Bookings.Entities;
using RoomBooking.Api.Features.Rooms.Entities;

namespace RoomBooking.Api.Shared.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Room>()
            .HasMany(r => r.Bookings)
            .WithOne(b => b.Room)
            .HasForeignKey(b => b.RoomCode);

        modelBuilder.Entity<Booking>()
            .HasIndex(b => new { b.RoomCode, b.Start, b.End });

        modelBuilder.Entity<RefreshToken>()
            .HasIndex(t => t.TokenHash)
            .IsUnique();
    }
}
