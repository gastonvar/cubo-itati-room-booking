namespace RoomBooking.Api.Features.Chat.Tools;

/// <summary>Stateless catalog of booking LLM tool schemas.</summary>
public static class BookingToolDefinitions
{
    public static IReadOnlyList<ToolDefinition> All { get; } =
    [
        new()
        {
            Name = "list_available_rooms",
            Description =
                "List rooms available for booking in a given time range with minimum capacity. " +
                "A room is unavailable if any user already has a booking overlapping that range. " +
                "When the user already named a room, prefer get_room_schedule for that room instead of picking a smaller room from this list. " +
                "Use today's date from the system prompt when the user omits a date. " +
                "If the start is slightly past, the tool clamps forward on the same day — it never moves the search to tomorrow. " +
                "If no rooms are free for the requested range, report that — do not invent another day. " +
                "Always read the message field for a human-readable summary of the result.",
            Properties = new Dictionary<string, ToolParam>
            {
                ["from_datetime"] = new(ToolParamType.String, "Start datetime in ISO 8601 format"),
                ["to_datetime"] = new(ToolParamType.String, "End datetime in ISO 8601 format"),
                ["attendees"] = new(ToolParamType.Integer, "Number of attendees")
            },
            Required = ["from_datetime", "to_datetime", "attendees"]
        },
        new()
        {
            Name = "get_room_schedule",
            Description =
                "Get the schedule (occupied slots and free slots) for a specific room in a date range. " +
                "Use this when the user named a room — check that room, do not switch to another. " +
                "Occupied slots include bookings from every user — any booking blocks the room. " +
                "If the requested slot is occupied, tell the user; do not book a different day without asking. " +
                "Always read the message field for a human-readable summary of the result.",
            Properties = new Dictionary<string, ToolParam>
            {
                ["room_code"] = new(ToolParamType.String,
                    "Exact room code the user requested (from the Rooms list in the system prompt)"),
                ["from_datetime"] = new(ToolParamType.String, "Start datetime in ISO 8601 format"),
                ["to_datetime"] = new(ToolParamType.String, "End datetime in ISO 8601 format")
            },
            Required = ["room_code", "from_datetime", "to_datetime"]
        },
        new()
        {
            Name = "list_my_bookings",
            Description =
                "List the current user's bookings. Use this to verify whether a booking exists " +
                "before claiming one was created, or when the user asks about their reservations. " +
                "Always read the message field for a human-readable summary of the result."
        },
        new()
        {
            Name = "create_booking",
            Description =
                "Create a new room booking AFTER the user has confirmed the summary. " +
                "Requires room_code, title, attendees, start, end, and user_confirmed=true. " +
                "room_code MUST be the room the user asked for (or explicitly accepted) — never substitute a smaller room. " +
                "start/end MUST be the exact slot the user confirmed — never invent another day/time because the requested one was busy. " +
                "First show the user room, title, attendees, start, and end; ask if they want to edit anything; " +
                "only call this tool after they explicitly confirm. " +
                "Start must be in the future using today's date from the system prompt unless the user named another date. " +
                "If the datetime is past or needs correction, this tool returns success:false — tell the user and ask for a valid time; do not create on a guessed day. " +
                "Returns success:true with a descriptive message and the booking (including id) on success, " +
                "or success:false with message/error explaining why. " +
                "Only tell the user the booking succeeded when success is true. " +
                "Do not call this tool until the user has provided a meeting title and confirmed.",
            Properties = new Dictionary<string, ToolParam>
            {
                ["room_code"] = new(ToolParamType.String,
                    "Exact room code the user requested or confirmed (do not swap for a smaller-capacity room)"),
                ["title"] = new(ToolParamType.String,
                    "Specific meeting title from the user (required). Never invent placeholders like \"Meeting\"."),
                ["attendees"] = new(ToolParamType.Integer, "Number of attendees"),
                ["start"] = new(ToolParamType.String,
                    "Exact start datetime the user confirmed (ISO 8601 with Montevideo offset); " +
                    "use today's date from the system prompt only when the user omitted a date"),
                ["end"] = new(ToolParamType.String,
                    "Exact end datetime the user confirmed (ISO 8601 with Montevideo offset); same calendar day as start"),
                ["user_confirmed"] = new(ToolParamType.Boolean,
                    "Must be true only after the user explicitly confirmed the booking summary in chat. " +
                    "Never set true in the same turn you first proposed the details.")
            },
            Required = ["room_code", "title", "attendees", "start", "end", "user_confirmed"]
        },
        new()
        {
            Name = "cancel_booking",
            Description =
                "Cancel an existing booking by its ID AFTER the user has confirmed. " +
                "First show the booking details (id, room, title, time) and ask for confirmation; " +
                "only call this tool with user_confirmed=true after they explicitly agree. " +
                "Returns success:true with a message when cancelled, or success:false with message/error if it failed.",
            Properties = new Dictionary<string, ToolParam>
            {
                ["booking_id"] = new(ToolParamType.Integer, "The booking ID to cancel"),
                ["user_confirmed"] = new(ToolParamType.Boolean,
                    "Must be true only after the user explicitly confirmed cancelling this booking in chat. " +
                    "Never set true before showing the booking details and getting a yes.")
            },
            Required = ["booking_id", "user_confirmed"]
        }
    ];
}
