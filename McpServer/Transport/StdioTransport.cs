using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using McpServer.Models;
using McpServer.Services;
using System.Text.Json;

namespace McpServer.Transport;

/// <summary>
/// STDIO transport for MCP communication
/// </summary>
public class StdioTransport : BackgroundService
{
    private readonly ILogger<StdioTransport> _logger;
    private readonly IMcpServer _mcpServer;
    private readonly JsonSerializerOptions _jsonOptions;

    public StdioTransport(ILogger<StdioTransport> logger, IMcpServer mcpServer)
    {
        _logger = logger;
        _mcpServer = mcpServer;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            PropertyNameCaseInsensitive = true
        };
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting MCP STDIO transport");

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var line = await Console.In.ReadLineAsync();
                if (line == null)
                {
                    _logger.LogInformation("STDIN closed, shutting down");
                    break;
                }

                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                _logger.LogDebug("Received: {Line}", line);

                try
                {
                    var request = JsonSerializer.Deserialize<McpRequest>(line, _jsonOptions);
                    if (request == null)
                    {
                        _logger.LogWarning("Failed to deserialize request: {Line}", line);
                        continue;
                    }

                    _logger.LogInformation("Processing request: {Method} (ID: {RequestId})", request.Method, request.Id);

                    var response = await _mcpServer.ProcessRequestAsync(request, stoppingToken);
                    var responseJson = JsonSerializer.Serialize(response, _jsonOptions);

                    await Console.Out.WriteLineAsync(responseJson);
                    await Console.Out.FlushAsync();

                    _logger.LogDebug("Sent: {Response}", responseJson);
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "JSON error processing request: {Line}", line);
                    
                    var errorResponse = new McpResponse
                    {
                        Id = "",
                        Error = new McpError
                        {
                            Code = -32700,
                            Message = "Parse error",
                            Data = ex.Message
                        }
                    };

                    var errorJson = JsonSerializer.Serialize(errorResponse, _jsonOptions);
                    await Console.Out.WriteLineAsync(errorJson);
                    await Console.Out.FlushAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error processing request: {Line}", line);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in STDIO transport");
        }
        finally
        {
            _logger.LogInformation("MCP STDIO transport stopped");
        }
    }
}