using ModulerMonolith.Core.Mediator;
using ModulerMonolith.Core.Results;

namespace Module.Order.Application.Commands;

public sealed record CancelOrderCommand(Guid OrderId) : ICommand<Result>;
