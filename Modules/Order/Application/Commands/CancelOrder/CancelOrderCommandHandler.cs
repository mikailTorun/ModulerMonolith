using Module.Order.Domain;
using ModulerMonolith.Core.Mediator;
using ModulerMonolith.Core.Results;
using ModulerMonolith.Infrastructure.Persistence;

namespace Module.Order.Application.Commands;

internal sealed class CancelOrderCommandHandler : ICommandHandler<CancelOrderCommand, Result>
{
    private readonly AppDbContext _db;

    public CancelOrderCommandHandler(AppDbContext db) => _db = db;

    public async Task<Result> HandleAsync(CancelOrderCommand cmd, CancellationToken ct)
    {
        var order = await _db.Set<Domain.Order>().FindAsync([cmd.OrderId], ct);

        if (order is null)
            return Result.NotFound($"'{cmd.OrderId}' ID'li sipariş bulunamadı.");

        order.Status = OrderStatus.Cancelled;
        return Result.Ok();
    }
}
