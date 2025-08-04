using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol;
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
    Console.WriteLine("\n=== MCP Client Sample ===");
    Console.WriteLine("Select a server to connect to:");
    Console.WriteLine("1. Console STDIO MCP Server");
    Console.WriteLine("2. Web SSE MCP Server (HTTP)");
    Console.WriteLine("3. Web HTTP MCP Server (HTTP)"); 
    Console.WriteLine("4. Exit");
    Console.Write("Enter your choice (1-4): ");

    var choice = Console.ReadLine();

    switch (choice)
    {
        case "1":
            await ConnectToStdioServerAsync(logger);
            break;
        case "2":
            await ConnectToWebServerAsync("http://localhost:5001/api/mcp", "SSE", logger);
            break;
        case "3":
            await ConnectToWebServerAsync("http://localhost:5002/api/mcp", "HTTP", logger);
            break;
        case "4":
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

static async Task ConnectToWebServerAsync(string url, string serverType, ILogger logger)
{
    logger.LogInformation("Connecting to {ServerType} MCP Server at {Url}...", serverType, url);
    
    try
    {
        using var httpClient = new HttpClient();
        
        // First, check if the server is running
        try
        {
            var response = await httpClient.GetAsync(url.Replace("/api/mcp", "/weatherforecast"));
            if (response.IsSuccessStatusCode)
            {
                logger.LogInformation("Server is responding at {Url}", url);
                
                // Show available actions
                await DemonstrateServerCapabilities(url, serverType, httpClient, logger);
            }
        }
        catch (HttpRequestException)
        {
            Console.WriteLine($"\n{serverType} server is not running.");
            Console.WriteLine($"To start the server:");
            Console.WriteLine($"1. Open another terminal");
            Console.WriteLine($"2. Navigate to: Shos.MCPSample2.Server.Web{(serverType == "SSE" ? "SSE" : "HTTP")}/");
            Console.WriteLine($"3. Run: dotnet run");
            Console.WriteLine($"4. The server will start at {url}");
            Console.WriteLine($"\nThis demonstrates the {serverType} transport where MCP messages are exchanged via HTTP.");
        }
        
        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error connecting to {ServerType} server", serverType);
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
    
    Console.WriteLine($"\nMCP Endpoint: {baseUrl}");
    Console.WriteLine($"Transport Type: {serverType}");
    
    // Test a simple HTTP endpoint to verify the server is working
    try
    {
        var testUrl = baseUrl.Replace("/api/mcp", "/weatherforecast");
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
