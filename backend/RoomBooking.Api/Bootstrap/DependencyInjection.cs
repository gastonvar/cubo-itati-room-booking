using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RoomBooking.Api.Features.Auth;
using RoomBooking.Api.Shared.Config;
using RoomBooking.Api.Shared.Data;
using RoomBooking.Api.Shared.Security;

namespace RoomBooking.Api.Bootstrap;

public static class DependencyInjection
{
    public static IServiceCollection AddControllersWithCamelCaseJson(this IServiceCollection services)
    {
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            });
        return services;
    }

    public static IServiceCollection AddAppDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connString = configuration.GetConnectionString("Default")
            ?? "Data Source=Data/roombooking.db;Cache=Shared;Default Timeout=30";
        services.AddDbContext<AppDbContext>(o => o.UseSqlite(connString));
        return services;
    }

    public static IServiceCollection AddAppAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));

        var jwtSecret = configuration["Jwt:Secret"] ?? "";
        if (jwtSecret.Length < 32)
            throw new InvalidOperationException(
                "Jwt__Secret must be set to at least 32 characters (see backend/.env.example).");

        services.AddSingleton<JwtTokenService>();
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(o =>
            {
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = JwtTokenService.Issuer,
                    ValidateAudience = true,
                    ValidAudience = JwtTokenService.Audience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                    ClockSkew = TimeSpan.FromMinutes(1)
                };
                o.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        // Prefer Authorization header (tests / API clients); otherwise httpOnly cookie.
                        if (string.IsNullOrEmpty(context.Token)
                            && context.Request.Cookies.TryGetValue(AuthCookies.AccessToken, out var cookieToken))
                        {
                            context.Token = cookieToken;
                        }

                        return Task.CompletedTask;
                    }
                };
            });
        services.AddAuthorization();
        return services;
    }

    public static IServiceCollection AddAppCors(this IServiceCollection services, IConfiguration configuration)
    {
        var corsOrigins = configuration["Cors:Origins"]
            ?? "http://localhost:5173;http://127.0.0.1:5173;http://localhost:5174;http://127.0.0.1:5174";
        var originList = corsOrigins.Split(
            [';', ','],
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        services.AddCors(o => o.AddDefaultPolicy(p =>
            p.WithOrigins(originList)
             .AllowAnyHeader()
             .AllowAnyMethod()
             .AllowCredentials()));
        return services;
    }
}
