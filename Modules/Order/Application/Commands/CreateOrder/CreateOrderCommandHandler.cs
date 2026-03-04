using Module.Order.Domain;
using ModulerMonolith.Core.Mediator;
using ModulerMonolith.Core.Results;
using ModulerMonolith.Infrastructure.Outbox;
using ModulerMonolith.Infrastructure.Persistence;

namespace Module.Order.Application.Commands;

internal sealed class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand, Result<Guid>>
{
    private readonly AppDbContext _db;
    private readonly IOutboxService _outbox;

    public CreateOrderCommandHandler(AppDbContext db, IOutboxService outbox)
    {
        _db = db;
        _outbox = outbox;
    }

    public Task<Result<Guid>> HandleAsync(CreateOrderCommand cmd, CancellationToken ct)
    {
        var order = new Domain.Order
        {
            Id = Guid.NewGuid(),
            CustomerId = cmd.CustomerId,
            Items = [.. cmd.Items.Select(i => new OrderItem
            {
                Id = Guid.NewGuid(),
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                UnitPrice = i.UnitPrice,
                Quantity = i.Quantity,
            })]
        };

        order.TotalAmount = order.Items.Sum(i => i.UnitPrice * i.Quantity);

        _db.Set<Domain.Order>().Add(order);

        _outbox.Add("OrderCreated", new
        {
            orderId = order.Id,
            customerId = order.CustomerId,
            totalAmount = order.TotalAmount,
            itemCount = order.Items.Count
        });

        // SaveChanges yok — TransactionBehavior halleder
        return Task.FromResult(Result<Guid>.Ok(order.Id));
    }
}
