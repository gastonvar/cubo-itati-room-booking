using RoomBooking.Api.Bootstrap;
using RoomBooking.Api.Features.Auth;
using RoomBooking.Api.Features.Bookings;
using RoomBooking.Api.Features.Chat;
using RoomBooking.Api.Features.Rooms;
using RoomBooking.Api.Shared.Time;

var builder = WebApplication.CreateBuilder(args);

EnvironmentConfiguration.Load(builder.Environment.ContentRootPath, builder.Configuration);

builder.Services.AddControllersWithCamelCaseJson();
builder.Services.AddForwardedHeadersSupport();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<IBookingClock, BookingClock>();
builder.Services.AddAppDatabase(builder.Configuration);
builder.Services.AddAppAuthentication(builder.Configuration);
builder.Services.AddAppCors(builder.Configuration);
builder.Services.AddAuthFeature();
builder.Services.AddRoomsFeature();
builder.Services.AddBookingsFeature();
builder.Services.AddChatFeature(builder.Configuration);

var app = builder.Build();

await DatabaseInitialization.InitializeAsync(app);
app.ConfigurePipeline();

app.Run();

public partial class Program;
