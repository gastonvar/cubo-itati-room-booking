namespace RoomBooking.Api.Shared.Http;

public sealed record ApiError(string Detail, string? Code = null);
