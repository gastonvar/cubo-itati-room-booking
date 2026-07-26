using Microsoft.EntityFrameworkCore;
using RoomBooking.Api.Features.Auth.Entities;
using RoomBooking.Api.Shared.Data;

namespace RoomBooking.Api.Features.Auth.Repositories;

public sealed class UserRepository(AppDbContext db) : IUserRepository
{
    public Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default) =>
        db.Users.FirstOrDefaultAsync(u => u.Username == username, cancellationToken);

    public Task<bool> AnyAsync(CancellationToken cancellationToken = default) =>
        db.Users.AnyAsync(cancellationToken);

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await db.Users.AddAsync(user, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        db.SaveChangesAsync(cancellationToken);
}
