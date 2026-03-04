using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Module.Auth.Authorization;
using Module.Auth.Dtos;
using Module.Auth.Options;
using Module.Auth.Services;
using ModulerMonolith.Core;

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

        services.AddAuthorization(options =>
        {
            options.AddPolicy(AuthPolicies.Authenticated, p =>
                p.RequireAuthenticatedUser());

            options.AddPolicy(AuthPolicies.ProductRead, p =>
                p.RequireAuthenticatedUser());

            options.AddPolicy(AuthPolicies.ProductWrite, p =>
                p.RequireAuthenticatedUser()
                 .RequireRole("product-manager", "admin"));

            options.AddPolicy(AuthPolicies.OrderRead, p =>
                p.RequireAuthenticatedUser());

            options.AddPolicy(AuthPolicies.OrderWrite, p =>
                p.RequireAuthenticatedUser()
                 .RequireRole("product-manager", "admin"));

            options.AddPolicy(AuthPolicies.Admin, p =>
                p.RequireAuthenticatedUser()
                 .RequireRole("admin"));
        });

        services.AddTransient<IClaimsTransformation, KeycloakClaimsTransformation>();
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();

        services.AddHttpClient("keycloak");
        services.AddScoped<ITokenService, TokenService>();

        return services;
    }

    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/token", async (LoginRequest request, ITokenService tokenService, CancellationToken ct) =>
        {
            try
            {
                var token = await tokenService.LoginAsync(request.Username, request.Password, ct);
                return Results.Ok(token);
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Problem(
                    title: "Authentication failed",
                    detail: "Invalid username or password.",
                    statusCode: StatusCodes.Status401Unauthorized);
            }
        })
        .WithName("Login")
        .WithSummary("Kullanıcı girişi")
        .WithDescription("Kullanıcı adı ve şifre ile Keycloak üzerinden JWT token alır. Dönen `access_token` diğer korumalı endpoint'lerde Bearer token olarak kullanılır.")
        .WithHttpLogging(HttpLoggingFields.RequestMethod | HttpLoggingFields.RequestPath | HttpLoggingFields.ResponseStatusCode | HttpLoggingFields.Duration);

        group.MapPost("/token/refresh", async (RefreshTokenRequest request, ITokenService tokenService, CancellationToken ct) =>
        {
            try
            {
                var token = await tokenService.RefreshAsync(request.RefreshToken, ct);
                return Results.Ok(token);
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Problem(
                    title: "Token refresh failed",
                    detail: "The refresh token is invalid or has expired.",
                    statusCode: StatusCodes.Status401Unauthorized);
            }
        })
        .WithName("RefreshToken")
        .WithSummary("Token yenile")
        .WithDescription("Geçerli bir `refresh_token` ile yeni bir `access_token` alır. Access token süresi dolduğunda bu endpoint kullanılır.")
        .WithHttpLogging(HttpLoggingFields.RequestMethod | HttpLoggingFields.RequestPath | HttpLoggingFields.ResponseStatusCode | HttpLoggingFields.Duration);

        group.MapPost("/logout", async (LogoutRequest request, ITokenService tokenService, CancellationToken ct) =>
        {
            try
            {
                await tokenService.LogoutAsync(request.RefreshToken, ct);
            }
            catch
            {
                // Logout is best-effort — swallow errors
            }
            return Results.NoContent();
        })
        .WithName("Logout")
        .WithSummary("Çıkış yap")
        .WithDescription("Refresh token'ı Keycloak'ta iptal eder. Başarısız olsa bile 204 döner.")
        .WithHttpLogging(HttpLoggingFields.RequestMethod | HttpLoggingFields.RequestPath | HttpLoggingFields.ResponseStatusCode | HttpLoggingFields.Duration);

        group.MapGet("/me", (HttpContext ctx) =>
        {
            var claims = ctx.User.Claims.Select(c => new { c.Type, c.Value });
            return Results.Ok(claims);
        })
        .RequireAuthorization()
        .WithName("GetCurrentUser")
        .WithSummary("Mevcut kullanıcı bilgilerini getirir")
        .WithDescription("Geçerli Bearer token içindeki tüm claim'leri döner. Bu endpoint yalnızca kimliği doğrulanmış kullanıcılar tarafından çağrılabilir.")
        .WithHttpLogging(HttpLoggingFields.All & ~HttpLoggingFields.RequestHeaders & ~HttpLoggingFields.ResponseHeaders);

        return app;
    }
}
