using System.Text.Json.Serialization;

namespace Module.Auth.Dtos;

public sealed record TokenResponse(
    [property: JsonPropertyName("access_token")]  string AccessToken,
    [property: JsonPropertyName("refresh_token")] string RefreshToken,
    [property: JsonPropertyName("expires_in")]    int ExpiresIn,
    [property: JsonPropertyName("token_type")]    string TokenType
);
