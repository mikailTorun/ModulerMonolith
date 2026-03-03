using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Module.Auth.Options;

namespace Module.Auth;

public static class AuthModule
{
    public static IServiceCollection AddAuthModule(this IServiceCollection services, IConfiguration configuration)
    {
        var keycloakOptions = configuration.GetSection("Keycloak").Get<KeycloakOptions>()
            ?? throw new InvalidOperationException("Keycloak configuration is missing.");

        services.Configure<KeycloakOptions>(configuration.GetSection("Keycloak"));

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = $"{keycloakOptions.Authority}/realms/{keycloakOptions.Realm}";
                options.Audience = keycloakOptions.ClientId;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                };
            });

        services.AddAuthorization();

        return services;
    }

    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        group.MapGet("/me", (HttpContext ctx) =>
        {
            var claims = ctx.User.Claims.Select(c => new { c.Type, c.Value });
            return Results.Ok(claims);
        })
        .RequireAuthorization()
        .WithName("GetCurrentUser")
        .WithSummary("Mevcut kullanıcı bilgilerini getirir")
        .WithDescription("Geçerli Bearer token içindeki tüm claim'leri döner. Bu endpoint yalnızca kimliği doğrulanmış kullanıcılar tarafından çağrılabilir.");

        return app;
    }
}
