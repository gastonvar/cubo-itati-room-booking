using RoomBooking.Api.Features.Auth.Entities;
using RoomBooking.Api.Features.Auth.Repositories;
using RoomBooking.Api.Features.Rooms.Entities;
using RoomBooking.Api.Features.Rooms.Repositories;
using RoomBooking.Api.Shared.Domain;

namespace RoomBooking.Api.Shared.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IRoomRepository rooms, IUserRepository users)
    {
        if (!await rooms.AnyAsync())
        {
            foreach (var (code, capacity) in RoomCatalog.Defaults)
                await rooms.AddAsync(new Room { Code = code, Capacity = capacity });

            await rooms.SaveChangesAsync();
        }

        if (!await users.AnyAsync())
        {
            foreach (var username in UserCatalog.DefaultUsernames)
            {
                await users.AddAsync(new User
                {
                    Username = username,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(UserCatalog.DefaultPassword)
                });
            }

            await users.SaveChangesAsync();
        }
    }
}
