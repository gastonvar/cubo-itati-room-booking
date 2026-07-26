using Google.GenAI.Types;
using GType = Google.GenAI.Types.Type;

namespace RoomBooking.Api.Features.Chat.Tools;

public enum ToolParamType
{
    String,
    Integer,
    Boolean
}

public sealed record ToolParam(ToolParamType Type, string Description);

/// <summary>Single source of truth for an LLM tool schema; maps to Gemini and OpenAI formats.</summary>
public sealed class ToolDefinition
{
    public required string Name { get; init; }
    public required string Description { get; init; }
    public Dictionary<string, ToolParam> Properties { get; init; } = new();
    public string[] Required { get; init; } = [];

    public FunctionDeclaration ToGemini() => new()
    {
        Name = Name,
        Description = Description,
        Parameters = new Schema
        {
            Type = GType.Object,
            Properties = Properties.ToDictionary(
                p => p.Key,
                p => new Schema
                {
                    Type = ToGeminiType(p.Value.Type),
                    Description = p.Value.Description
                }),
            Required = Required.Length > 0 ? Required.ToList() : null
        }
    };

    public object ToOpenAi() => new
    {
        type = "function",
        function = new
        {
            name = Name,
            description = Description,
            parameters = new
            {
                type = "object",
                properties = Properties.ToDictionary(
                    p => p.Key,
                    p => (object)new
                    {
                        type = ToOpenAiType(p.Value.Type),
                        description = p.Value.Description
                    }),
                required = Required
            }
        }
    };

    private static GType ToGeminiType(ToolParamType type) => type switch
    {
        ToolParamType.Integer => GType.Integer,
        ToolParamType.Boolean => GType.Boolean,
        _ => GType.String
    };

    private static string ToOpenAiType(ToolParamType type) => type switch
    {
        ToolParamType.Integer => "integer",
        ToolParamType.Boolean => "boolean",
        _ => "string"
    };
}
