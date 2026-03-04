using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Module.Auth;
using Module.Order;
using Module.Product;
using ModulerMonolith.Api.OpenApi;
using ModulerMonolith.Infrastructure;
using ModulerMonolith.Infrastructure.Persistence;
using Scalar.AspNetCore;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, services, lc) => lc
        .ReadFrom.Configuration(ctx.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.Seq(ctx.Configuration["Seq:ServerUrl"] ?? "http://localhost:5341"));

    builder.Services.AddOpenApi(options =>
    {
        options.AddDocumentTransformer((document, context, ct) =>
        {
            document.Info.Title = "ModulerMonolith API";
            document.Info.Version = "v1";
            document.Info.Description = """
                .NET 10 Modüler Monolit mimarisi üzerine kurulu REST API.

                ## Modüller
                - **Auth** — Keycloak tabanlı JWT kimlik doğrulama
                - **Products** — Ürün yönetimi (CRUD)
                - **Orders** — Sipariş yönetimi

                ## Kimlik Doğrulama
                Korumalı endpoint'ler için Keycloak'tan alınan Bearer token gereklidir.
                """;
            return Task.CompletedTask;
        });

        options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();

        options.AddOperationTransformer((operation, context, ct) =>
        {
            var requiresAuth = context.Description.ActionDescriptor.EndpointMetadata
                .OfType<IAuthorizeData>().Any();

            if (requiresAuth)
            {
                operation.Security =
                [
                    new OpenApiSecurityRequirement
                    {
                        [new OpenApiSecuritySchemeReference("Bearer", context.Document)] = []
                    }
                ];
            }

            return Task.CompletedTask;
        });
    });

    builder.Services
        .AddInfrastructure(builder.Configuration)
        .AddAuthModule(builder.Configuration)
        .AddProductModule(builder.Configuration)
        .AddOrderModule(builder.Configuration);

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();
    }

    app.UseExceptionHandler();
    app.UseSerilogRequestLogging();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapScalarApiReference();
    }

    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapAuthEndpoints();
    app.MapProductEndpoints();
    app.MapOrderEndpoints();

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
