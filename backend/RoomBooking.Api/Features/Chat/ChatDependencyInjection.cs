using RoomBooking.Api.Features.Chat.Config;
using RoomBooking.Api.Features.Chat.Llm;
using RoomBooking.Api.Features.Chat.Services;
using RoomBooking.Api.Features.Chat.Tools;

namespace RoomBooking.Api.Features.Chat;

public static class ChatDependencyInjection
{
    public static IServiceCollection AddChatFeature(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<GeminiSettings>(configuration.GetSection("Gemini"));
        services.Configure<GroqSettings>(configuration.GetSection("Groq"));
        services.Configure<OpenRouterSettings>(configuration.GetSection("OpenRouter"));

        services.AddHttpClient(LlmHttpClients.Groq, client =>
        {
            client.BaseAddress = new Uri("https://api.groq.com/openai/v1/");
            client.Timeout = TimeSpan.FromSeconds(90);
        });
        services.AddHttpClient(LlmHttpClients.OpenRouter, client =>
        {
            client.BaseAddress = new Uri("https://openrouter.ai/api/v1/");
            client.Timeout = TimeSpan.FromSeconds(90);
        });

        services.AddScoped<ChatBookingTools>();
        services.AddScoped<IToolDateTimeNormalizer, ToolDateTimeNormalizer>();
        services.AddScoped<OpenAiCompatibleChatClient>();
        services.AddScoped<GroqChatClient>();
        services.AddScoped<OpenRouterChatClient>();
        services.AddScoped<GeminiChatClient>();
        services.AddScoped<ChatSystemPromptBuilder>();
        services.AddScoped<IChatService, ChatOrchestrator>();
        return services;
    }
}
