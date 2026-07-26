using RoomBooking.Api.Features.Chat.Llm;

namespace RoomBooking.Api.Features.Chat.Services;

public sealed class ChatOrchestrator(
    GeminiChatClient geminiChatClient,
    GroqChatClient groqChatClient,
    OpenRouterChatClient openRouterChatClient,
    ChatSystemPromptBuilder systemPromptBuilder,
    ILogger<ChatOrchestrator> logger) : IChatService
{
    public async Task<string> ChatAsync(
        List<ChatMessage> messages,
        string username,
        CancellationToken cancellationToken = default)
    {
        var systemPrompt = await systemPromptBuilder.BuildAsync(cancellationToken);

        var providers = new List<(string Name, Func<CancellationToken, Task<string>> Invoke)>();

        if (geminiChatClient.IsConfigured)
            providers.Add(("Gemini", ct => geminiChatClient.ChatAsync(messages, username, systemPrompt, ct)));

        if (groqChatClient.IsConfigured)
            providers.Add(("Groq", ct => groqChatClient.ChatAsync(messages, username, systemPrompt, ct)));

        if (openRouterChatClient.IsConfigured)
            providers.Add(("OpenRouter", ct => openRouterChatClient.ChatAsync(messages, username, systemPrompt, ct)));

        if (providers.Count == 0)
        {
            throw new LlmException(
                "No LLM provider configured (set Gemini__ApiKey, Groq__ApiKey, or OpenRouter__ApiKey)",
                503);
        }

        // Each provider starts fresh with the same user/assistant history from the client.
        // Mid-request tool loops are not carried across providers — only the original messages are.
        Exception? lastError = null;
        for (var i = 0; i < providers.Count; i++)
        {
            var (name, invoke) = providers[i];
            var isLast = i == providers.Count - 1;

            try
            {
                if (i > 0)
                    logger.LogInformation("Using {Provider} as chat provider", name);
                return await invoke(cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (LlmException ex) when (!isLast && ex.StatusCode is 429 or 502 or 503)
            {
                logger.LogWarning(ex, "{Provider} failed ({Status}); trying next provider", name, ex.StatusCode);
                lastError = ex;
            }
            catch (Exception ex) when (!isLast)
            {
                logger.LogWarning(ex, "{Provider} failed unexpectedly; trying next provider", name);
                lastError = ex;
            }
        }

        throw lastError is LlmException llm
            ? llm
            : new LlmException("All configured LLM providers failed", 502);
    }
}
