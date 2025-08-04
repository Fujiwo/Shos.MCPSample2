using System.ComponentModel.DataAnnotations;

namespace Shos.MCPSample2.Shared.OAuth;

/// <summary>
/// OAuth 2.1 configuration settings
/// </summary>
public class OAuthConfiguration
{
    public const string SectionName = "OAuth";
    
    /// <summary>
    /// JWT signing key - should be stored securely in production
    /// </summary>
    [Required]
    public string JwtSigningKey { get; set; } = "this-is-a-sample-signing-key-change-in-production-and-use-key-vault-or-similar-secure-storage-mechanism";
    
    /// <summary>
    /// JWT issuer
    /// </summary>
    [Required]
    public string Issuer { get; set; } = "https://localhost:7001";
    
    /// <summary>
    /// JWT audience
    /// </summary>
    [Required]
    public string Audience { get; set; } = "mcp-api";
    
    /// <summary>
    /// Access token expiration time in minutes
    /// </summary>
    public int AccessTokenExpirationMinutes { get; set; } = 60;
    
    /// <summary>
    /// Authorization endpoint
    /// </summary>
    public string AuthorizationEndpoint { get; set; } = "/oauth/authorize";
    
    /// <summary>
    /// Token endpoint
    /// </summary>
    public string TokenEndpoint { get; set; } = "/oauth/token";
    
    /// <summary>
    /// Default client ID for the sample
    /// </summary>
    public string DefaultClientId { get; set; } = "mcp-sample-client";
    
    /// <summary>
    /// Redirect URI for the sample client
    /// </summary>
    public string DefaultRedirectUri { get; set; } = "http://localhost:8080/callback";
}