namespace ModulerMonolith.Infrastructure.Outbox;

public interface IOutboxService
{
    /// <summary>
    /// Outbox'a event ekler. SaveChanges çağırmaz.
    /// Çağıran servis kendi SaveChangesAsync'i ile hem asıl entity'yi
    /// hem bu mesajı aynı transaction'da commit eder.
    /// </summary>
    void Add(string eventType, object payload);
}
