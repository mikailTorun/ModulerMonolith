using Microsoft.EntityFrameworkCore;
using ModulerMonolith.Core.Mediator;
using ModulerMonolith.Infrastructure.Persistence;

namespace Module.Order.Application.Queries;

internal sealed class GetOrderByIdQueryHandler : IQueryHandler<GetOrderByIdQuery, Domain.Order?>
{
    private readonly AppDbContext _db;

    public GetOrderByIdQueryHandler(AppDbContext db) => _db = db;

    public Task<Domain.Order?> HandleAsync(GetOrderByIdQuery query, CancellationToken ct) =>
        _db.Set<Domain.Order>().Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == query.Id, ct);
}
