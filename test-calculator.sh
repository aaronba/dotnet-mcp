#!/bin/bash

cd "$(dirname "$0")/McpServer"

echo "Testing MCP Calculator Tool..." >&2

# Test calculator tool
(
    echo '{"jsonrpc": "2.0", "id": "1", "method": "initialize", "params": {"protocolVersion": "2024-11-05", "capabilities": {}, "clientInfo": {"name": "test-client", "version": "1.0.0"}}}'
    sleep 0.5
    echo '{"jsonrpc": "2.0", "id": "2", "method": "tools/call", "params": {"name": "calculator", "arguments": {"operation": "add", "a": 5, "b": 3}}}'
    sleep 0.5
) | timeout 10s dotnet run 2>/dev/null
echo ""
