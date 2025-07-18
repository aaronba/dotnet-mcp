using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using McpServer.Services;
using McpServer.Tools;
using McpServer.Transport;

// Create the host builder
var builder = Host.CreateApplicationBuilder(args);

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Add services to the container
builder.Services.AddSingleton<IMcpServer, McpServerService>();
builder.Services.AddSingleton<CalculatorTool>();
builder.Services.AddSingleton<EchoTool>();
builder.Services.AddSingleton<TimeTool>();

// Add the STDIO transport as a hosted service
builder.Services.AddHostedService<StdioTransport>();

// Build the host
var host = builder.Build();

// Get the MCP server and register tools
var mcpServer = host.Services.GetRequiredService<IMcpServer>();
var calculatorTool = host.Services.GetRequiredService<CalculatorTool>();
var echoTool = host.Services.GetRequiredService<EchoTool>();
var timeTool = host.Services.GetRequiredService<TimeTool>();

// Register the tools with the MCP server
mcpServer.RegisterTool("calculator", calculatorTool.HandleAsync, CalculatorTool.GetToolDefinition());
mcpServer.RegisterTool("echo", echoTool.HandleAsync, EchoTool.GetToolDefinition());
mcpServer.RegisterTool("time", timeTool.HandleAsync, TimeTool.GetToolDefinition());

// Get logger
var logger = host.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Starting .NET MCP Server");

// Run the host
await host.RunAsync();
