namespace RoomBooking.Api.Features.Auth.Services;

public record LoginRequest(string Username, string Password);

public record AuthUserResponse(string Username);

public record IssuedTokens(string AccessToken, string RefreshToken, string Username);

public interface IAuthService
{
    Task<(IssuedTokens? Result, string? Error)> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default);

    Task<(IssuedTokens? Result, string? Error)> RefreshAsync(
        string refreshToken,
        CancellationToken cancellationToken = default);

    Task LogoutAsync(string? refreshToken, CancellationToken cancellationToken = default);
}
