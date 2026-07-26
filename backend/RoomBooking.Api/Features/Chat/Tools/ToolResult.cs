namespace RoomBooking.Api.Features.Chat.Tools;

/// <summary>Standard LLM tool payload helpers — always include a descriptive <c>message</c>.</summary>
internal static class ToolResult
{
    public static object Fail(string message) =>
        new { success = false, message, error = message };

    public static object Ok(string message) =>
        new { success = true, message };
}
