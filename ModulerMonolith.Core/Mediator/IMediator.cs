using ModulerMonolith.Core.Results;

namespace ModulerMonolith.Core.Mediator;

public interface IMediator
{
    /// <summary>
    /// Command gönderir.
    /// Pipeline: ValidationBehavior → TransactionBehavior → Handler
    /// </summary>
    Task<TResponse> SendAsync<TCommand, TResponse>(TCommand command, CancellationToken ct = default)
        where TCommand : ICommand<TResponse>
        where TResponse : IResultBase<TResponse>;

    /// <summary>
    /// Query gönderir. Validation ve transaction çalışmaz.
    /// </summary>
    Task<TResponse> QueryAsync<TQuery, TResponse>(TQuery query, CancellationToken ct = default)
        where TQuery : IQuery<TResponse>;
}
