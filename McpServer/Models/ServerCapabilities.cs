using System.Text.Json.Serialization;

namespace McpServer.Models;

/// <summary>
/// Server capabilities and information
/// </summary>
public class ServerInfo
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;
}

/// <summary>
/// Server capabilities
/// </summary>
public class ServerCapabilities
{
    [JsonPropertyName("tools")]
    public ToolCapability? Tools { get; set; }

    [JsonPropertyName("resources")]
    public ResourceCapability? Resources { get; set; }

    [JsonPropertyName("prompts")]
    public PromptCapability? Prompts { get; set; }
}

/// <summary>
/// Tool capability
/// </summary>
public class ToolCapability
{
    [JsonPropertyName("listChanged")]
    public bool ListChanged { get; set; } = false;
}

/// <summary>
/// Resource capability
/// </summary>
public class ResourceCapability
{
    [JsonPropertyName("subscribe")]
    public bool Subscribe { get; set; } = false;

    [JsonPropertyName("listChanged")]
    public bool ListChanged { get; set; } = false;
}

/// <summary>
/// Prompt capability
/// </summary>
public class PromptCapability
{
    [JsonPropertyName("listChanged")]
    public bool ListChanged { get; set; } = false;
}

/// <summary>
/// Initialize request parameters
/// </summary>
public class InitializeParams
{
    [JsonPropertyName("protocolVersion")]
    public string ProtocolVersion { get; set; } = string.Empty;

    [JsonPropertyName("capabilities")]
    public ClientCapabilities Capabilities { get; set; } = new();

    [JsonPropertyName("clientInfo")]
    public ClientInfo ClientInfo { get; set; } = new();
}

/// <summary>
/// Client capabilities
/// </summary>
public class ClientCapabilities
{
    [JsonPropertyName("roots")]
    public RootCapability? Roots { get; set; }

    [JsonPropertyName("sampling")]
    public SamplingCapability? Sampling { get; set; }
}

/// <summary>
/// Root capability
/// </summary>
public class RootCapability
{
    [JsonPropertyName("listChanged")]
    public bool ListChanged { get; set; } = false;
}

/// <summary>
/// Sampling capability
/// </summary>
public class SamplingCapability
{
}

/// <summary>
/// Client information
/// </summary>
public class ClientInfo
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;
}

/// <summary>
/// Initialize result
/// </summary>
public class InitializeResult
{
    [JsonPropertyName("protocolVersion")]
    public string ProtocolVersion { get; set; } = "2024-11-05";

    [JsonPropertyName("capabilities")]
    public ServerCapabilities Capabilities { get; set; } = new();

    [JsonPropertyName("serverInfo")]
    public ServerInfo ServerInfo { get; set; } = new();
}