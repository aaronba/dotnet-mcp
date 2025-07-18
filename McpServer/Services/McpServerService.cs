using Microsoft.Extensions.Logging;
using McpServer.Models;
using System.Text.Json;

namespace McpServer.Services;

/// <summary>
/// Main MCP server implementation
/// </summary>
public class McpServerService : IMcpServer
{
    private readonly ILogger<McpServerService> _logger;
    private readonly Dictionary<string, Func<ToolCallParams, CancellationToken, Task<ToolCallResult>>> _toolHandlers;
    private readonly Dictionary<string, McpTool> _tools;
    private bool _initialized;

    public McpServerService(ILogger<McpServerService> logger)
    {
        _logger = logger;
        _toolHandlers = new Dictionary<string, Func<ToolCallParams, CancellationToken, Task<ToolCallResult>>>();
        _tools = new Dictionary<string, McpTool>();
        _initialized = false;
    }

    public async Task<InitializeResult> InitializeAsync(InitializeParams initParams, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Initializing MCP server with client: {ClientName} {ClientVersion}", 
            initParams.ClientInfo.Name, initParams.ClientInfo.Version);

        _initialized = true;

        var result = new InitializeResult
        {
            ProtocolVersion = "2024-11-05",
            ServerInfo = new ServerInfo
            {
                Name = "dotnet-mcp-server",
                Version = "1.0.0"
            },
            Capabilities = new ServerCapabilities
            {
                Tools = new ToolCapability
                {
                    ListChanged = false
                }
            }
        };

        return await Task.FromResult(result);
    }

    public async Task<List<McpTool>> GetToolsAsync(CancellationToken cancellationToken = default)
    {
        if (!_initialized)
        {
            throw new InvalidOperationException("Server not initialized");
        }

        _logger.LogDebug("Getting list of {ToolCount} available tools", _tools.Count);
        return await Task.FromResult(_tools.Values.ToList());
    }

    public async Task<ToolCallResult> CallToolAsync(ToolCallParams toolCall, CancellationToken cancellationToken = default)
    {
        if (!_initialized)
        {
            throw new InvalidOperationException("Server not initialized");
        }

        _logger.LogInformation("Calling tool: {ToolName}", toolCall.Name);

        if (!_toolHandlers.TryGetValue(toolCall.Name, out var handler))
        {
            _logger.LogWarning("Tool not found: {ToolName}", toolCall.Name);
            return new ToolCallResult
            {
                IsError = true,
                Content = new List<ToolContent>
                {
                    new ToolContent
                    {
                        Type = "text",
                        Text = $"Tool '{toolCall.Name}' not found"
                    }
                }
            };
        }

        try
        {
            return await handler(toolCall, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling tool: {ToolName}", toolCall.Name);
            return new ToolCallResult
            {
                IsError = true,
                Content = new List<ToolContent>
                {
                    new ToolContent
                    {
                        Type = "text",
                        Text = $"Error calling tool '{toolCall.Name}': {ex.Message}"
                    }
                }
            };
        }
    }

    public void RegisterTool(string toolName, Func<ToolCallParams, CancellationToken, Task<ToolCallResult>> handler, McpTool toolDefinition)
    {
        _logger.LogInformation("Registering tool: {ToolName}", toolName);
        _toolHandlers[toolName] = handler;
        _tools[toolName] = toolDefinition;
    }

    public async Task<McpResponse> ProcessRequestAsync(McpRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Processing request: {Method} (ID: {RequestId})", request.Method, request.Id);

        try
        {
            object? result = request.Method switch
            {
                "initialize" => await HandleInitializeAsync(request, cancellationToken),
                "tools/list" => await HandleToolsListAsync(cancellationToken),
                "tools/call" => await HandleToolCallAsync(request, cancellationToken),
                _ => throw new NotSupportedException($"Method '{request.Method}' not supported")
            };

            return new McpResponse
            {
                Id = request.Id,
                Result = result
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing request: {Method} (ID: {RequestId})", request.Method, request.Id);
            return new McpResponse
            {
                Id = request.Id,
                Error = new McpError
                {
                    Code = -32603,
                    Message = "Internal error",
                    Data = ex.Message
                }
            };
        }
    }

    private async Task<InitializeResult> HandleInitializeAsync(McpRequest request, CancellationToken cancellationToken)
    {
        if (request.Params == null)
        {
            throw new ArgumentException("Initialize request missing parameters");
        }

        var json = JsonSerializer.Serialize(request.Params);
        var initParams = JsonSerializer.Deserialize<InitializeParams>(json) 
            ?? throw new ArgumentException("Invalid initialize parameters");

        return await InitializeAsync(initParams, cancellationToken);
    }

    private async Task<Dictionary<string, List<McpTool>>> HandleToolsListAsync(CancellationToken cancellationToken)
    {
        var tools = await GetToolsAsync(cancellationToken);
        return new Dictionary<string, List<McpTool>>
        {
            ["tools"] = tools
        };
    }

    private async Task<ToolCallResult> HandleToolCallAsync(McpRequest request, CancellationToken cancellationToken)
    {
        if (request.Params == null)
        {
            throw new ArgumentException("Tool call request missing parameters");
        }

        var json = JsonSerializer.Serialize(request.Params);
        var toolCall = JsonSerializer.Deserialize<ToolCallParams>(json)
            ?? throw new ArgumentException("Invalid tool call parameters");

        return await CallToolAsync(toolCall, cancellationToken);
    }
}