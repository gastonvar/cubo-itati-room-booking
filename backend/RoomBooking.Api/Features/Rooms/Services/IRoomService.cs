namespace RoomBooking.Api.Features.Rooms.Services;

public record RoomDto(string Code, int Capacity);
public record OccupiedSlot(DateTimeOffset Start, DateTimeOffset End, string Title, string Owner, int Attendees);
public record FreeSlot(DateTimeOffset Start, DateTimeOffset End);
public record ScheduleResponse(string RoomCode, List<OccupiedSlot> Occupied, List<FreeSlot> FreeSlots);

public interface IRoomService
{
    Task<List<RoomDto>> ListAsync(CancellationToken cancellationToken = default);
    Task<(List<RoomDto>? Rooms, string? Error)> GetAvailableAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        int attendees,
        CancellationToken cancellationToken = default);
    Task<(ScheduleResponse? Result, string? Error)> GetScheduleAsync(
        string code,
        DateOnly fromDate,
        DateOnly toDateExclusive,
        CancellationToken cancellationToken = default);
}
