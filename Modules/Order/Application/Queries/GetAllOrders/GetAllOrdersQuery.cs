using ModulerMonolith.Core.Mediator;

namespace Module.Order.Application.Queries;

public sealed record GetAllOrdersQuery : IQuery<IEnumerable<Domain.Order>>;
