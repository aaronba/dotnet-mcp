#!/bin/bash

cd "$(dirname "$0")/McpServer"

echo "Testing MCP Server..." >&2

# Test 1: Initialize
echo "Test 1: Initialize server"
echo '{"jsonrpc": "2.0", "id": "1", "method": "initialize", "params": {"protocolVersion": "2024-11-05", "capabilities": {}, "clientInfo": {"name": "test-client", "version": "1.0.0"}}}' | dotnet run 2>/dev/null &
INIT_PID=$!
sleep 3
kill $INIT_PID 2>/dev/null || true
echo ""

# Test 2: List tools 
echo "Test 2: List tools"
(
    echo '{"jsonrpc": "2.0", "id": "1", "method": "initialize", "params": {"protocolVersion": "2024-11-05", "capabilities": {}, "clientInfo": {"name": "test-client", "version": "1.0.0"}}}'
    sleep 0.5
    echo '{"jsonrpc": "2.0", "id": "2", "method": "tools/list", "params": {}}'
    sleep 0.5
) | timeout 10s dotnet run 2>/dev/null
echo ""

echo "Test completed"
