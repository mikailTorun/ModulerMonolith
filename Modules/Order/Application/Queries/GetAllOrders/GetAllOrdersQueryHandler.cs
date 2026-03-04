using Microsoft.EntityFrameworkCore;
using ModulerMonolith.Core.Mediator;
using ModulerMonolith.Infrastructure.Persistence;

namespace Module.Order.Application.Queries;

internal sealed class GetAllOrdersQueryHandler : IQueryHandler<GetAllOrdersQuery, IEnumerable<Domain.Order>>
{
    private readonly AppDbContext _db;

    public GetAllOrdersQueryHandler(AppDbContext db) => _db = db;

    public async Task<IEnumerable<Domain.Order>> HandleAsync(GetAllOrdersQuery query, CancellationToken ct) =>
        await _db.Set<Domain.Order>().Include(o => o.Items).ToListAsync(ct);
}
