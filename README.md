# .NET MCP Server

A minimal Model Context Protocol (MCP) server implementation using .NET 8, following Microsoft's documentation and best practices for building MCP servers.

## Overview

This project implements a minimal MCP server that can be connected to GitHub Copilot and other MCP clients. The server provides basic tools and demonstrates proper MCP protocol implementation with async/await patterns, error handling, and tool registration.

## Features

- **MCP Protocol Implementation**: Full JSON-RPC 2.0 based MCP protocol support
- **STDIO Transport**: Communication over standard input/output for client compatibility
- **Tool System**: Extensible tool registration and handling system
- **Built-in Tools**:
  - **Calculator**: Perform basic mathematical operations (add, subtract, multiply, divide)
  - **Echo**: Echo back text messages for testing
  - **Time**: Get current date and time in various formats
  - **PDF to PNG**: Convert PDF files to PNG images as base64 for Azure OpenAI GPT 4.1 vision/OCR input
- **Async/Await**: Proper asynchronous programming patterns
- **Error Handling**: Comprehensive error handling with proper MCP error responses
- **Logging**: Structured logging using Microsoft.Extensions.Logging

## Project Structure

```
McpServer/
├── Models/                 # MCP protocol message models
│   ├── McpMessage.cs      # Base message types (Request, Response, Notification)
│   ├── McpTool.cs         # Tool-related models
│   └── ServerCapabilities.cs # Server capabilities and initialization
├── Services/              # Core service implementations
│   ├── IMcpServer.cs      # MCP server interface
│   └── McpServerService.cs # Main MCP server implementation
├── Tools/                 # Example tool implementations
│   ├── CalculatorTool.cs  # Mathematical calculations
│   ├── EchoTool.cs        # Text echo functionality
│   ├── TimeTool.cs        # Date/time operations
│   └── PdfTool.cs         # PDF to PNG conversion
├── Transport/             # Communication transport layer
│   └── StdioTransport.cs  # STDIO transport implementation
└── Program.cs             # Application entry point and DI setup
```

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- Any MCP-compatible client (e.g., GitHub Copilot, Claude Desktop, etc.)

## Installation

1. Clone the repository:
   ```bash
   git clone https://github.com/aaronba/dotnet-mcp.git
   cd dotnet-mcp/McpServer
   ```

2. Build the project:
   ```bash
   dotnet build
   ```

3. Run the server:
   ```bash
   dotnet run
   ```

## Usage

### Quick Start in GitHub Codespaces

If you're using this in a GitHub Codespace, follow these steps:

1. **Open this repository in Codespaces**
2. **Build the project**:
   ```bash
   cd McpServer
   dotnet build
   ```
3. **Configure GitHub Copilot** to use this MCP server by creating/editing `~/.config/Code/User/settings.json`:
   ```json
   {
     "github.copilot.chat.experimental.mcp": {
       "enabled": true,
       "servers": {
         "dotnet-mcp": {
           "command": "/workspaces/dotnet-mcp/run-server.sh"
         }
       }
     }
   }
   ```
4. **Reload VS Code** (Cmd/Ctrl + Shift + P → "Developer: Reload Window")
5. **Test** by asking Copilot: "Use the calculator tool to add 5 and 3"

### Running the Server

The MCP server communicates over STDIO (standard input/output). When you run the server, it will wait for JSON-RPC messages on stdin and respond on stdout.

```bash
cd McpServer
dotnet run
```

### MCP Client Configuration

To connect this server to an MCP client, you'll typically need to configure the client with the command to start this server.

#### GitHub Copilot in Codespaces

To connect this MCP server to GitHub Copilot in a Codespace:

1. **Open your Codespace** with this repository
2. **Create MCP configuration**: Create or edit the MCP settings file at `~/.config/Code/User/settings.json` in your Codespace:

```json
{
  "github.copilot.chat.experimental.mcp": {
    "enabled": true,
    "servers": {
      "dotnet-mcp": {
        "command": "/workspaces/dotnet-mcp/run-server.sh"
      }
    }
  }
}
```

3. **Alternative configuration** using dotnet directly:

```json
{
  "github.copilot.chat.experimental.mcp": {
    "enabled": true,
    "servers": {
      "dotnet-mcp": {
        "command": "dotnet",
        "args": ["run", "--project", "/workspaces/dotnet-mcp/McpServer"],
        "cwd": "/workspaces/dotnet-mcp"
      }
    }
  }
}
```

4. **Reload VS Code** to pick up the new configuration
5. **Test the connection** by asking Copilot to use the calculator, echo, or time tools

#### Other MCP Clients

For other MCP clients, use this general configuration:

```json
{
  "servers": {
    "dotnet-mcp": {
      "command": "dotnet",
      "args": ["run", "--project", "/path/to/McpServer"],
      "env": {}
    }
  }
}
```

### Available Tools

#### Calculator Tool
Performs basic mathematical operations.

**Parameters:**
- `operation` (string): One of "add", "subtract", "multiply", "divide"
- `a` (number): First number
- `b` (number): Second number

**Example:**
```json
{
  "jsonrpc": "2.0",
  "id": "1",
  "method": "tools/call",
  "params": {
    "name": "calculator",
    "arguments": {
      "operation": "add",
      "a": 5,
      "b": 3
    }
  }
}
```

#### Echo Tool
Echoes back the provided text message.

**Parameters:**
- `message` (string): The message to echo back

**Example:**
```json
{
  "jsonrpc": "2.0",
  "id": "2",
  "method": "tools/call",
  "params": {
    "name": "echo",
    "arguments": {
      "message": "Hello, MCP!"
    }
  }
}
```

#### Time Tool
Gets current date and time information.

**Parameters:**
- `format` (string): One of "iso", "local", "utc", "timestamp"
- `timezone` (string, optional): Timezone identifier

**Example:**
```json
{
  "jsonrpc": "2.0",
  "id": "3",
  "method": "tools/call",
  "params": {
    "name": "time",
    "arguments": {
      "format": "iso"
    }
  }
}
```

#### PDF to PNG Tool
Converts PDF files to PNG images as base64 strings for use with Azure OpenAI GPT 4.1 vision/OCR input.

**Parameters:**
- `file_path` (string, optional): Path to the PDF file to convert
- `pdf_data` (string, optional): Base64 encoded PDF data
- `page_number` (number, optional): Page number to convert (1-based, defaults to 1)
- `quality` (number, optional): Image quality/DPI (defaults to 150)

**Note:** Either `file_path` or `pdf_data` must be provided.

**Example with file path:**
```json
{
  "jsonrpc": "2.0",
  "id": "4",
  "method": "tools/call",
  "params": {
    "name": "pdf_to_png",
    "arguments": {
      "file_path": "/path/to/document.pdf",
      "page_number": 1,
      "quality": 200
    }
  }
}
```

**Example with base64 PDF data:**
```json
{
  "jsonrpc": "2.0",
  "id": "5",
  "method": "tools/call",
  "params": {
    "name": "pdf_to_png",
    "arguments": {
      "pdf_data": "JVBERi0xLjQKJeLjz9MKMSAwIG9iago8PC9Qcm9kdWNlcihQREZsaWI...",
      "page_number": 1
    }
  }
}
```

## Development

### Adding New Tools

1. Create a new tool class in the `Tools/` directory that implements `IToolHandler`:

```csharp
public class MyTool : IToolHandler
{
    public static McpTool GetToolDefinition()
    {
        return new McpTool
        {
            Name = "my-tool",
            Description = "Description of what the tool does",
            InputSchema = new ToolInputSchema
            {
                // Define your tool's input schema
            }
        };
    }

    public async Task<ToolCallResult> HandleAsync(ToolCallParams toolCall, CancellationToken cancellationToken = default)
    {
        // Implement your tool logic
    }
}
```

2. Register the tool in `Program.cs`:

```csharp
builder.Services.AddSingleton<MyTool>();

// After building the host
var myTool = host.Services.GetRequiredService<MyTool>();
mcpServer.RegisterTool("my-tool", myTool.HandleAsync, MyTool.GetToolDefinition());
```

### Building and Testing

Build the project:
```bash
dotnet build
```

Run with verbose logging:
```bash
dotnet run --property:Configuration=Debug
```

### Dependencies

- **Microsoft.Extensions.Hosting** (9.0.7): For hosting and dependency injection
- **Microsoft.Extensions.Logging** (9.0.7): For structured logging
- **Microsoft.Extensions.Configuration** (9.0.7): For configuration management
- **System.Text.Json** (9.0.7): For JSON serialization
- **SkiaSharp** (3.119.0): For graphics and image processing
- **SkiaSharp.NativeAssets.Linux** (3.119.0): Linux native dependencies for SkiaSharp
- **Magick.NET-Q16-AnyCPU** (14.7.0): For PDF to image conversion using ImageMagick

## Architecture

The server follows these key design principles:

1. **Separation of Concerns**: Models, services, tools, and transport are separated into distinct layers
2. **Dependency Injection**: Uses Microsoft's built-in DI container for service management
3. **Async/Await**: All operations are asynchronous for better performance
4. **Error Handling**: Comprehensive error handling with proper MCP error codes
5. **Extensibility**: Easy to add new tools and capabilities

## MCP Protocol Support

This implementation supports the following MCP methods:

- `initialize`: Server initialization and capability negotiation
- `tools/list`: List available tools
- `tools/call`: Call a specific tool

The server implements MCP protocol version `2024-11-05`.

## Logging

The server uses structured logging with different log levels:

- **Information**: Server startup, tool registration, and tool calls
- **Debug**: Detailed request/response logging
- **Warning**: Non-critical issues (e.g., tool not found)
- **Error**: Exceptions and critical errors

## Contributing

1. Fork the repository
2. Create a feature branch
3. Add your changes with appropriate tests
4. Submit a pull request

## License

This project is provided as-is for educational and development purposes.

## References

- [Microsoft Learn: Build MCP Server](https://learn.microsoft.com/en-us/dotnet/ai/quickstarts/build-mcp-server)
- [Model Context Protocol Specification](https://spec.modelcontextprotocol.io/)
- [MCP Official Documentation](https://modelcontextprotocol.io/)