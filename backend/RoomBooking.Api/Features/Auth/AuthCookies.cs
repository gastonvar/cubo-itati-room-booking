using RoomBooking.Api.Shared.Security;

namespace RoomBooking.Api.Features.Auth;

public static class AuthCookies
{
    public const string AccessToken = "rb_access";
    public const string RefreshToken = "rb_refresh";

    public static void SetTokens(HttpResponse response, string accessToken, string refreshToken, bool secure)
    {
        response.Cookies.Append(AccessToken, accessToken, BuildOptions(secure, JwtTokenService.AccessTokenLifetime));
        response.Cookies.Append(RefreshToken, refreshToken, BuildOptions(secure, JwtTokenService.RefreshTokenLifetime));
    }

    public static void Clear(HttpResponse response, bool secure)
    {
        var options = BuildOptions(secure, maxAge: null);
        response.Cookies.Delete(AccessToken, options);
        response.Cookies.Delete(RefreshToken, options);
    }

    public static string? GetRefreshToken(HttpRequest request) =>
        request.Cookies.TryGetValue(RefreshToken, out var value) ? value : null;

    private static CookieOptions BuildOptions(bool secure, TimeSpan? maxAge) => new()
    {
        HttpOnly = true,
        // Local HTTP (localhost): Lax + Secure=false. HTTPS / cross-origin SPA: None + Secure.
        Secure = secure,
        SameSite = secure ? SameSiteMode.None : SameSiteMode.Lax,
        Path = "/",
        IsEssential = true,
        MaxAge = maxAge,
    };
}
