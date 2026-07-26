using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using RoomBooking.Api.Features.Chat.Services;

namespace RoomBooking.Api.Features.Chat.Llm;

/// <summary>Shared OpenAI-compatible chat/completions + tool loop (Groq, OpenRouter, etc.).</summary>
public sealed class OpenAiCompatibleChatClient(
    IHttpClientFactory httpClientFactory,
    ChatBookingTools bookingTools,
    ILogger<OpenAiCompatibleChatClient> logger)
{
    private const int MaxToolRounds = 8;

    public async Task<string> ChatAsync(
        string httpClientName,
        string providerName,
        string apiKey,
        string model,
        List<ChatMessage> messages,
        string username,
        string systemPrompt,
        IReadOnlyDictionary<string, string>? extraHeaders = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new LlmException($"{providerName} API key is not configured", 503);

        var client = httpClientFactory.CreateClient(httpClientName);

        var conversation = new List<object>
        {
            new { role = "system", content = systemPrompt }
        };

        foreach (var message in messages)
        {
            conversation.Add(new
            {
                role = message.Role == "assistant" ? "assistant" : "user",
                content = message.Content
            });
        }

        var tools = bookingTools.GetOpenAiToolDefinitions();

        for (var round = 0; round < MaxToolRounds; round++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using var request = new HttpRequestMessage(HttpMethod.Post, "chat/completions");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            if (extraHeaders is not null)
            {
                foreach (var (key, value) in extraHeaders)
                {
                    if (!string.IsNullOrWhiteSpace(value))
                        request.Headers.TryAddWithoutValidation(key, value);
                }
            }

            var payload = new
            {
                model,
                messages = conversation,
                tools,
                tool_choice = "auto",
                temperature = 0.2
            };

            request.Content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json");

            using var response = await client.SendAsync(request, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if ((int)response.StatusCode == 429)
                throw new LlmException($"Rate limited by {providerName} API", 429);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "{Provider} API error {Status}: {Body}",
                    providerName,
                    (int)response.StatusCode,
                    Truncate(body));
                throw new LlmException($"{providerName} API error", 502);
            }

            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;
            var choice = root.GetProperty("choices")[0];
            var message = choice.GetProperty("message");

            conversation.Add(CloneMessageForHistory(message));

            if (!message.TryGetProperty("tool_calls", out var toolCalls)
                || toolCalls.ValueKind != JsonValueKind.Array
                || toolCalls.GetArrayLength() == 0)
            {
                if (message.TryGetProperty("content", out var content)
                    && content.ValueKind == JsonValueKind.String
                    && !string.IsNullOrWhiteSpace(content.GetString()))
                {
                    return content.GetString()!;
                }

                return "I'm sorry, I couldn't generate a response.";
            }

            foreach (var toolCall in toolCalls.EnumerateArray())
            {
                cancellationToken.ThrowIfCancellationRequested();

                var id = toolCall.GetProperty("id").GetString() ?? $"call_{round}";
                var function = toolCall.GetProperty("function");
                var name = function.GetProperty("name").GetString() ?? "";
                var argsRaw = function.TryGetProperty("arguments", out var argsEl)
                    ? argsEl.GetString() ?? "{}"
                    : "{}";

                object toolResult;
                try
                {
                    using var argsDoc = JsonDocument.Parse(argsRaw);
                    var argsJson = argsDoc.RootElement.Clone();
                    toolResult = await bookingTools.ExecuteAsync(name, argsJson, username, cancellationToken);
                }
                catch (JsonException ex)
                {
                    logger.LogWarning(ex, "{Provider} tool {Tool} returned invalid arguments JSON", providerName, name);
                    toolResult = new
                    {
                        success = false,
                        message = "Invalid tool arguments JSON — fix the arguments and try again.",
                        error = "Invalid tool arguments JSON"
                    };
                }

                conversation.Add(new
                {
                    role = "tool",
                    tool_call_id = id,
                    content = JsonSerializer.Serialize(toolResult)
                });
            }
        }

        return "I've reached the maximum number of tool calls. Please try a simpler request.";
    }

    private static Dictionary<string, object?> CloneMessageForHistory(JsonElement message)
    {
        var dict = new Dictionary<string, object?>
        {
            ["role"] = message.TryGetProperty("role", out var role) ? role.GetString() : "assistant"
        };

        if (message.TryGetProperty("content", out var content)
            && content.ValueKind == JsonValueKind.String)
        {
            dict["content"] = content.GetString();
        }

        if (message.TryGetProperty("tool_calls", out var toolCalls)
            && toolCalls.ValueKind == JsonValueKind.Array)
        {
            dict["tool_calls"] = JsonSerializer.Deserialize<object>(toolCalls.GetRawText());
        }

        return dict;
    }

    private static string Truncate(string value, int max = 300)
        => value.Length <= max ? value : value[..max] + "…";
}
