using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Shos.MCPSample2.Shared;
using Shos.MCPSample2.Shared.OAuth;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure OAuth settings
var oauthConfig = new OAuthConfiguration();
builder.Configuration.GetSection(OAuthConfiguration.SectionName).Bind(oauthConfig);
builder.Services.AddSingleton(oauthConfig);
builder.Services.AddSingleton<OAuthService>();

// Add JWT Bearer authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = oauthConfig.Issuer,
            ValidAudience = oauthConfig.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(oauthConfig.JwtSigningKey)),
            ClockSkew = TimeSpan.FromMinutes(5)
        };
    });

builder.Services.AddAuthorization();

// Add MCP server with HTTP transport for SSE
builder.Services.AddMcpServer()
    .WithHttpTransport() // HTTP transport with Server-Sent Events support
    .WithTools<SampleMcpTools>();

// Add CORS for MCP client connections
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

// OAuth 2.1 Authorization endpoint
app.MapGet("/oauth/authorize", async (
    string response_type,
    string client_id,
    string redirect_uri,
    string scope,
    string state,
    string code_challenge,
    string code_challenge_method,
    OAuthService oauthService) =>
{
    // Validate request parameters
    if (response_type != "code")
        return Results.BadRequest(new ErrorResponse { Error = "unsupported_response_type" });

    if (client_id != oauthConfig.DefaultClientId)
        return Results.BadRequest(new ErrorResponse { Error = "invalid_client" });

    if (code_challenge_method != "S256")
        return Results.BadRequest(new ErrorResponse { Error = "invalid_request", ErrorDescription = "code_challenge_method must be S256" });

    // For this demo, we'll automatically approve the authorization
    // In a real implementation, you'd redirect to a login/consent page
    var authCode = oauthService.StoreAuthorizationCode(client_id, redirect_uri, code_challenge, code_challenge_method, scope);
    
    var redirectUrl = $"{redirect_uri}?code={authCode}&state={state}";
    return Results.Redirect(redirectUrl);
})
.WithName("Authorize")
.WithOpenApi();

// OAuth 2.1 Token endpoint
app.MapPost("/oauth/token", async (HttpContext context, OAuthService oauthService) =>
{
    var form = await context.Request.ReadFormAsync();
    
    var grantType = form["grant_type"].ToString();
    var code = form["code"].ToString();
    var redirectUri = form["redirect_uri"].ToString();
    var clientId = form["client_id"].ToString();
    var codeVerifier = form["code_verifier"].ToString();
    
    if (grantType != "authorization_code")
        return Results.BadRequest(new ErrorResponse { Error = "unsupported_grant_type" });

    var authCode = oauthService.ValidateAuthorizationCode(code, clientId, redirectUri, codeVerifier);
    if (authCode == null)
        return Results.BadRequest(new ErrorResponse { Error = "invalid_grant" });

    var accessToken = oauthService.GenerateAccessToken(authCode.UserId, authCode.ClientId, authCode.Scope);
    
    var tokenResponse = new TokenResponse
    {
        AccessToken = accessToken,
        TokenType = "Bearer",
        ExpiresIn = oauthConfig.AccessTokenExpirationMinutes * 60,
        Scope = authCode.Scope
    };

    return Results.Ok(tokenResponse);
})
.WithName("Token")
.WithOpenApi();

// Map the MCP server endpoint with SSE transport - now protected with authentication
app.MapMcp("/api/mcp").RequireAuthorization();

// Sample endpoint for testing
var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

// OAuth discovery endpoint (optional but helpful)
app.MapGet("/.well-known/oauth-authorization-server", (OAuthConfiguration config) =>
{
    var baseUrl = config.Issuer;
    return Results.Ok(new
    {
        issuer = config.Issuer,
        authorization_endpoint = $"{baseUrl}{config.AuthorizationEndpoint}",
        token_endpoint = $"{baseUrl}{config.TokenEndpoint}",
        response_types_supported = new[] { "code" },
        grant_types_supported = new[] { "authorization_code" },
        code_challenge_methods_supported = new[] { "S256" },
        scopes_supported = new[] { "openid", "profile" }
    });
})
.WithName("OAuthDiscovery")
.WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
