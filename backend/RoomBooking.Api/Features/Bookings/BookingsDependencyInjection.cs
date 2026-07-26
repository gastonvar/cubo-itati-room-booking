using RoomBooking.Api.Features.Bookings.Repositories;
using RoomBooking.Api.Features.Bookings.Services;

namespace RoomBooking.Api.Features.Bookings;

public static class BookingsDependencyInjection
{
    public static IServiceCollection AddBookingsFeature(this IServiceCollection services)
    {
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IBookingService, BookingService>();
        return services;
    }
}
