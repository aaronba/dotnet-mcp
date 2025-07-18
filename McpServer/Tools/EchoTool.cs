using McpServer.Models;
using McpServer.Services;
using Microsoft.Extensions.Logging;

namespace McpServer.Tools;

/// <summary>
/// Echo tool that returns the input text
/// </summary>
public class EchoTool : IToolHandler
{
    private readonly ILogger<EchoTool> _logger;

    public EchoTool(ILogger<EchoTool> logger)
    {
        _logger = logger;
    }

    public static McpTool GetToolDefinition()
    {
        return new McpTool
        {
            Name = "echo",
            Description = "Echo back the provided text message",
            InputSchema = new ToolInputSchema
            {
                Type = "object",
                Properties = new Dictionary<string, ToolProperty>
                {
                    ["message"] = new ToolProperty
                    {
                        Type = "string",
                        Description = "The message to echo back"
                    }
                },
                Required = new List<string> { "message" }
            }
        };
    }

    public async Task<ToolCallResult> HandleAsync(ToolCallParams toolCall, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!toolCall.Arguments.TryGetValue("message", out var messageObj))
            {
                return new ToolCallResult
                {
                    IsError = true,
                    Content = new List<ToolContent>
                    {
                        new ToolContent
                        {
                            Type = "text",
                            Text = "Missing required parameter: message"
                        }
                    }
                };
            }

            var message = messageObj.ToString() ?? string.Empty;

            _logger.LogInformation("Echo: {Message}", message);

            return await Task.FromResult(new ToolCallResult
            {
                IsError = false,
                Content = new List<ToolContent>
                {
                    new ToolContent
                    {
                        Type = "text",
                        Text = $"Echo: {message}"
                    }
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in echo tool");
            return new ToolCallResult
            {
                IsError = true,
                Content = new List<ToolContent>
                {
                    new ToolContent
                    {
                        Type = "text",
                        Text = $"Echo error: {ex.Message}"
                    }
                }
            };
        }
    }
}