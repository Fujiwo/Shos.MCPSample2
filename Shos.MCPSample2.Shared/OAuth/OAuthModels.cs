namespace Shos.MCPSample2.Shared.OAuth;

/// <summary>
/// OAuth 2.1 request models
/// </summary>
public class AuthorizationRequest
{
    public string ResponseType { get; set; } = "code";
    public string ClientId { get; set; } = "";
    public string RedirectUri { get; set; } = "";
    public string Scope { get; set; } = "openid profile";
    public string State { get; set; } = "";
    public string CodeChallenge { get; set; } = "";
    public string CodeChallengeMethod { get; set; } = "S256";
}

public class TokenRequest
{
    public string GrantType { get; set; } = "authorization_code";
    public string Code { get; set; } = "";
    public string RedirectUri { get; set; } = "";
    public string ClientId { get; set; } = "";
    public string CodeVerifier { get; set; } = "";
}

public class TokenResponse
{
    public string AccessToken { get; set; } = "";
    public string TokenType { get; set; } = "Bearer";
    public int ExpiresIn { get; set; }
    public string? RefreshToken { get; set; }
    public string? Scope { get; set; }
}

public class ErrorResponse
{
    public string Error { get; set; } = "";
    public string? ErrorDescription { get; set; }
    public string? ErrorUri { get; set; }
}

/// <summary>
/// In-memory storage for authorization codes and PKCE challenges
/// In production, use a proper data store
/// </summary>
public class AuthorizationCode
{
    public string Code { get; set; } = "";
    public string ClientId { get; set; } = "";
    public string RedirectUri { get; set; } = "";
    public string CodeChallenge { get; set; } = "";
    public string CodeChallengeMethod { get; set; } = "";
    public string Scope { get; set; } = "";
    public DateTime ExpiresAt { get; set; }
    public string UserId { get; set; } = "demo-user"; // For demo purposes
}