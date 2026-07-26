using System.IdentityModel.Tokens.Jwt;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;

namespace RoomBooking.Api.Shared.Security;

public static class CurrentUser
{
    public static bool TryGetUsername(ClaimsPrincipal user, [NotNullWhen(true)] out string? username)
    {
        username = user.FindFirstValue(JwtRegisteredClaimNames.Sub)
                   ?? user.FindFirstValue(ClaimTypes.NameIdentifier);
        return !string.IsNullOrWhiteSpace(username);
    }
}
