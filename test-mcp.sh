#!/bin/bash

cd McpServer

# Start the server in the background
dotnet run &
SERVER_PID=$!

# Give the server time to start
sleep 2

# Test initialization
echo '{"jsonrpc": "2.0", "id": "1", "method": "initialize", "params": {"protocolVersion": "2024-11-05", "capabilities": {}, "clientInfo": {"name": "test-client", "version": "1.0.0"}}}' > /dev/stdin

# Wait a bit for response
sleep 1

# Test tools list
echo '{"jsonrpc": "2.0", "id": "2", "method": "tools/list", "params": {}}' > /dev/stdin

# Wait for response
sleep 1

# Kill the server
kill $SERVER_PID

echo "Test completed"
