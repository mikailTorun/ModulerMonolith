using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Module.Order.Application;
using Module.Order.Domain;
using ModulerMonolith.Core;

namespace Module.Order.Endpoints;

internal static class OrderEndpoints
{
    internal static IEndpointRouteBuilder MapOrderCrudEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/orders").WithTags("Orders");

        group.MapGet("/", async (IOrderService service) =>
            Results.Ok(await service.GetAllAsync()))
            .WithName("GetAllOrders")
            .WithSummary("Tüm siparişleri listeler")
            .WithDescription("Sistemdeki tüm siparişleri items dahil döner.")
            .RequireAuthorization(AuthPolicies.OrderRead);

        group.MapGet("/{id:guid}", async (Guid id, IOrderService service) =>
        {
            var order = await service.GetByIdAsync(id);
            return order is not null ? Results.Ok(order) : Results.NotFound();
        })
        .WithName("GetOrderById")
        .WithSummary("ID'ye göre sipariş getirir")
        .WithDescription("Verilen GUID'e sahip siparişi items dahil döner. Bulunamazsa 404 döner.")
        .RequireAuthorization(AuthPolicies.OrderRead);

        group.MapPost("/", async (CreateOrderRequest request, IOrderService service, ICurrentUser currentUser) =>
        {
            var order = new Domain.Order
            {
                Id = Guid.NewGuid(),
                CustomerId = currentUser.Id,
                Items = request.Items.Select(i => new OrderItem
                {
                    Id = Guid.NewGuid(),
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    UnitPrice = i.UnitPrice,
                    Quantity = i.Quantity,
                }).ToList()
            };

            var created = await service.CreateAsync(order);
            return Results.Created($"/api/orders/{created.Id}", created);
        })
        .WithName("CreateOrder")
        .WithSummary("Yeni sipariş oluşturur")
        .WithDescription("Giriş yapmış kullanıcı adına sipariş oluşturur. TotalAmount sunucu tarafından hesaplanır.")
        .RequireAuthorization(AuthPolicies.Authenticated);

        group.MapPut("/{id:guid}/confirm", async (Guid id, IOrderService service) =>
        {
            await service.ConfirmAsync(id);
            return Results.NoContent();
        })
        .WithName("ConfirmOrder")
        .WithSummary("Siparişi onaylar")
        .WithDescription("Siparişin durumunu Confirmed olarak günceller. admin veya product-manager rolü gerektirir.")
        .RequireAuthorization(AuthPolicies.OrderWrite);

        group.MapPut("/{id:guid}/cancel", async (Guid id, IOrderService service) =>
        {
            await service.CancelAsync(id);
            return Results.NoContent();
        })
        .WithName("CancelOrder")
        .WithSummary("Siparişi iptal eder")
        .WithDescription("Siparişin durumunu Cancelled olarak günceller.")
        .RequireAuthorization(AuthPolicies.Authenticated);

        return app;
    }
}

internal record CreateOrderRequest(List<CreateOrderItemRequest> Items);
internal record CreateOrderItemRequest(Guid ProductId, string ProductName, decimal UnitPrice, int Quantity);
