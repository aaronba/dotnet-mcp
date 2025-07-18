using McpServer.Models;
using McpServer.Services;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace McpServer.Tools;

/// <summary>
/// Calculator tool for basic mathematical operations
/// </summary>
public class CalculatorTool : IToolHandler
{
    private readonly ILogger<CalculatorTool> _logger;

    public CalculatorTool(ILogger<CalculatorTool> logger)
    {
        _logger = logger;
    }

    public static McpTool GetToolDefinition()
    {
        return new McpTool
        {
            Name = "calculator",
            Description = "Perform basic mathematical calculations (add, subtract, multiply, divide)",
            InputSchema = new ToolInputSchema
            {
                Type = "object",
                Properties = new Dictionary<string, ToolProperty>
                {
                    ["operation"] = new ToolProperty
                    {
                        Type = "string",
                        Description = "The mathematical operation to perform",
                        Enum = new List<string> { "add", "subtract", "multiply", "divide" }
                    },
                    ["a"] = new ToolProperty
                    {
                        Type = "number",
                        Description = "First number"
                    },
                    ["b"] = new ToolProperty
                    {
                        Type = "number",
                        Description = "Second number"
                    }
                },
                Required = new List<string> { "operation", "a", "b" }
            }
        };
    }

    public async Task<ToolCallResult> HandleAsync(ToolCallParams toolCall, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!toolCall.Arguments.TryGetValue("operation", out var operationObj) ||
                !toolCall.Arguments.TryGetValue("a", out var aObj) ||
                !toolCall.Arguments.TryGetValue("b", out var bObj))
            {
                return new ToolCallResult
                {
                    IsError = true,
                    Content = new List<ToolContent>
                    {
                        new ToolContent
                        {
                            Type = "text",
                            Text = "Missing required parameters: operation, a, b"
                        }
                    }
                };
            }

            var operation = operationObj.ToString();
            var a = GetDoubleValue(aObj);
            var b = GetDoubleValue(bObj);

            double result = operation?.ToLower() switch
            {
                "add" => a + b,
                "subtract" => a - b,
                "multiply" => a * b,
                "divide" => b != 0 ? a / b : throw new DivideByZeroException("Cannot divide by zero"),
                _ => throw new ArgumentException($"Unknown operation: {operation}")
            };

            _logger.LogInformation("Calculator: {A} {Operation} {B} = {Result}", a, operation, b, result);

            return await Task.FromResult(new ToolCallResult
            {
                IsError = false,
                Content = new List<ToolContent>
                {
                    new ToolContent
                    {
                        Type = "text",
                        Text = $"Result: {a} {operation} {b} = {result}"
                    }
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in calculator tool");
            return new ToolCallResult
            {
                IsError = true,
                Content = new List<ToolContent>
                {
                    new ToolContent
                    {
                        Type = "text",
                        Text = $"Calculation error: {ex.Message}"
                    }
                }
            };
        }
    }

    private static double GetDoubleValue(object value)
    {
        return value switch
        {
            JsonElement element when element.ValueKind == JsonValueKind.Number => element.GetDouble(),
            double d => d,
            int i => i,
            long l => l,
            float f => f,
            string s when double.TryParse(s, out var parsed) => parsed,
            _ => Convert.ToDouble(value)
        };
    }
}