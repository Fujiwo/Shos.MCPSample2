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
│ - OAuth 2.1     │    │ - GetCurrentTime     │
│   PKCE Client   │    │ - FormatDate         │
└─────────────────┘    │ - CalculateDistance  │
         │              │ - GenerateFibonacci   │
         │              └──────────────────────┘
         ▼                        ▲
┌─────────────────┐              │
│ OAuth 2.1       │              │
│ Protected       │              │
│ MCP Servers     │              │
│                 │              │
│ ├ Console STDIO │──────────────┤ (No OAuth)
│ ├ Web SSE       │──────────────┤ (OAuth 2.1)
│ └ Web HTTP      │──────────────┘ (OAuth 2.1)
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
**Web SSE MCP Server with OAuth 2.1**
- Transport: HTTP with Server-Sent Events
- Port: 5001 (HTTP), 7001 (HTTPS)
- Endpoint: `/api/mcp` (OAuth 2.1 Protected)
- OAuth: Authorization Code Flow with PKCE
- Best for: Real-time web applications, persistent connections

### 3. Shos.MCPSample2.Server.WebHTTP
**Web HTTP MCP Server with OAuth 2.1**  
- Transport: Streamable HTTP
- Port: 5002 (HTTP), 7002 (HTTPS)
- Endpoint: `/api/mcp` (OAuth 2.1 Protected)
- OAuth: Authorization Code Flow with PKCE
- Best for: REST-like integrations, stateless connections

### 4. Shos.MCPSample2.Client
**MCP Client Application with OAuth 2.1 Support**
- Interactive console application
- Connects to all three server types
- Demonstrates different transport methods
- Shows server capabilities and health checks
- **OAuth 2.1 PKCE client implementation**
- Tests OAuth authorization flows

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

The client application provides an interactive menu with OAuth 2.1 support:

```
=== MCP Client Sample with OAuth 2.1 ===
Select a server to connect to:
1. Console STDIO MCP Server (No OAuth - Local only)
2. Web SSE MCP Server (HTTP) - OAuth 2.1 Protected
3. Web HTTP MCP Server (HTTP) - OAuth 2.1 Protected
4. Test OAuth 2.1 Flow
5. Exit
```

Each option demonstrates:
- How to connect to that transport type
- Available server capabilities  
- Health check results
- OAuth 2.1 protection status
- Setup instructions if server is not running

**Option 4** provides a complete OAuth 2.1 PKCE flow demonstration, showing:
- OAuth discovery
- PKCE challenge generation
- Authorization code exchange
- Access token usage with protected endpoints

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

✅ **OAuth 2.1 implementation is now complete!** 

The MCP web servers (WebSSE and WebHTTP) are now protected with OAuth 2.1 authorization using PKCE (Proof Key for Code Exchange) for enhanced security.

### OAuth 2.1 Features

- **PKCE Required**: All authorization flows use PKCE for security
- **JWT Bearer Tokens**: Access tokens are JWT format with secure signing
- **Discovery Endpoint**: Standard OAuth discovery at `/.well-known/oauth-authorization-server`
- **Authorization Code Flow**: Standard OAuth 2.1 authorization code flow
- **MCP Protection**: MCP endpoints require valid Bearer tokens

### OAuth 2.1 Endpoints

Both web servers expose the following OAuth endpoints:

| Endpoint | Purpose |
|----------|---------|
| `/.well-known/oauth-authorization-server` | OAuth discovery information |
| `/oauth/authorize` | Authorization endpoint (PKCE required) |
| `/oauth/token` | Token exchange endpoint |
| `/api/mcp` | Protected MCP endpoint (requires Bearer token) |

### Testing OAuth 2.1

1. **Start a web server:**
   ```bash
   cd Shos.MCPSample2.Server.WebSSE
   dotnet run  # Starts on http://localhost:5001
   ```

2. **Run the client with OAuth testing:**
   ```bash
   cd Shos.MCPSample2.Client
   dotnet run
   # Select option 4: "Test OAuth 2.1 Flow"
   ```

3. **Manual OAuth Flow Testing:**
   ```bash
   # Discovery
   curl http://localhost:5001/.well-known/oauth-authorization-server
   
   # Test protected endpoint (should return 401)
   curl -I http://localhost:5001/api/mcp
   
   # Generate PKCE challenge and test full flow
   # (See client implementation for complete example)
   ```

### OAuth 2.1 Configuration

OAuth settings are configured in `appsettings.json`:

```json
{
  "OAuth": {
    "Issuer": "https://localhost:7001",
    "Audience": "mcp-api",
    "AccessTokenExpirationMinutes": 60,
    "DefaultClientId": "mcp-sample-client",
    "DefaultRedirectUri": "http://localhost:8080/callback"
  }
}
```

**Security Note**: The JWT signing key is currently hardcoded for demo purposes. In production, use secure key management (Azure Key Vault, etc.).

## Contributing

This is a sample/educational project demonstrating MCP concepts. Feel free to use it as a starting point for your own MCP implementations.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Resources

- [Model Context Protocol Documentation](https://modelcontextprotocol.io/)
- [MCP C# SDK](https://github.com/modelcontextprotocol/csharp-sdk)
- [Microsoft Learn - MCP with .NET](https://learn.microsoft.com/en-us/dotnet/ai/get-started-mcp)
