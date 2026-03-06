using Microsoft.EntityFrameworkCore;
using Module.Order.Domain;
using ModulerMonolith.Infrastructure.Persistence;

namespace Module.Order;

internal sealed class OrderModuleApi(AppDbContext db) : IOrderModuleApi
{
    public Task<bool> HasActiveOrdersForProductAsync(Guid productId, CancellationToken ct) =>
        db.Set<Domain.Order>()
          .Where(o => o.Status != OrderStatus.Cancelled)
          .AnyAsync(o => o.Items.Any(i => i.ProductId == productId), ct);
}
