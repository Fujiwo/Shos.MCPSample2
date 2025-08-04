using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol;
using Shos.MCPSample2.Shared.OAuth;
using System.Text.Json;

// Create host builder
var builder = Host.CreateApplicationBuilder(args);

// Configure logging
builder.Logging.SetMinimumLevel(LogLevel.Information);
builder.Logging.AddConsole();

// Build the host
var host = builder.Build();
var logger = host.Services.GetRequiredService<ILogger<Program>>();

logger.LogInformation("MCP Client Sample Application Starting...");

// Interactive menu loop
while (true)
{
    Console.WriteLine("\n=== MCP Client Sample with OAuth 2.1 ===");
    Console.WriteLine("Select a server to connect to:");
    Console.WriteLine("1. Console STDIO MCP Server (No OAuth - Local only)");
    Console.WriteLine("2. Web SSE MCP Server (HTTP) - OAuth 2.1 Protected");
    Console.WriteLine("3. Web HTTP MCP Server (HTTP) - OAuth 2.1 Protected"); 
    Console.WriteLine("4. Test OAuth 2.1 Flow");
    Console.WriteLine("5. Exit");
    Console.Write("Enter your choice (1-5): ");

    var choice = Console.ReadLine();

    switch (choice)
    {
        case "1":
            await ConnectToStdioServerAsync(logger);
            break;
        case "2":
            await ConnectToWebServerAsync("http://localhost:5001", "SSE", logger);
            break;
        case "3":
            await ConnectToWebServerAsync("http://localhost:5002", "HTTP", logger);
            break;
        case "4":
            await TestOAuthFlowAsync(logger);
            break;
        case "5":
            logger.LogInformation("Exiting application...");
            return;
        default:
            Console.WriteLine("Invalid choice. Please try again.");
            break;
    }
}

static async Task ConnectToStdioServerAsync(ILogger logger)
{
    logger.LogInformation("Connecting to Console STDIO MCP Server...");
    
    try
    {
        // This would require starting the console server as a subprocess
        // For now, just show what would happen
        Console.WriteLine("\nTo test the STDIO server:");
        Console.WriteLine("1. Open another terminal");
        Console.WriteLine("2. Navigate to: Shos.MCPSample2.Server.Console/");
        Console.WriteLine("3. Run: dotnet run");
        Console.WriteLine("4. The server will wait for MCP protocol messages on STDIN");
        Console.WriteLine("\nThis demonstrates the STDIO transport where MCP messages are exchanged via standard input/output.");
        
        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error connecting to STDIO server");
    }
}

static async Task ConnectToWebServerAsync(string baseUrl, string serverType, ILogger logger)
{
    var mcpUrl = $"{baseUrl}/api/mcp";
    logger.LogInformation("Connecting to {ServerType} MCP Server at {Url}...", serverType, mcpUrl);
    
    try
    {
        using var httpClient = new HttpClient();
        
        // First, check if the server is running
        try
        {
            var response = await httpClient.GetAsync($"{baseUrl}/weatherforecast");
            if (response.IsSuccessStatusCode)
            {
                logger.LogInformation("Server is responding at {Url}", baseUrl);
                
                // Show available actions
                await DemonstrateServerCapabilities(baseUrl, serverType, httpClient, logger);
                
                // Try to access protected MCP endpoint without token
                Console.WriteLine("\n=== Testing OAuth 2.1 Protection ===");
                await TestProtectedEndpoint(mcpUrl, httpClient, logger);
            }
        }
        catch (HttpRequestException)
        {
            Console.WriteLine($"\n{serverType} server is not running.");
            Console.WriteLine($"To start the server:");
            Console.WriteLine($"1. Open another terminal");
            Console.WriteLine($"2. Navigate to: Shos.MCPSample2.Server.Web{(serverType == "SSE" ? "SSE" : "HTTP")}/");
            Console.WriteLine($"3. Run: dotnet run");
            Console.WriteLine($"4. The server will start at {baseUrl}");
            Console.WriteLine($"\nThis demonstrates the {serverType} transport where MCP messages are exchanged via HTTP.");
            Console.WriteLine($"The MCP endpoint ({mcpUrl}) is now protected with OAuth 2.1 authentication.");
        }
        
        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error connecting to {ServerType} server", serverType);
    }
}

static async Task TestProtectedEndpoint(string mcpUrl, HttpClient httpClient, ILogger logger)
{
    try
    {
        Console.WriteLine($"Attempting to access protected MCP endpoint: {mcpUrl}");
        var response = await httpClient.GetAsync(mcpUrl);
        
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            Console.WriteLine("✓ MCP endpoint is properly protected with OAuth 2.1 (401 Unauthorized)");
            Console.WriteLine("  Access requires a valid Bearer token from the OAuth 2.1 flow.");
        }
        else if (response.IsSuccessStatusCode)
        {
            Console.WriteLine("⚠ MCP endpoint responded successfully - OAuth protection may not be active");
        }
        else
        {
            Console.WriteLine($"MCP endpoint returned: {response.StatusCode}");
        }
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Could not test protected endpoint");
    }
}

static async Task TestOAuthFlowAsync(ILogger logger)
{
    Console.WriteLine("\n=== OAuth 2.1 PKCE Flow Test ===");
    Console.WriteLine("Select server to test OAuth with:");
    Console.WriteLine("1. SSE Server (localhost:5001)");
    Console.WriteLine("2. HTTP Server (localhost:5002)");
    Console.Write("Enter choice (1-2): ");
    
    var choice = Console.ReadLine();
    string baseUrl = choice == "1" ? "http://localhost:5001" : "http://localhost:5002";
    string serverType = choice == "1" ? "SSE" : "HTTP";
    
    logger.LogInformation("Testing OAuth 2.1 flow with {ServerType} server at {BaseUrl}", serverType, baseUrl);
    
    try
    {
        using var httpClient = new HttpClient();
        var oauthClient = new OAuthClient(httpClient, "mcp-sample-client", baseUrl);
        
        // Check if server is running
        try
        {
            await httpClient.GetAsync($"{baseUrl}/weatherforecast");
        }
        catch (HttpRequestException)
        {
            Console.WriteLine($"\n{serverType} server is not running. Please start it first:");
            Console.WriteLine($"Navigate to: Shos.MCPSample2.Server.Web{serverType}/");
            Console.WriteLine("Run: dotnet run");
            return;
        }
        
        // Get OAuth discovery info
        Console.WriteLine("\n1. Fetching OAuth discovery information...");
        var discovery = await oauthClient.GetDiscoveryInfoAsync();
        if (discovery.HasValue)
        {
            Console.WriteLine("✓ OAuth 2.1 server discovered");
            Console.WriteLine($"  Issuer: {discovery.Value.GetProperty("issuer").GetString()}");
            Console.WriteLine($"  Authorization Endpoint: {discovery.Value.GetProperty("authorization_endpoint").GetString()}");
            Console.WriteLine($"  Token Endpoint: {discovery.Value.GetProperty("token_endpoint").GetString()}");
            
            var methods = discovery.Value.GetProperty("code_challenge_methods_supported").EnumerateArray()
                .Select(x => x.GetString()).ToArray();
            Console.WriteLine($"  PKCE Methods: {string.Join(", ", methods)}");
        }
        
        // Generate PKCE challenge
        Console.WriteLine("\n2. Generating PKCE challenge...");
        var (codeVerifier, codeChallenge) = oauthClient.GeneratePkceChallenge();
        Console.WriteLine($"✓ Code verifier generated (length: {codeVerifier.Length})");
        Console.WriteLine($"✓ Code challenge generated: {codeChallenge[..20]}...");
        
        // Create authorization URL
        var state = Guid.NewGuid().ToString("N")[..16];
        var redirectUri = "http://localhost:8080/callback";
        var authUrl = oauthClient.GetAuthorizationUrl(redirectUri, state, codeChallenge);
        
        Console.WriteLine("\n3. Authorization URL created:");
        Console.WriteLine($"   {authUrl}");
        
        Console.WriteLine("\n4. In a real application, you would:");
        Console.WriteLine("   a) Redirect user to the authorization URL");
        Console.WriteLine("   b) User would log in and consent");
        Console.WriteLine("   c) User would be redirected back with authorization code");
        Console.WriteLine("   d) Exchange code for access token using PKCE verifier");
        
        // For demo purposes, simulate the OAuth flow
        Console.WriteLine("\n5. Simulating OAuth flow for demonstration...");
        
        // Since this is a demo and we auto-approve, we can actually test the flow
        Console.WriteLine("   Visiting authorization endpoint...");
        var authResponse = await httpClient.GetAsync(authUrl);
        
        if (authResponse.StatusCode == System.Net.HttpStatusCode.Redirect)
        {
            var location = authResponse.Headers.Location?.ToString();
            if (location != null && location.Contains("code="))
            {
                // Extract the authorization code
                var uri = new Uri(location);
                var queryParams = ParseQueryString(uri.Query);
                var code = queryParams.GetValueOrDefault("code");
                var returnedState = queryParams.GetValueOrDefault("state");
                
                if (returnedState == state && !string.IsNullOrEmpty(code))
                {
                    Console.WriteLine("✓ Authorization code received");
                    
                    // Exchange code for token
                    Console.WriteLine("   Exchanging code for access token...");
                    var tokenResponse = await oauthClient.ExchangeCodeForTokenAsync(code, redirectUri, codeVerifier);
                    
                    if (tokenResponse != null)
                    {
                        Console.WriteLine("✓ OAuth 2.1 PKCE flow completed successfully!");
                        Console.WriteLine($"   Access Token: {tokenResponse.AccessToken[..20]}...");
                        Console.WriteLine($"   Token Type: {tokenResponse.TokenType}");
                        Console.WriteLine($"   Expires In: {tokenResponse.ExpiresIn} seconds");
                        
                        // Test accessing protected MCP endpoint with token
                        Console.WriteLine("\n6. Testing protected MCP endpoint with access token...");
                        httpClient.DefaultRequestHeaders.Authorization = 
                            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);
                        
                        var mcpResponse = await httpClient.GetAsync($"{baseUrl}/api/mcp");
                        if (mcpResponse.IsSuccessStatusCode)
                        {
                            Console.WriteLine("✓ Successfully accessed protected MCP endpoint with OAuth 2.1 token!");
                        }
                        else
                        {
                            Console.WriteLine($"⚠ MCP endpoint returned: {mcpResponse.StatusCode}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("✗ Failed to exchange code for token");
                    }
                }
                else
                {
                    Console.WriteLine("✗ Invalid authorization response");
                }
            }
        }
        else
        {
            Console.WriteLine($"✗ Unexpected authorization response: {authResponse.StatusCode}");
        }
        
        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error testing OAuth flow");
        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey();
    }
}

static async Task DemonstrateServerCapabilities(string baseUrl, string serverType, HttpClient httpClient, ILogger logger)
{
    Console.WriteLine($"\n=== {serverType} MCP Server Capabilities ===");
    
    // For now, we'll demonstrate the concept
    // In a real implementation, we'd use the MCP client to connect and call tools
    Console.WriteLine("\nAvailable MCP Tools (from shared SampleMcpTools):");
    Console.WriteLine("- GetRandomNumber: Generates a random number between min and max values");
    Console.WriteLine("- CalculateFactorial: Calculates the factorial of a positive integer");  
    Console.WriteLine("- IsPrime: Checks if a number is prime");
    Console.WriteLine("- ReverseString: Reverses a given string");
    Console.WriteLine("- GetCurrentTime: Gets the current server time in UTC");
    Console.WriteLine("- FormatDate: Formats a date in a specific format");
    Console.WriteLine("- CalculateDistance: Calculates distance between two 2D points");
    Console.WriteLine("- GenerateFibonacci: Generates Fibonacci numbers up to a count");
    
    Console.WriteLine($"\nMCP Endpoint: {baseUrl}/api/mcp (OAuth 2.1 Protected)");
    Console.WriteLine($"Transport Type: {serverType}");
    Console.WriteLine($"Authorization: Bearer Token Required");
    
    // Test a simple HTTP endpoint to verify the server is working
    try
    {
        var testUrl = $"{baseUrl}/weatherforecast";
        var response = await httpClient.GetAsync(testUrl);
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine("\nServer health check (WeatherForecast endpoint):");
            Console.WriteLine("✓ Server is responding correctly");
            
            // Parse and display a sample of the weather forecast
            try
            {
                var weatherData = JsonSerializer.Deserialize<JsonElement[]>(content);
                if (weatherData?.Length > 0)
                {
                    var sample = weatherData[0];
                    Console.WriteLine($"Sample response: Date={sample.GetProperty("date")}, Temp={sample.GetProperty("temperatureC")}°C");
                }
            }
            catch
            {
                // Ignore JSON parsing errors for demo
            }
        }
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Could not perform health check");
    }
}

static Dictionary<string, string> ParseQueryString(string query)
{
    var result = new Dictionary<string, string>();
    if (string.IsNullOrEmpty(query)) return result;
    
    query = query.TrimStart('?');
    var pairs = query.Split('&');
    
    foreach (var pair in pairs)
    {
        var keyValue = pair.Split('=', 2);
        if (keyValue.Length == 2)
        {
            var key = Uri.UnescapeDataString(keyValue[0]);
            var value = Uri.UnescapeDataString(keyValue[1]);
            result[key] = value;
        }
    }
    
    return result;
}
