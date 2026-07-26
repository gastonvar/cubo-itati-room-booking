using System.Text.Json;

namespace RoomBooking.Api.Features.Chat.Tools;

internal static class ToolArgs
{
    public static bool TryRequireString(
        JsonElement args,
        string name,
        out string value,
        out object error)
    {
        value = "";
        error = ToolResult.Fail($"{name} is required");

        if (!args.TryGetProperty(name, out var el) || el.ValueKind != JsonValueKind.String)
            return false;

        var raw = el.GetString();
        if (string.IsNullOrWhiteSpace(raw))
            return false;

        value = raw.Trim();
        return true;
    }

    public static bool TryRequireInt(
        JsonElement args,
        string name,
        out int value,
        out object error)
    {
        value = 0;
        error = ToolResult.Fail($"{name} is required");

        if (!args.TryGetProperty(name, out var el) || !el.TryGetInt32(out value))
            return false;

        return true;
    }

    public static bool TryRequireBool(
        JsonElement args,
        string name,
        out bool value,
        out object error)
    {
        value = false;
        error = ToolResult.Fail($"{name} is required");

        if (!args.TryGetProperty(name, out var el))
            return false;

        if (el.ValueKind is JsonValueKind.True or JsonValueKind.False)
        {
            value = el.GetBoolean();
            return true;
        }

        return false;
    }

    public static bool TryRequireDateTime(
        JsonElement args,
        string name,
        IToolDateTimeNormalizer normalizer,
        out DateTimeOffset value,
        out object error)
    {
        value = default;
        error = ToolResult.Fail($"{name} is required");

        if (!args.TryGetProperty(name, out var el) || el.ValueKind != JsonValueKind.String
            || string.IsNullOrWhiteSpace(el.GetString()))
            return false;

        try
        {
            value = normalizer.Parse(el.GetString()!);
            return true;
        }
        catch (Exception ex)
        {
            error = ToolResult.Fail($"Invalid {name}: {ex.Message}");
            return false;
        }
    }
}
