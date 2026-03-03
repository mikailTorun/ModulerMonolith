using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Module.Product.Application;
using ProductEntity = Module.Product.Domain.Product;

namespace Module.Product.Endpoints;

internal static class ProductEndpoints
{
    internal static IEndpointRouteBuilder MapProductCrudEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/products").WithTags("Products");

        group.MapGet("/", async (IProductService service) =>
            Results.Ok(await service.GetAllAsync()))
            .WithName("GetAllProducts")
            .WithSummary("Tüm ürünleri listeler")
            .WithDescription("Sistemdeki tüm ürünleri döner. Filtreleme veya sayfalama uygulanmaz.");

        group.MapGet("/{id:guid}", async (Guid id, IProductService service) =>
        {
            var product = await service.GetByIdAsync(id);
            return product is not null ? Results.Ok(product) : Results.NotFound();
        })
        .WithName("GetProductById")
        .WithSummary("ID'ye göre ürün getirir")
        .WithDescription("Verilen GUID'e sahip ürünü döner. Ürün bulunamazsa 404 döner.");

        group.MapPost("/", async (CreateProductRequest request, IProductService service) =>
        {
            var product = await service.CreateAsync(new ProductEntity
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Price = request.Price,
            });
            return Results.Created($"/api/products/{product.Id}", product);
        })
        .WithName("CreateProduct")
        .WithSummary("Yeni ürün oluşturur")
        .WithDescription("Yeni bir ürün kaydı oluşturur. ID otomatik atanır, `CreatedAt` alanı sunucu tarafından belirlenir.");

        group.MapPut("/{id:guid}", async (Guid id, UpdateProductRequest request, IProductService service) =>
        {
            var existing = await service.GetByIdAsync(id);
            if (existing is null) return Results.NotFound();

            existing.Name = request.Name;
            existing.Price = request.Price;
            await service.UpdateAsync(existing);
            return Results.NoContent();
        })
        .WithName("UpdateProduct")
        .WithSummary("Ürün bilgilerini günceller")
        .WithDescription("Mevcut bir ürünün `Name` ve `Price` alanlarını günceller. Ürün bulunamazsa 404 döner.");

        group.MapDelete("/{id:guid}", async (Guid id, IProductService service) =>
        {
            await service.DeleteAsync(id);
            return Results.NoContent();
        })
        .WithName("DeleteProduct")
        .WithSummary("Ürünü siler")
        .WithDescription("Verilen ID'ye sahip ürünü kalıcı olarak siler. Ürün mevcut değilse yine 204 döner.");

        return app;
    }
}

internal record CreateProductRequest(string Name, decimal Price);
internal record UpdateProductRequest(string Name, decimal Price);
