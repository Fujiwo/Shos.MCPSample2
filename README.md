# Shos.MCPSample2

A comprehensive sample solution demonstrating the **Model Context Protocol (MCP)** implementation in C# with multiple server types and a client application.

## Overview

This solution showcases three different MCP server implementations and a client that can connect to all of them:

- **Console STDIO MCP Server** - Uses standard input/output for MCP communication
- **Web SSE MCP Server** - Uses HTTP with Server-Sent Events transport  
- **Web HTTP MCP Server** - Uses streamable HTTP transport
- **MCP Client** - Interactive console application that connects to all server types

## Architecture

```
┌─────────────────┐    ┌──────────────────────┐
│   MCP Client    │    │   Shared MCP Tools   │
│                 │    │                      │
│ - Interactive   │    │ - GetRandomNumber    │
│ - Multi-server  │    │ - CalculateFactorial │
│ - Transport     │    │ - IsPrime            │
│   demos         │    │ - ReverseString      │
└─────────────────┘    │ - GetCurrentTime     │
         │              │ - FormatDate         │
         │              │ - CalculateDistance  │
         ▼              │ - GenerateFibonacci   │
┌─────────────────┐    └──────────────────────┘
│ MCP Servers     │              ▲
│                 │              │
│ ├ Console STDIO │──────────────┤
│ ├ Web SSE       │──────────────┤  
│ └ Web HTTP      │──────────────┘
└─────────────────┘
```

## Projects

### 1. Shos.MCPSample2.Server.Console
**Console STDIO MCP Server**
- Transport: Standard Input/Output (STDIO)
- Port: N/A (uses STDIN/STDOUT)
- Usage: Direct process communication
- Best for: Local integrations, CLI tools, GitHub Copilot

### 2. Shos.MCPSample2.Server.WebSSE  
**Web SSE MCP Server**
- Transport: HTTP with Server-Sent Events
- Port: 5001 (HTTP), 7001 (HTTPS)
- Endpoint: `/api/mcp`
- Best for: Real-time web applications, persistent connections

### 3. Shos.MCPSample2.Server.WebHTTP
**Web HTTP MCP Server**  
- Transport: Streamable HTTP
- Port: 5002 (HTTP), 7002 (HTTPS)
- Endpoint: `/api/mcp`
- Best for: REST-like integrations, stateless connections

### 4. Shos.MCPSample2.Client
**MCP Client Application**
- Interactive console application
- Connects to all three server types
- Demonstrates different transport methods
- Shows server capabilities and health checks

### 5. Shos.MCPSample2.Shared
**Shared Library**
- Common MCP tools used by all servers
- Demonstrates various tool types:
  - **Mathematical**: Random numbers, factorials, prime checking, Fibonacci
  - **String operations**: Reverse string
  - **Date/Time**: Current time, date formatting
  - **Geometric**: Distance calculations

## Available MCP Tools

All servers expose the same set of tools through the shared library:

| Tool | Description | Parameters |
|------|-------------|------------|
| `GetRandomNumber` | Generate random number | `min` (int), `max` (int) |
| `CalculateFactorial` | Calculate factorial | `number` (int) |
| `IsPrime` | Check if number is prime | `number` (int) |
| `ReverseString` | Reverse a string | `input` (string) |
| `GetCurrentTime` | Get current UTC time | None |
| `FormatDate` | Format date string | `date` (DateTime), `format` (string) |
| `CalculateDistance` | Calculate 2D distance | `x1`, `y1`, `x2`, `y2` (double) |
| `GenerateFibonacci` | Generate Fibonacci sequence | `count` (int) |

## Quick Start

### Prerequisites
- [.NET 8.0 SDK or higher](https://dotnet.microsoft.com/download)
- [Visual Studio Code](https://code.visualstudio.com/) (recommended)

### Running the Applications

1. **Clone and build:**
   ```bash
   git clone https://github.com/Fujiwo/Shos.MCPSample2.git
   cd Shos.MCPSample2
   dotnet build
   ```

2. **Start the MCP Client:**
   ```bash
   cd Shos.MCPSample2.Client
   dotnet run
   ```

3. **Start individual servers** (in separate terminals):

   **Console STDIO Server:**
   ```bash
   cd Shos.MCPSample2.Server.Console
   dotnet run
   ```

   **Web SSE Server:**
   ```bash
   cd Shos.MCPSample2.Server.WebSSE  
   dotnet run
   # Server starts at: http://localhost:5001
   ```

   **Web HTTP Server:**
   ```bash
   cd Shos.MCPSample2.Server.WebHTTP
   dotnet run  
   # Server starts at: http://localhost:5002
   ```

## Testing the Servers

### Console STDIO Server
The STDIO server communicates via standard input/output. It's designed to be used by:
- MCP clients that can launch processes
- GitHub Copilot (when properly configured)
- Other development tools that support MCP

### Web Servers (SSE & HTTP)
Both web servers provide:
- **MCP endpoint**: `/api/mcp` 
- **Health check**: `/weatherforecast`
- **Swagger UI**: `/swagger` (in development mode)

Test the web servers:
```bash
# Health check
curl http://localhost:5001/weatherforecast  # SSE server
curl http://localhost:5002/weatherforecast  # HTTP server

# Access Swagger UI
open http://localhost:5001/swagger  # SSE server  
open http://localhost:5002/swagger  # HTTP server
```

## MCP Client Usage

The client application provides an interactive menu:

```
=== MCP Client Sample ===
Select a server to connect to:
1. Console STDIO MCP Server
2. Web SSE MCP Server (HTTP)  
3. Web HTTP MCP Server (HTTP)
4. Exit
```

Each option demonstrates:
- How to connect to that transport type
- Available server capabilities  
- Health check results
- Setup instructions if server is not running

## Development Notes

### Technology Stack
- **.NET 8.0**: Target framework
- **ModelContextProtocol**: Official C# MCP SDK (preview)
- **ModelContextProtocol.AspNetCore**: ASP.NET Core integration
- **Microsoft.Extensions.Hosting**: Dependency injection and hosting

### Transport Types

1. **STDIO Transport**
   - Direct process communication
   - Used by: `WithStdioServerTransport()`
   - Best for: Local tools, IDE integrations

2. **HTTP Transport (SSE & Streamable)**
   - Web-based communication
   - Used by: `WithHttpTransport()`  
   - Best for: Web applications, remote services

### Architecture Benefits

- **Shared Tools**: Common functionality across all servers
- **Transport Flexibility**: Same tools, different connection methods
- **Extensible**: Easy to add new tools or servers
- **Standards Compliant**: Uses official MCP SDK

## OAuth 2.1 Integration

> **Note**: OAuth 2.1 integration is planned for future implementation. The current version focuses on demonstrating core MCP functionality across different transport types.

## Contributing

This is a sample/educational project demonstrating MCP concepts. Feel free to use it as a starting point for your own MCP implementations.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Resources

- [Model Context Protocol Documentation](https://modelcontextprotocol.io/)
- [MCP C# SDK](https://github.com/modelcontextprotocol/csharp-sdk)
- [Microsoft Learn - MCP with .NET](https://learn.microsoft.com/en-us/dotnet/ai/get-started-mcp)
