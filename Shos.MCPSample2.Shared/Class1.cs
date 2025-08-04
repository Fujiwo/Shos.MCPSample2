using System.ComponentModel;
using ModelContextProtocol.Server;

namespace Shos.MCPSample2.Shared;

/// <summary>
/// Sample MCP tools for demonstration purposes.
/// These tools can be invoked by MCP clients to perform various operations.
/// </summary>
public class SampleMcpTools
{
    [McpServerTool]
    [Description("Generates a random number between the specified minimum and maximum values.")]
    public int GetRandomNumber(
        [Description("Minimum value (inclusive)")] int min = 0,
        [Description("Maximum value (exclusive)")] int max = 100)
    {
        return Random.Shared.Next(min, max);
    }

    [McpServerTool]
    [Description("Calculates the factorial of a given positive integer.")]
    public long CalculateFactorial(
        [Description("A positive integer to calculate factorial for")] int number)
    {
        if (number < 0)
            throw new ArgumentException("Number must be non-negative", nameof(number));
        
        if (number == 0 || number == 1)
            return 1;
        
        long result = 1;
        for (int i = 2; i <= number; i++)
        {
            result *= i;
        }
        
        return result;
    }

    [McpServerTool]
    [Description("Checks if a given number is prime.")]
    public bool IsPrime(
        [Description("The number to check for primality")] int number)
    {
        if (number < 2)
            return false;
        
        if (number == 2)
            return true;
        
        if (number % 2 == 0)
            return false;
        
        for (int i = 3; i * i <= number; i += 2)
        {
            if (number % i == 0)
                return false;
        }
        
        return true;
    }

    [McpServerTool]
    [Description("Reverses a given string.")]
    public string ReverseString(
        [Description("The string to reverse")] string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;
        
        return new string(input.Reverse().ToArray());
    }

    [McpServerTool]
    [Description("Gets the current server time in UTC.")]
    public DateTime GetCurrentTime()
    {
        return DateTime.UtcNow;
    }

    [McpServerTool]
    [Description("Formats a given date in a specific format.")]
    public string FormatDate(
        [Description("The date to format")] DateTime date,
        [Description("The format string (e.g., 'yyyy-MM-dd HH:mm:ss')")] string format = "yyyy-MM-dd HH:mm:ss")
    {
        return date.ToString(format);
    }

    [McpServerTool]
    [Description("Calculates the distance between two points in 2D space.")]
    public double CalculateDistance(
        [Description("X coordinate of the first point")] double x1,
        [Description("Y coordinate of the first point")] double y1,
        [Description("X coordinate of the second point")] double x2,
        [Description("Y coordinate of the second point")] double y2)
    {
        return Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
    }

    [McpServerTool]
    [Description("Generates a list of Fibonacci numbers up to a specified count.")]
    public List<long> GenerateFibonacci(
        [Description("The number of Fibonacci numbers to generate")] int count)
    {
        if (count <= 0)
            return new List<long>();
        
        var fibonacci = new List<long>();
        
        if (count >= 1) fibonacci.Add(0);
        if (count >= 2) fibonacci.Add(1);
        
        for (int i = 2; i < count; i++)
        {
            fibonacci.Add(fibonacci[i - 1] + fibonacci[i - 2]);
        }
        
        return fibonacci;
    }
}
