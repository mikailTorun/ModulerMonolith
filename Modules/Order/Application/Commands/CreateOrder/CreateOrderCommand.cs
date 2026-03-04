using ModulerMonolith.Core.Mediator;
using ModulerMonolith.Core.Results;

namespace Module.Order.Application.Commands;

public sealed record CreateOrderCommand(Guid CustomerId, List<OrderItemInput> Items)
    : ICommand<Result<Guid>>;

public sealed record OrderItemInput(
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity);
