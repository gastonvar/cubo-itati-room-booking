using Microsoft.Extensions.Options;
using RoomBooking.Api.Features.Chat.Services;
using RoomBooking.Api.Features.Chat.Config;

namespace RoomBooking.Api.Features.Chat.Llm;

public sealed class GroqChatClient(
    OpenAiCompatibleChatClient openAiChat,
    IOptions<GroqSettings> settings)
{
    private readonly GroqSettings _settings = settings.Value;

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_settings.ApiKey);

    public Task<string> ChatAsync(
        List<ChatMessage> messages,
        string username,
        string systemPrompt,
        CancellationToken cancellationToken = default)
    {
        var model = string.IsNullOrWhiteSpace(_settings.Model)
            ? "llama-3.3-70b-versatile"
            : _settings.Model;

        return openAiChat.ChatAsync(
            httpClientName: LlmHttpClients.Groq,
            providerName: "Groq",
            apiKey: _settings.ApiKey,
            model: model,
            messages: messages,
            username: username,
            systemPrompt: systemPrompt,
            cancellationToken: cancellationToken);
    }
}
