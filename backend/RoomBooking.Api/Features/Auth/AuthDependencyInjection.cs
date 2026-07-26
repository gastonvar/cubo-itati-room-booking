using RoomBooking.Api.Features.Auth.Repositories;
using RoomBooking.Api.Features.Auth.Services;

namespace RoomBooking.Api.Features.Auth;

public static class AuthDependencyInjection
{
    public static IServiceCollection AddAuthFeature(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IAuthService, AuthService>();
        return services;
    }
}
