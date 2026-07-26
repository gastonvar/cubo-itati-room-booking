using Microsoft.Extensions.Options;
using RoomBooking.Api.Features.Chat.Services;
using RoomBooking.Api.Features.Chat.Config;

namespace RoomBooking.Api.Features.Chat.Llm;

public sealed class OpenRouterChatClient(
    OpenAiCompatibleChatClient openAiChat,
    IOptions<OpenRouterSettings> settings)
{
    private readonly OpenRouterSettings _settings = settings.Value;

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_settings.ApiKey);

    public Task<string> ChatAsync(
        List<ChatMessage> messages,
        string username,
        string systemPrompt,
        CancellationToken cancellationToken = default)
    {
        var model = string.IsNullOrWhiteSpace(_settings.Model)
            ? "openai/gpt-4o-mini"
            : _settings.Model;

        var headers = new Dictionary<string, string>
        {
            ["HTTP-Referer"] = string.IsNullOrWhiteSpace(_settings.HttpReferer)
                ? "http://localhost:5173"
                : _settings.HttpReferer,
            ["X-Title"] = string.IsNullOrWhiteSpace(_settings.AppTitle)
                ? "RoomBooking"
                : _settings.AppTitle
        };

        return openAiChat.ChatAsync(
            httpClientName: LlmHttpClients.OpenRouter,
            providerName: "OpenRouter",
            apiKey: _settings.ApiKey,
            model: model,
            messages: messages,
            username: username,
            systemPrompt: systemPrompt,
            extraHeaders: headers,
            cancellationToken: cancellationToken);
    }
}
