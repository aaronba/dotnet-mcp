#!/bin/bash

# .NET MCP Server Launch Script
# This script runs the .NET MCP Server with proper error handling

cd "$(dirname "$0")/McpServer"

echo "Starting .NET MCP Server..." >&2

# Check if .NET is installed
if ! command -v dotnet &> /dev/null; then
    echo "Error: .NET is not installed or not in PATH" >&2
    exit 1
fi

# Check if the project exists
if [ ! -f "McpServer.csproj" ]; then
    echo "Error: McpServer.csproj not found in current directory" >&2
    exit 1
fi

# Run the server
exec dotnet run --configuration Release