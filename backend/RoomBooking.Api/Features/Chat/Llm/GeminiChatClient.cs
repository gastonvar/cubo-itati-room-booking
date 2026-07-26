using Google.GenAI;
using Google.GenAI.Types;
using Microsoft.Extensions.Options;
using RoomBooking.Api.Features.Chat.Services;
using RoomBooking.Api.Features.Chat.Config;
using System.Text.Json;

namespace RoomBooking.Api.Features.Chat.Llm;

public sealed class GeminiChatClient(
    IOptions<GeminiSettings> settings,
    ChatBookingTools bookingTools)
{
    private const int MaxToolRounds = 8;

    private readonly GeminiSettings _settings = settings.Value;

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_settings.ApiKey);

    public async Task<string> ChatAsync(
        List<ChatMessage> messages,
        string username,
        string systemPrompt,
        CancellationToken cancellationToken = default)
    {
        if (!IsConfigured)
            throw new LlmException("Gemini API key is not configured", 503);

        var client = new Client(apiKey: _settings.ApiKey);
        var model = string.IsNullOrWhiteSpace(_settings.Model)
            ? "gemini-3-flash-preview"
            : _settings.Model;

        var toolDecl = bookingTools.GetToolDeclarations();

        var config = new GenerateContentConfig
        {
            SystemInstruction = new Content
            {
                Parts = [new Part { Text = systemPrompt }]
            },
            Tools = [toolDecl],
            ThinkingConfig = new ThinkingConfig { ThinkingBudget = 0 }
        };

        var contents = messages.Select(m => new Content
        {
            Role = m.Role == "assistant" ? "model" : "user",
            Parts = [new Part { Text = m.Content }]
        }).ToList();

        for (var round = 0; round < MaxToolRounds; round++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            GenerateContentResponse response;
            try
            {
                response = await client.Models.GenerateContentAsync(
                    model: model,
                    contents: contents,
                    config: config);
            }
            catch (ClientError ex) when (ex.Message.Contains("429") || ex.Message.Contains("RESOURCE_EXHAUSTED"))
            {
                throw new LlmException("Rate limited by Gemini API", 429);
            }
            catch (ClientError)
            {
                throw new LlmException("Gemini API error", 502);
            }

            var candidate = response.Candidates?.FirstOrDefault();
            if (candidate?.Content?.Parts is null || candidate.Content.Parts.Count == 0)
                return "I'm sorry, I couldn't generate a response.";

            contents.Add(candidate.Content);

            var functionCalls = candidate.Content.Parts
                .Where(p => p.FunctionCall is not null)
                .ToList();

            if (functionCalls.Count == 0)
            {
                var textPart = candidate.Content.Parts
                    .FirstOrDefault(p => p.Text is not null && p.Thought is not true);
                return textPart?.Text ?? "I'm sorry, I couldn't generate a response.";
            }

            var functionResponseParts = new List<Part>();

            foreach (var part in functionCalls)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var fc = part.FunctionCall!;
                var argsJson = fc.Args is not null
                    ? JsonSerializer.SerializeToElement(fc.Args)
                    : JsonSerializer.SerializeToElement(new { });

                var result = await bookingTools.ExecuteAsync(fc.Name!, argsJson, username, cancellationToken);
                // GenAI FunctionResponse.Response expects Dictionary<string, object>; round-trip coerces anonymous tool results.
                var resultDict = JsonSerializer.Deserialize<Dictionary<string, object>>(
                    JsonSerializer.Serialize(result));

                functionResponseParts.Add(new Part
                {
                    FunctionResponse = new FunctionResponse
                    {
                        Name = fc.Name,
                        Response = resultDict,
                        Id = fc.Id
                    }
                });
            }

            contents.Add(new Content
            {
                Role = "user",
                Parts = functionResponseParts
            });
        }

        return "I've reached the maximum number of tool calls. Please try a simpler request.";
    }
}
