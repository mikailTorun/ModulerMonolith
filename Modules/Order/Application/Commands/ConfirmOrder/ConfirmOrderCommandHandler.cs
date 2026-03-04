using Module.Order.Domain;
using ModulerMonolith.Core.Mediator;
using ModulerMonolith.Core.Results;
using ModulerMonolith.Infrastructure.Persistence;

namespace Module.Order.Application.Commands;

internal sealed class ConfirmOrderCommandHandler : ICommandHandler<ConfirmOrderCommand, Result<string>>
{
    private readonly AppDbContext _db;

    public ConfirmOrderCommandHandler(AppDbContext db) => _db = db;

    public async Task<Result<string>> HandleAsync(ConfirmOrderCommand cmd, CancellationToken ct)
    {
        var order = await _db.Set<Domain.Order>().FindAsync([cmd.OrderId], ct);

        if (order is null)
            return Result<string>.NotFound($"'{cmd.OrderId}' ID'li sipariş bulunamadı.");

        order.Status = OrderStatus.Confirmed;
        return Result<string>.Ok(order.Id.ToString());
    }

}
