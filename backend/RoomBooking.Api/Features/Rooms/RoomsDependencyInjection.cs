using RoomBooking.Api.Features.Rooms.Repositories;
using RoomBooking.Api.Features.Rooms.Services;

namespace RoomBooking.Api.Features.Rooms;

public static class RoomsDependencyInjection
{
    public static IServiceCollection AddRoomsFeature(this IServiceCollection services)
    {
        services.AddScoped<IRoomRepository, RoomRepository>();
        services.AddScoped<IRoomService, RoomService>();
        return services;
    }
}
