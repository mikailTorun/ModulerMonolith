using System.Text.Json;
using ModulerMonolith.Infrastructure.Persistence;

namespace ModulerMonolith.Infrastructure.Outbox;

internal sealed class OutboxService : IOutboxService
{
    private readonly AppDbContext _context;

    public OutboxService(AppDbContext context) => _context = context;

    public void Add(string eventType, object payload)
    {
        _context.Set<OutboxMessage>().Add(new OutboxMessage
        {
            EventType = eventType,
            Payload = JsonSerializer.Serialize(payload)
        });
    }
}
