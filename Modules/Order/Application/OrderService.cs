using Microsoft.EntityFrameworkCore;
using Module.Order.Domain;
using ModulerMonolith.Infrastructure.Outbox;
using ModulerMonolith.Infrastructure.Persistence;

namespace Module.Order.Application;

internal sealed class OrderService : IOrderService
{
    private readonly AppDbContext _context;
    private readonly IOutboxService _outbox;

    public OrderService(AppDbContext context, IOutboxService outbox)
    {
        _context = context;
        _outbox = outbox;
    }

    public async Task<IEnumerable<Domain.Order>> GetAllAsync() =>
        await _context.Set<Domain.Order>().Include(o => o.Items).ToListAsync();

    public Task<Domain.Order?> GetByIdAsync(Guid id) =>
        _context.Set<Domain.Order>().Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id);

    public async Task<Domain.Order> CreateAsync(Domain.Order order)
    {
        order.TotalAmount = order.Items.Sum(i => i.UnitPrice * i.Quantity);
        _context.Set<Domain.Order>().Add(order);
        _outbox.Add("OrderCreated", new
        {
            orderId = order.Id,
            customerId = order.CustomerId,
            totalAmount = order.TotalAmount,
            itemCount = order.Items.Count
        });

        await _context.SaveChangesAsync(); // order + outbox_message aynı transaction
        return order;
    }

    public async Task ConfirmAsync(Guid id)
    {
        var order = await _context.Set<Domain.Order>().FindAsync(id);
        if (order is null) return;
        order.Status = OrderStatus.Confirmed;
        await _context.SaveChangesAsync();
    }

    public async Task CancelAsync(Guid id)
    {
        var order = await _context.Set<Domain.Order>().FindAsync(id);
        if (order is null) return;
        order.Status = OrderStatus.Cancelled;
        await _context.SaveChangesAsync();
    }
}
