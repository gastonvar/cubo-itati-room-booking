using System.Text.Json;
using Google.GenAI.Types;
using RoomBooking.Api.Features.Bookings.Services;
using RoomBooking.Api.Features.Chat.Tools;
using RoomBooking.Api.Features.Rooms.Services;
using RoomBooking.Api.Shared.Time;

namespace RoomBooking.Api.Features.Chat.Services;

/// <summary>Chat LLM booking tools: schemas + ExecuteAsync dispatch.</summary>
public sealed class ChatBookingTools(
    IRoomService roomService,
    IBookingService bookingService,
    IToolDateTimeNormalizer dateTimeNormalizer,
    IBookingClock clock,
    ILogger<ChatBookingTools> logger)
{
    public IReadOnlyList<ToolDefinition> Definitions { get; } = BookingToolDefinitions.All;

    public Tool GetToolDeclarations() => new()
    {
        FunctionDeclarations = Definitions.Select(d => d.ToGemini()).ToList()
    };

    public object[] GetOpenAiToolDefinitions() =>
        Definitions.Select(d => d.ToOpenAi()).ToArray();

    public async Task<object> ExecuteAsync(
        string functionName,
        JsonElement args,
        string username,
        CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation(
                "Chat tool {Tool} args for {User}: {Args}",
                functionName,
                username,
                args.ToString());

            var result = functionName switch
            {
                "list_available_rooms" => await ListAvailableRoomsAsync(args, cancellationToken),
                "get_room_schedule" => await GetRoomScheduleAsync(args, cancellationToken),
                "list_my_bookings" => await ListMyBookingsAsync(username, cancellationToken),
                "create_booking" => await CreateBookingAsync(args, username, cancellationToken),
                "cancel_booking" => await CancelBookingAsync(args, username, cancellationToken),
                _ => ToolResult.Fail($"Unknown function: {functionName}. Use only the declared booking tools.")
            };

            logger.LogInformation(
                "Chat tool {Tool} for {User}: {Result}",
                functionName,
                username,
                JsonSerializer.Serialize(result));
            return result;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Chat tool {Tool} failed for {User}", functionName, username);
            return ToolResult.Fail($"Tool {functionName} failed unexpectedly: {ex.Message}");
        }
    }

    private async Task<object> ListAvailableRoomsAsync(JsonElement args, CancellationToken cancellationToken)
    {
        if (!ToolArgs.TryRequireDateTime(
                args, "from_datetime", dateTimeNormalizer, out var from, out var error))
            return error;
        if (!ToolArgs.TryRequireDateTime(
                args, "to_datetime", dateTimeNormalizer, out var to, out error))
            return error;
        if (!ToolArgs.TryRequireInt(args, "attendees", out var attendees, out error))
            return error;

        // Never roll availability to another day (e.g. 10:30 today past → tomorrow).
        // Clamp same-day past starts forward; reject ranges that are entirely past.
        var (snappedFrom, snappedTo, adjustmentNote, rejected) =
            dateTimeNormalizer.NormalizeAvailabilityRange(from, to);
        if (rejected)
            return ToolResult.Fail(adjustmentNote!);

        var (available, serviceError) = await roomService.GetAvailableAsync(
            snappedFrom, snappedTo, attendees, cancellationToken);
        if (available is null)
            return ToolResult.Fail($"Could not list available rooms: {serviceError}");

        var roomList = available.Select(r => new { code = r.Code, capacity = r.Capacity }).ToList();
        var range =
            $"{clock.FormatLocal(snappedFrom)}–{clock.FormatLocal(snappedTo, "HH:mm")} Montevideo";
        var message = roomList.Count == 0
            ? $"No rooms are available for {attendees} attendees between {range}. " +
              "Inform the user this slot is unavailable; do not invent another day or create a booking elsewhere."
            : $"Found {roomList.Count} room(s) available for {attendees} attendees between {range}: " +
              string.Join(", ", roomList.Select(r => $"{r.code} (capacity {r.capacity})")) + ". " +
              "If the user named a specific room, use that room when it appears here — do not switch to a smaller room.";

        if (adjustmentNote is not null)
            message += $" Note: {adjustmentNote}";

        return new
        {
            success = true,
            message,
            from = clock.ToLocal(snappedFrom),
            to = clock.ToLocal(snappedTo),
            datetime_adjustment = adjustmentNote,
            rooms = roomList
        };
    }

    private async Task<object> GetRoomScheduleAsync(JsonElement args, CancellationToken cancellationToken)
    {
        if (!ToolArgs.TryRequireString(args, "room_code", out var code, out var error))
            return error;
        if (!ToolArgs.TryRequireDateTime(
                args, "from_datetime", dateTimeNormalizer, out var from, out error))
            return error;
        if (!ToolArgs.TryRequireDateTime(
                args, "to_datetime", dateTimeNormalizer, out var to, out error))
            return error;

        var (snappedFrom, snappedTo, adjustmentNote) =
            dateTimeNormalizer.SnapPastCalendarToToday(from, to);
        var localFrom = clock.ToLocal(snappedFrom);
        var localTo = clock.ToLocal(snappedTo);
        var fromDate = DateOnly.FromDateTime(localFrom.DateTime);
        var toLocalDate = DateOnly.FromDateTime(localTo.DateTime);
        var toDateExclusive = TimeOnly.FromDateTime(localTo.DateTime) == TimeOnly.MinValue
            && toLocalDate > fromDate
                ? toLocalDate
                : toLocalDate.AddDays(1);

        var (schedule, scheduleError) = await roomService.GetScheduleAsync(
            code, fromDate, toDateExclusive, cancellationToken);
        if (schedule is null)
            return ToolResult.Fail($"Could not get schedule for room {code}: {scheduleError}");

        var occupied = schedule.Occupied.Select(o => new
        {
            start = o.Start,
            end = o.End,
            title = o.Title,
            owner = o.Owner,
            attendees = o.Attendees
        }).ToList();
        var free = schedule.FreeSlots.Select(f => new { start = f.Start, end = f.End }).ToList();

        var range =
            $"{clock.FormatLocal(snappedFrom)}–{clock.FormatLocal(snappedTo)} Montevideo";
        var message =
            $"Schedule for room {schedule.RoomCode} from {range}: " +
            $"{occupied.Count} occupied slot(s), {free.Count} free slot(s).";
        if (adjustmentNote is not null)
            message += $" Note: {adjustmentNote}";

        return new
        {
            success = true,
            message,
            roomCode = schedule.RoomCode,
            from = clock.ToLocal(snappedFrom),
            to = clock.ToLocal(snappedTo),
            datetime_adjustment = adjustmentNote,
            occupied,
            free
        };
    }

    private async Task<object> ListMyBookingsAsync(string username, CancellationToken cancellationToken)
    {
        var bookings = await bookingService.ListByOwnerAsync(username, cancellationToken);
        var list = bookings.Select(b => new
        {
            id = b.Id,
            roomCode = b.RoomCode,
            title = b.Title,
            attendees = b.Attendees,
            start = b.Start,
            end = b.End
        }).ToList();

        var message = list.Count == 0
            ? $"{username} has no bookings."
            : $"{username} has {list.Count} booking(s): " +
              string.Join("; ", list.Select(b =>
                  $"id={b.id} room {b.roomCode} \"{b.title}\" " +
                  $"{clock.FormatLocal(b.start)}–{clock.FormatLocal(b.end, "HH:mm")}"));

        return new
        {
            success = true,
            message,
            bookings = list
        };
    }

    private async Task<object> CreateBookingAsync(
        JsonElement args,
        string username,
        CancellationToken cancellationToken)
    {
        if (!ToolArgs.TryRequireBool(args, "user_confirmed", out var userConfirmed, out var error))
            return ToolResult.Fail(
                "user_confirmed is required. Show the booking summary (room, title, attendees, start, end), " +
                "ask the user to confirm or edit, then call again with user_confirmed=true only after they agree.");

        if (!userConfirmed)
            return ToolResult.Fail(
                "Booking was not created because user_confirmed is false. " +
                "Show the summary and wait for the user to confirm before calling create_booking.");

        if (!ToolArgs.TryRequireString(args, "room_code", out var roomCode, out error))
            return error;

        if (!ToolArgs.TryRequireString(args, "title", out var title, out error))
            return ToolResult.Fail("title is required — ask the user for a meeting title before creating a booking");

        if (!ToolArgs.TryRequireInt(args, "attendees", out var attendees, out error))
            return error;

        if (!ToolArgs.TryRequireDateTime(
                args, "start", dateTimeNormalizer, out var start, out error))
            return error;

        if (!ToolArgs.TryRequireDateTime(
                args, "end", dateTimeNormalizer, out var end, out error))
            return error;

        // Do not silently roll past/wrong-year datetimes into another day and create the booking.
        // Tell the model so it can inform the user instead of guessing a new slot.
        var (snappedStart, snappedEnd, adjustmentNote) =
            dateTimeNormalizer.SnapRangeToFuture(start, end);
        if (adjustmentNote is not null)
        {
            return ToolResult.Fail(
                $"The requested start/end is in the past or needed correction " +
                $"({clock.FormatLocal(start)}–{clock.FormatLocal(end, "HH:mm")} → " +
                $"suggested {clock.FormatLocal(snappedStart)}–{clock.FormatLocal(snappedEnd, "HH:mm")} Montevideo). " +
                "Do NOT create a booking on a guessed day. Tell the user the requested time was not valid " +
                "and ask which date/time they want (you may mention the suggested slot as an option only).");
        }

        var result = await bookingService.CreateBookingAsync(
            roomCode, title, attendees, start, end, username, cancellationToken);

        if (result.Booking is not null)
        {
            var b = result.Booking;
            var message =
                $"Booking created successfully for {username}: id={b.Id}, room {b.RoomCode}, " +
                $"\"{b.Title}\", {b.Attendees} attendees, " +
                $"{clock.FormatLocal(b.Start)}–{clock.FormatLocal(b.End, "HH:mm")} Montevideo.";

            return new { success = true, message, booking = b };
        }

        return ToolResult.Fail(
            $"Booking was not created: {result.Error!.Message}. " +
            "Tell the user this exact slot/room failed. Do not invent another day or room and create it without asking.");
    }

    private async Task<object> CancelBookingAsync(
        JsonElement args,
        string username,
        CancellationToken cancellationToken)
    {
        if (!ToolArgs.TryRequireBool(args, "user_confirmed", out var userConfirmed, out var error))
            return ToolResult.Fail(
                "user_confirmed is required. Show the booking details (id, room, title, time), " +
                "ask the user to confirm cancellation, then call again with user_confirmed=true only after they agree.");

        if (!userConfirmed)
            return ToolResult.Fail(
                "Booking was not cancelled because user_confirmed is false. " +
                "Show the booking details and wait for the user to confirm before calling cancel_booking.");

        if (!ToolArgs.TryRequireInt(args, "booking_id", out var id, out error))
            return error;

        var result = await bookingService.CancelBookingAsync(id, username, cancellationToken);
        if (result.Success)
            return ToolResult.Ok($"Booking {id} was cancelled successfully for {username}.");

        return ToolResult.Fail($"Could not cancel booking {id}: {result.Error!.Message}");
    }
}
