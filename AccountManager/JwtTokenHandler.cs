using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json.Serialization;

public class JwtTokenHandler
{
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly string _tenantId;
    private readonly string _authority;
    private readonly string _scope;
    private static readonly HttpClient _httpClient = new HttpClient();

    public JwtTokenHandler(string? clientId, string? clientSecret, string? tenantId, string? scope)
    {
        // Validate environment variables and throw meaningful exceptions
        _clientId = !string.IsNullOrWhiteSpace(clientId)
            ? clientId
            : throw new ArgumentException("Client ID is missing or empty.");
        _clientSecret = !string.IsNullOrWhiteSpace(clientSecret)
            ? clientSecret
            : throw new ArgumentException("Client Secret is missing or empty.");
        _tenantId = !string.IsNullOrWhiteSpace(tenantId)
            ? tenantId
            : throw new ArgumentException("Tenant ID is missing or empty.");
        _authority = $"https://login.microsoftonline.com/{_tenantId}/oauth2/v2.0/token";
        _scope = !string.IsNullOrWhiteSpace(scope)
            ? scope
            : throw new ArgumentException("Scope is missing or empty.");
    }

    // Method to generate an OAuth 2.0 access token using client credentials flow
    public async Task<string> GenerateTokenAsync()
    {
        var parameters = new Dictionary<string, string>
        {
            { "client_id", _clientId },
            { "client_secret", _clientSecret },
            { "scope", _scope },
            { "grant_type", "client_credentials" }
        };

        var content = new FormUrlEncodedContent(parameters);

        var response = await _httpClient.PostAsync(_authority, content);
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadAsStringAsync();
            var tokenResponse = System.Text.Json.JsonSerializer.Deserialize<TokenResponse>(result);
            return tokenResponse?.AccessToken ?? string.Empty;
        }
        else
        {
            throw new Exception($"Error generating token: {response.StatusCode}");
        }
    }

    // Class to hold token response
    private class TokenResponse
    {
        [JsonPropertyName("token_type")]
        public string? TokenType { get; set; }
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }
        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
    }
}
