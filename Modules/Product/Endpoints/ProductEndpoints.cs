using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Routing;
using Module.Product.Application;
using ModulerMonolith.Core;
using ModulerMonolith.Core.Results;
using ProductEntity = Module.Product.Domain.Product;

namespace Module.Product.Endpoints;

internal static class ProductEndpoints
{
    internal static IEndpointRouteBuilder MapProductCrudEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/products").WithTags("Products");

        group.MapGet("/", async (IProductService service) =>
        {
            var data = await service.GetAllAsync();
            return Results.Ok(Result<IEnumerable<ProductEntity>>.Ok(data));
        })
        .WithName("GetAllProducts")
        .WithSummary("Tüm ürünleri listeler")
        .Produces<Result<IEnumerable<ProductEntity>>>()
        .WithHttpLogging(HttpLoggingFields.All & ~HttpLoggingFields.RequestHeaders & ~HttpLoggingFields.ResponseHeaders)
        .RequireAuthorization(AuthPolicies.ProductRead);

        group.MapGet("/{id:guid}", async (Guid id, IProductService service) =>
        {
            var product = await service.GetByIdAsync(id);
            return product is not null
                ? Results.Ok(Result<ProductEntity>.Ok(product))
                : Results.Json(Result<ProductEntity>.NotFound("Ürün bulunamadı."), statusCode: StatusCodes.Status404NotFound);
        })
        .WithName("GetProductById")
        .WithSummary("ID'ye göre ürün getirir")
        .Produces<Result<ProductEntity>>()
        .Produces<Result<ProductEntity>>(StatusCodes.Status404NotFound)
        .WithHttpLogging(HttpLoggingFields.All & ~HttpLoggingFields.RequestHeaders & ~HttpLoggingFields.ResponseHeaders)
        .RequireAuthorization(AuthPolicies.ProductRead);

        group.MapPost("/", async (CreateProductRequest request, IProductService service) =>
        {
            var product = await service.CreateAsync(new ProductEntity
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Price = request.Price,
            });
            return Results.Json(Result<ProductEntity>.Ok(product, "Ürün başarıyla oluşturuldu.", "Ürün sayısı güncellendi."), statusCode: StatusCodes.Status201Created);
        })
        .WithName("CreateProduct")
        .WithSummary("Yeni ürün oluşturur")
        .WithDescription("Bu endpoint yeni bir ürün oluşturur. Ürün adı ve fiyatı sağlanmalıdır.")
        .Produces<Result<ProductEntity>>(StatusCodes.Status201Created)
        .WithHttpLogging(HttpLoggingFields.All & ~HttpLoggingFields.RequestHeaders & ~HttpLoggingFields.ResponseHeaders)
        .RequireAuthorization(AuthPolicies.ProductWrite);

        group.MapPut("/{id:guid}", async (Guid id, UpdateProductRequest request, IProductService service) =>
        {
            var existing = await service.GetByIdAsync(id);
            if (existing is null)
                return Results.Json(Result<ProductEntity>.NotFound("Ürün bulunamadı."), statusCode: StatusCodes.Status404NotFound);

            existing.Name = request.Name;
            existing.Price = request.Price;
            await service.UpdateAsync(existing);
            return Results.Ok(Result.Ok());
        })
        .WithName("UpdateProduct")
        .WithSummary("Ürün bilgilerini günceller")
        .Produces<Result>()
        .Produces<Result<ProductEntity>>(StatusCodes.Status404NotFound)
        .WithHttpLogging(HttpLoggingFields.All & ~HttpLoggingFields.RequestHeaders & ~HttpLoggingFields.ResponseHeaders)
        .RequireAuthorization(AuthPolicies.ProductWrite);

        group.MapDelete("/{id:guid}", async (Guid id, IProductService service) =>
        {
            await service.DeleteAsync(id);
            return Results.Ok(Result.Ok());
        })
        .WithName("DeleteProduct")
        .WithSummary("Ürünü siler")
        .Produces<Result>()
        .WithHttpLogging(HttpLoggingFields.All & ~HttpLoggingFields.RequestHeaders & ~HttpLoggingFields.ResponseHeaders)
        .RequireAuthorization(AuthPolicies.ProductWrite);

        return app;
    }
}

internal record CreateProductRequest(string Name, decimal Price);
internal record UpdateProductRequest(string Name, decimal Price);
