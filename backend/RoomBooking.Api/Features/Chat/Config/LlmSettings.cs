namespace RoomBooking.Api.Features.Chat.Config;

public sealed class GeminiSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gemini-3-flash-preview";
}

public sealed class GroqSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "llama-3.3-70b-versatile";
}

public sealed class OpenRouterSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "openai/gpt-4o-mini";
    public string HttpReferer { get; set; } = "http://localhost:5173";
    public string AppTitle { get; set; } = "RoomBooking";
}
