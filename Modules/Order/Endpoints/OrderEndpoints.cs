using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Module.Order.Application.Commands;
using Module.Order.Application.Queries;
using ModulerMonolith.Core;
using ModulerMonolith.Core.Mediator;
using ModulerMonolith.Core.Results;
using ModulerMonolith.Infrastructure.Http;

namespace Module.Order.Endpoints;

internal static class OrderEndpoints
{
    internal static IEndpointRouteBuilder MapOrderCrudEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/orders").WithTags("Orders");

        group.MapGet("/", async (IMediator mediator, CancellationToken ct) =>
        {
            var data = await mediator.QueryAsync<GetAllOrdersQuery, IEnumerable<Domain.Order>>(new GetAllOrdersQuery(), ct);
            return Results.Ok(Result<IEnumerable<Domain.Order>>.Ok(data));
        })
        .WithName("GetAllOrders")
        .WithSummary("Tüm siparişleri listeler")
        .Produces<Result<IEnumerable<Domain.Order>>>()
        .RequireAuthorization(AuthPolicies.OrderRead);

        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator, CancellationToken ct) =>
        {
            var order = await mediator.QueryAsync<GetOrderByIdQuery, Domain.Order?>(new GetOrderByIdQuery(id), ct);
            return order is not null
                ? Results.Ok(Result<Domain.Order>.Ok(order))
                : Results.Json(Result<Domain.Order>.NotFound("Sipariş bulunamadı.", "Id"), statusCode: StatusCodes.Status404NotFound);
        })
        .WithName("GetOrderById")
        .WithSummary("ID'ye göre sipariş getirir")
        .Produces<Result<Domain.Order>>()
        .Produces<Result<Domain.Order>>(StatusCodes.Status404NotFound)
        .RequireAuthorization(AuthPolicies.OrderRead);

        group.MapPost("/", async (CreateOrderRequest request, IMediator mediator, ICurrentUser currentUser, CancellationToken ct) =>
        {
            var command = new CreateOrderCommand(
                currentUser.Id,
                [.. request.Items.Select(i => new OrderItemInput(i.ProductId, i.ProductName, i.UnitPrice, i.Quantity))]);

            var result = await mediator.SendAsync<CreateOrderCommand, Result<Guid>>(command, ct);
            return result.ToApiResult(StatusCodes.Status201Created);
        })
        .WithName("CreateOrder")
        .WithSummary("Yeni sipariş oluşturur")
        .Produces<Result<Guid>>(StatusCodes.Status201Created)
        .Produces<Result<Guid>>(StatusCodes.Status422UnprocessableEntity)
        .RequireAuthorization(AuthPolicies.Authenticated);

        group.MapPut("/{id:guid}/confirm", async (Guid id, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.SendAsync<ConfirmOrderCommand, Result<string>>(new ConfirmOrderCommand(id), ct);
            return result.ToApiResult();
        })
        .WithName("ConfirmOrder")
        .WithSummary("Siparişi onaylar")
        .Produces<Result<string>>()
        .Produces<Result<string>>(StatusCodes.Status404NotFound)
        .RequireAuthorization(AuthPolicies.OrderWrite);

        group.MapPut("/{id:guid}/cancel", async (Guid id, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.SendAsync<CancelOrderCommand, Result>(new CancelOrderCommand(id), ct);
            return result.ToApiResult();
        })
        .WithName("CancelOrder")
        .WithSummary("Siparişi iptal eder")
        .Produces<Result>()
        .Produces<Result>(StatusCodes.Status404NotFound)
        .RequireAuthorization(AuthPolicies.Authenticated);

        return app;
    }
}

internal record CreateOrderRequest(List<CreateOrderItemRequest> Items);
internal record CreateOrderItemRequest(Guid ProductId, string ProductName, decimal UnitPrice, int Quantity);
