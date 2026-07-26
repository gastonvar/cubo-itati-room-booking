using Microsoft.EntityFrameworkCore;
using RoomBooking.Api.Features.Auth.Entities;
using RoomBooking.Api.Shared.Data;

namespace RoomBooking.Api.Features.Auth.Repositories;

public sealed class RefreshTokenRepository(AppDbContext db) : IRefreshTokenRepository
{
    public async Task<RefreshToken?> FindActiveByHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default)
    {
        // SQLite cannot reliably translate DateTimeOffset compares — filter expiry in-process.
        var token = await db.RefreshTokens
            .FirstOrDefaultAsync(
                t => t.TokenHash == tokenHash && t.RevokedAt == null,
                cancellationToken);

        if (token is null || token.ExpiresAt <= DateTimeOffset.UtcNow)
            return null;

        return token;
    }

    public async Task AddAsync(RefreshToken token, CancellationToken cancellationToken = default)
    {
        await db.RefreshTokens.AddAsync(token, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        db.SaveChangesAsync(cancellationToken);
}
