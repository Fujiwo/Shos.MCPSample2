using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Shos.MCPSample2.Shared.OAuth;

/// <summary>
/// Service for handling OAuth 2.1 operations with PKCE support
/// </summary>
public class OAuthService
{
    private readonly OAuthConfiguration _config;
    private readonly Dictionary<string, AuthorizationCode> _authorizationCodes = new();

    public OAuthService(OAuthConfiguration config)
    {
        _config = config;
    }

    /// <summary>
    /// Generates a secure authorization code
    /// </summary>
    public string GenerateAuthorizationCode()
    {
        var bytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
    }

    /// <summary>
    /// Stores authorization code with PKCE challenge
    /// </summary>
    public string StoreAuthorizationCode(string clientId, string redirectUri, string codeChallenge, 
        string codeChallengeMethod, string scope)
    {
        var code = GenerateAuthorizationCode();
        var authCode = new AuthorizationCode
        {
            Code = code,
            ClientId = clientId,
            RedirectUri = redirectUri,
            CodeChallenge = codeChallenge,
            CodeChallengeMethod = codeChallengeMethod,
            Scope = scope,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10), // Authorization codes expire quickly
            UserId = "demo-user"
        };

        _authorizationCodes[code] = authCode;
        return code;
    }

    /// <summary>
    /// Validates authorization code and PKCE verifier
    /// </summary>
    public AuthorizationCode? ValidateAuthorizationCode(string code, string clientId, string redirectUri, string codeVerifier)
    {
        if (!_authorizationCodes.TryGetValue(code, out var authCode))
            return null;

        // Remove the code (single use)
        _authorizationCodes.Remove(code);

        // Check if expired
        if (authCode.ExpiresAt < DateTime.UtcNow)
            return null;

        // Validate client ID and redirect URI
        if (authCode.ClientId != clientId || authCode.RedirectUri != redirectUri)
            return null;

        // Validate PKCE challenge
        if (!ValidatePkceChallenge(authCode.CodeChallenge, authCode.CodeChallengeMethod, codeVerifier))
            return null;

        return authCode;
    }

    /// <summary>
    /// Validates PKCE code challenge against verifier
    /// </summary>
    private bool ValidatePkceChallenge(string codeChallenge, string codeChallengeMethod, string codeVerifier)
    {
        if (codeChallengeMethod != "S256")
            return false;

        using var sha256 = SHA256.Create();
        var verifierBytes = Encoding.UTF8.GetBytes(codeVerifier);
        var hashBytes = sha256.ComputeHash(verifierBytes);
        var computedChallenge = Convert.ToBase64String(hashBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");

        return codeChallenge == computedChallenge;
    }

    /// <summary>
    /// Generates JWT access token
    /// </summary>
    public string GenerateAccessToken(string userId, string clientId, string scope)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.JwtSigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Name, userId),
            new Claim("client_id", clientId),
            new Claim("scope", scope),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        var token = new JwtSecurityToken(
            issuer: _config.Issuer,
            audience: _config.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_config.AccessTokenExpirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Gets signing key for JWT validation
    /// </summary>
    public SymmetricSecurityKey GetSigningKey()
    {
        return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.JwtSigningKey));
    }
}