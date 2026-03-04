using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using ModulerMonolith.Core.Mediator;
using ModulerMonolith.Core.Results;
using CoreValidationException = ModulerMonolith.Core.Results.ValidationException;

namespace ModulerMonolith.Infrastructure.Mediator;

internal sealed class Mediator : IMediator
{
    private readonly IServiceProvider _sp;
    private readonly TransactionBehavior _tx;

    public Mediator(IServiceProvider sp, TransactionBehavior tx)
    {
        _sp = sp;
        _tx = tx;
    }

    public async Task<TResponse> SendAsync<TCommand, TResponse>(TCommand command, CancellationToken ct = default)
        where TCommand : ICommand<TResponse>
        where TResponse : IResultBase<TResponse>
    {
        var errors = await TryGetValidationErrorsAsync(command, ct);
        if (errors is not null)
            return TResponse.Fail(errors);

        var handler = _sp.GetRequiredService<ICommandHandler<TCommand, TResponse>>();
        return await _tx.ExecuteAsync(() => handler.HandleAsync(command, ct), ct);
    }

    public async Task<TResponse> QueryAsync<TQuery, TResponse>(TQuery query, CancellationToken ct = default)
        where TQuery : IQuery<TResponse>
    {
        var errors = await TryGetValidationErrorsAsync(query, ct);
        if (errors is not null)
            throw new CoreValidationException(errors);

        var handler = _sp.GetRequiredService<IQueryHandler<TQuery, TResponse>>();
        return await handler.HandleAsync(query, ct);
    }

    private async Task<IReadOnlyList<ResultError>?> TryGetValidationErrorsAsync<T>(T request, CancellationToken ct)
    {
        var validator = _sp.GetService<IValidator<T>>();
        if (validator is null) return null;

        var result = await validator.ValidateAsync(request, ct);
        if (result.IsValid) return null;

        return result.Errors
            .Select(e => new ResultError(e.PropertyName, e.ErrorMessage))
            .ToList();
    }
}
