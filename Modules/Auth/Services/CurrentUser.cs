using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using ModulerMonolith.Core;

namespace Module.Auth.Services;

internal sealed class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
        => _httpContextAccessor = httpContextAccessor;

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public Guid Id
    {
        get
        {
            var value = User?.FindFirstValue("sub")
                     ?? User?.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }
    }

    public string Email =>
        User?.FindFirstValue(ClaimTypes.Email) ??
        User?.FindFirstValue("email") ??
        string.Empty;

    public string Name =>
        User?.FindFirstValue("preferred_username") ??
        User?.FindFirstValue(ClaimTypes.Name) ??
        string.Empty;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public IReadOnlyList<string> Roles =>
        User?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList() ?? [];

    public bool IsInRole(string role) => User?.IsInRole(role) ?? false;
}
