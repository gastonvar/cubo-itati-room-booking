using RoomBooking.Api.Features.Auth.Entities;
using RoomBooking.Api.Features.Auth.Repositories;
using RoomBooking.Api.Shared.Security;

namespace RoomBooking.Api.Features.Auth.Services;

public sealed class AuthService(
    IUserRepository users,
    IRefreshTokenRepository refreshTokens,
    JwtTokenService jwt) : IAuthService
{
    public async Task<(IssuedTokens? Result, string? Error)> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await users.GetByUsernameAsync(request.Username, cancellationToken);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return (null, "Invalid username or password");

        var tokens = await IssueTokensAsync(user.Username, cancellationToken);
        return (tokens, null);
    }

    public async Task<(IssuedTokens? Result, string? Error)> RefreshAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return (null, "Invalid refresh token");

        var hash = JwtTokenService.HashRefreshToken(refreshToken);
        var existing = await refreshTokens.FindActiveByHashAsync(hash, cancellationToken);
        if (existing is null)
            return (null, "Invalid refresh token");

        // Rotate: revoke the presented token, then issue a new pair.
        existing.RevokedAt = DateTimeOffset.UtcNow;
        await refreshTokens.SaveChangesAsync(cancellationToken);

        var tokens = await IssueTokensAsync(existing.Username, cancellationToken);
        return (tokens, null);
    }

    public async Task LogoutAsync(string? refreshToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return;

        var hash = JwtTokenService.HashRefreshToken(refreshToken);
        var existing = await refreshTokens.FindActiveByHashAsync(hash, cancellationToken);
        if (existing is null)
            return;

        existing.RevokedAt = DateTimeOffset.UtcNow;
        await refreshTokens.SaveChangesAsync(cancellationToken);
    }

    private async Task<IssuedTokens> IssueTokensAsync(
        string username,
        CancellationToken cancellationToken)
    {
        var accessToken = jwt.GenerateAccessToken(username);
        var refreshToken = jwt.GenerateRefreshToken();

        await refreshTokens.AddAsync(
            new RefreshToken
            {
                Username = username,
                TokenHash = JwtTokenService.HashRefreshToken(refreshToken),
                CreatedAt = DateTimeOffset.UtcNow,
                ExpiresAt = DateTimeOffset.UtcNow.Add(JwtTokenService.RefreshTokenLifetime),
            },
            cancellationToken);
        await refreshTokens.SaveChangesAsync(cancellationToken);

        return new IssuedTokens(accessToken, refreshToken, username);
    }
}
