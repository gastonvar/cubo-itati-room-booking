using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoomBooking.Api.Features.Auth.Services;
using RoomBooking.Api.Shared.Http;
using RoomBooking.Api.Shared.Security;

namespace RoomBooking.Api.Features.Auth.Controllers;

[ApiController]
[Route("auth")]
public sealed class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var (result, error) = await authService.LoginAsync(request, cancellationToken);

        if (result is null)
            return Unauthorized(ApiResponse<AuthUserResponse>.Fail(error ?? "Invalid credentials"));

        AuthCookies.SetTokens(Response, result.AccessToken, result.RefreshToken, Request.IsHttps);
        return Ok(ApiResponse<AuthUserResponse>.Ok(new AuthUserResponse(result.Username)));
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(CancellationToken cancellationToken)
    {
        var refreshToken = AuthCookies.GetRefreshToken(Request);
        var (result, error) = await authService.RefreshAsync(refreshToken ?? string.Empty, cancellationToken);

        if (result is null)
        {
            AuthCookies.Clear(Response, Request.IsHttps);
            return Unauthorized(ApiResponse<AuthUserResponse>.Fail(error ?? "Invalid refresh token"));
        }

        AuthCookies.SetTokens(Response, result.AccessToken, result.RefreshToken, Request.IsHttps);
        return Ok(ApiResponse<AuthUserResponse>.Ok(new AuthUserResponse(result.Username)));
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        await authService.LogoutAsync(AuthCookies.GetRefreshToken(Request), cancellationToken);
        AuthCookies.Clear(Response, Request.IsHttps);
        return Ok(ApiResponse<object>.Ok(new { revoked = true }));
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        if (!CurrentUser.TryGetUsername(User, out var username))
            return Unauthorized(ApiResponse<AuthUserResponse>.Fail("Not authenticated"));

        return Ok(ApiResponse<AuthUserResponse>.Ok(new AuthUserResponse(username)));
    }
}
