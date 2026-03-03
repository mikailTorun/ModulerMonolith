using Module.Auth;
using Module.Product;
using ModulerMonolith.Infrastructure;
using ModulerMonolith.Infrastructure.Persistence;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

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

            ## Kimlik Doğrulama
            Korumalı endpoint'ler için Keycloak'tan alınan Bearer token gereklidir.
            """;
        return Task.CompletedTask;
    });
});

builder.Services
    .AddInfrastructure(builder.Configuration)
    .AddAuthModule(builder.Configuration)
    .AddProductModule(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

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

app.Run();
