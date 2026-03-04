using ModulerMonolith.Core.Mediator;

namespace Module.Order.Application.Queries;

public sealed record GetOrderByIdQuery(Guid Id) : IQuery<Domain.Order?>;
