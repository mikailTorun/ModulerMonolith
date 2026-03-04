using ModulerMonolith.Core.Mediator;
using ModulerMonolith.Core.Results;

namespace Module.Order.Application.Commands;

public sealed record ConfirmOrderCommand(Guid OrderId) : ICommand<Result<string>>;
