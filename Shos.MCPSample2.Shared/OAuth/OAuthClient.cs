using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Shos.MCPSample2.Shared.OAuth;

/// <summary>
/// OAuth 2.1 PKCE Client Helper
/// </summary>
public class OAuthClient
{
    private readonly HttpClient _httpClient;
    private readonly string _clientId;
    private readonly string _baseUrl;

    public OAuthClient(HttpClient httpClient, string clientId, string baseUrl)
    {
        _httpClient = httpClient;
        _clientId = clientId;
        _baseUrl = baseUrl.TrimEnd('/');
    }

    /// <summary>
    /// Generates PKCE code verifier and challenge
    /// </summary>
    public (string CodeVerifier, string CodeChallenge) GeneratePkceChallenge()
    {
        // Generate code verifier (43-128 characters, URL-safe)
        var bytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        var codeVerifier = Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");

        // Generate code challenge (SHA256 hash of verifier)
        using var sha256 = SHA256.Create();
        var verifierBytes = Encoding.UTF8.GetBytes(codeVerifier);
        var hashBytes = sha256.ComputeHash(verifierBytes);
        var codeChallenge = Convert.ToBase64String(hashBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");

        return (codeVerifier, codeChallenge);
    }

    /// <summary>
    /// Gets authorization URL for OAuth 2.1 flow
    /// </summary>
    public string GetAuthorizationUrl(string redirectUri, string state, string codeChallenge, string scope = "openid profile")
    {
        var queryParams = new Dictionary<string, string>
        {
            ["response_type"] = "code",
            ["client_id"] = _clientId,
            ["redirect_uri"] = redirectUri,
            ["scope"] = scope,
            ["state"] = state,
            ["code_challenge"] = codeChallenge,
            ["code_challenge_method"] = "S256"
        };

        var queryString = string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));
        return $"{_baseUrl}/oauth/authorize?{queryString}";
    }

    /// <summary>
    /// Exchanges authorization code for access token
    /// </summary>
    public async Task<TokenResponse?> ExchangeCodeForTokenAsync(string code, string redirectUri, string codeVerifier)
    {
        var tokenData = new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["code"] = code,
            ["redirect_uri"] = redirectUri,
            ["client_id"] = _clientId,
            ["code_verifier"] = codeVerifier
        };

        var content = new FormUrlEncodedContent(tokenData);
        var response = await _httpClient.PostAsync($"{_baseUrl}/oauth/token", content);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Token exchange failed: {error}");
            return null;
        }

        var jsonResponse = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<TokenResponse>(jsonResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        });
    }

    /// <summary>
    /// Gets OAuth discovery information
    /// </summary>
    public async Task<JsonElement?> GetDiscoveryInfoAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/.well-known/oauth-authorization-server");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<JsonElement>(json);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Discovery failed: {ex.Message}");
        }
        return null;
    }
}