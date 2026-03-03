using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication;

namespace Module.Auth.Authorization;

/// <summary>
/// Keycloak JWT'deki realm_access.roles alanını .NET'in ClaimTypes.Role formatına çevirir.
/// Böylece [Authorize(Roles = "admin")] ve RequireRole() doğru çalışır.
/// </summary>
internal sealed class KeycloakClaimsTransformation : IClaimsTransformation
{
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity is not ClaimsIdentity { IsAuthenticated: true } identity)
            return Task.FromResult(principal);

        var realmAccessClaim = identity.FindFirst("realm_access");
        if (realmAccessClaim is null)
            return Task.FromResult(principal);

        var realmAccess = JsonSerializer.Deserialize<RealmAccess>(realmAccessClaim.Value);
        if (realmAccess?.Roles is null)
            return Task.FromResult(principal);

        foreach (var role in realmAccess.Roles)
        {
            if (!identity.HasClaim(ClaimTypes.Role, role))
                identity.AddClaim(new Claim(ClaimTypes.Role, role));
        }

        return Task.FromResult(principal);
    }

    private sealed record RealmAccess(
        [property: JsonPropertyName("roles")] IReadOnlyList<string>? Roles);
}
