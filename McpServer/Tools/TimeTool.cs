using McpServer.Models;
using McpServer.Services;
using Microsoft.Extensions.Logging;

namespace McpServer.Tools;

/// <summary>
/// Time tool for getting current date and time information
/// </summary>
public class TimeTool : IToolHandler
{
    private readonly ILogger<TimeTool> _logger;

    public TimeTool(ILogger<TimeTool> logger)
    {
        _logger = logger;
    }

    public static McpTool GetToolDefinition()
    {
        return new McpTool
        {
            Name = "time",
            Description = "Get current date and time information",
            InputSchema = new ToolInputSchema
            {
                Type = "object",
                Properties = new Dictionary<string, ToolProperty>
                {
                    ["format"] = new ToolProperty
                    {
                        Type = "string",
                        Description = "The format for the time output",
                        Enum = new List<string> { "iso", "local", "utc", "timestamp" }
                    },
                    ["timezone"] = new ToolProperty
                    {
                        Type = "string",
                        Description = "Timezone identifier (optional, defaults to local)"
                    }
                },
                Required = new List<string> { "format" }
            }
        };
    }

    public async Task<ToolCallResult> HandleAsync(ToolCallParams toolCall, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!toolCall.Arguments.TryGetValue("format", out var formatObj))
            {
                return new ToolCallResult
                {
                    IsError = true,
                    Content = new List<ToolContent>
                    {
                        new ToolContent
                        {
                            Type = "text",
                            Text = "Missing required parameter: format"
                        }
                    }
                };
            }

            var format = formatObj.ToString()?.ToLower();
            var now = DateTime.Now;
            var utcNow = DateTime.UtcNow;

            string result = format switch
            {
                "iso" => now.ToString("yyyy-MM-ddTHH:mm:ss.fffK"),
                "local" => now.ToString("F"),
                "utc" => utcNow.ToString("F") + " UTC",
                "timestamp" => DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                _ => throw new ArgumentException($"Unknown format: {format}")
            };

            _logger.LogInformation("Time requested in format: {Format}", format);

            return await Task.FromResult(new ToolCallResult
            {
                IsError = false,
                Content = new List<ToolContent>
                {
                    new ToolContent
                    {
                        Type = "text",
                        Text = $"Current time ({format}): {result}"
                    }
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in time tool");
            return new ToolCallResult
            {
                IsError = true,
                Content = new List<ToolContent>
                {
                    new ToolContent
                    {
                        Type = "text",
                        Text = $"Time error: {ex.Message}"
                    }
                }
            };
        }
    }
}