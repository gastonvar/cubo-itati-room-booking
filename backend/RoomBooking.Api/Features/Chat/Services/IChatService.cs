namespace RoomBooking.Api.Features.Chat.Services;

public record ChatRequest(List<ChatMessage> Messages);
public record ChatResponse(string Reply);
public record ChatMessage(string Role, string Content);

public interface IChatService
{
    Task<string> ChatAsync(
        List<ChatMessage> messages,
        string username,
        CancellationToken cancellationToken = default);
}

public class LlmException(string message, int statusCode) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}
