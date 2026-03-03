using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using Module.Auth.Dtos;
using Module.Auth.Options;

namespace Module.Auth.Services;

internal sealed class TokenService(
    IHttpClientFactory httpClientFactory,
    IOptions<KeycloakOptions> options) : ITokenService
{
    private readonly KeycloakOptions _keycloak = options.Value;

    private string TokenEndpoint =>
        $"{_keycloak.Authority}/realms/{_keycloak.Realm}/protocol/openid-connect/token";

    private string LogoutEndpoint =>
        $"{_keycloak.Authority}/realms/{_keycloak.Realm}/protocol/openid-connect/logout";

    public async Task<TokenResponse> LoginAsync(string username, string password, CancellationToken ct = default)
    {
        var form = new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["client_id"]  = _keycloak.ClientId,
            ["username"]   = username,
            ["password"]   = password,
        };

        if (!string.IsNullOrEmpty(_keycloak.ClientSecret))
            form["client_secret"] = _keycloak.ClientSecret;

        return await PostTokenAsync(TokenEndpoint, form, ct);
    }

    public async Task<TokenResponse> RefreshAsync(string refreshToken, CancellationToken ct = default)
    {
        var form = new Dictionary<string, string>
        {
            ["grant_type"]    = "refresh_token",
            ["client_id"]     = _keycloak.ClientId,
            ["refresh_token"] = refreshToken,
        };

        if (!string.IsNullOrEmpty(_keycloak.ClientSecret))
            form["client_secret"] = _keycloak.ClientSecret;

        return await PostTokenAsync(TokenEndpoint, form, ct);
    }

    public async Task LogoutAsync(string refreshToken, CancellationToken ct = default)
    {
        var form = new Dictionary<string, string>
        {
            ["client_id"]     = _keycloak.ClientId,
            ["refresh_token"] = refreshToken,
        };

        if (!string.IsNullOrEmpty(_keycloak.ClientSecret))
            form["client_secret"] = _keycloak.ClientSecret;

        var client = httpClientFactory.CreateClient("keycloak");
        var response = await client.PostAsync(LogoutEndpoint, new FormUrlEncodedContent(form), ct);

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException("Logout failed.");
    }

    private async Task<TokenResponse> PostTokenAsync(
        string url,
        Dictionary<string, string> form,
        CancellationToken ct)
    {
        var client = httpClientFactory.CreateClient("keycloak");
        var response = await client.PostAsync(url, new FormUrlEncodedContent(form), ct);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(ct);
            throw new UnauthorizedAccessException($"Keycloak authentication failed: {error}");
        }

        var token = await response.Content.ReadFromJsonAsync<TokenResponse>(ct)
            ?? throw new InvalidOperationException("Keycloak returned an empty token response.");

        return token;
    }
}
