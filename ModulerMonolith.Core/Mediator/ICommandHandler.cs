using ModulerMonolith.Core.Results;

namespace ModulerMonolith.Core.Mediator;

public interface ICommandHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
    where TResponse : IResultBase<TResponse>
{
    Task<TResponse> HandleAsync(TCommand command, CancellationToken ct);
}
