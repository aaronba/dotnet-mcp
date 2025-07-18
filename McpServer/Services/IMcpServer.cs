using McpServer.Models;

namespace McpServer.Services;

/// <summary>
/// Interface for MCP tool handlers
/// </summary>
public interface IToolHandler
{
    Task<ToolCallResult> HandleAsync(ToolCallParams toolCall, CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface for the MCP server
/// </summary>
public interface IMcpServer
{
    /// <summary>
    /// Initialize the server
    /// </summary>
    Task<InitializeResult> InitializeAsync(InitializeParams initParams, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get list of available tools
    /// </summary>
    Task<List<McpTool>> GetToolsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Call a tool
    /// </summary>
    Task<ToolCallResult> CallToolAsync(ToolCallParams toolCall, CancellationToken cancellationToken = default);

    /// <summary>
    /// Register a tool handler
    /// </summary>
    void RegisterTool(string toolName, Func<ToolCallParams, CancellationToken, Task<ToolCallResult>> handler, McpTool toolDefinition);

    /// <summary>
    /// Process an incoming MCP request
    /// </summary>
    Task<McpResponse> ProcessRequestAsync(McpRequest request, CancellationToken cancellationToken = default);
}