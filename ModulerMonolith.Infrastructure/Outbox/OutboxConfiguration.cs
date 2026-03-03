using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ModulerMonolith.Infrastructure.Outbox;

internal sealed class OutboxConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.EventType).IsRequired().HasMaxLength(100);
        builder.Property(m => m.Payload).IsRequired().HasColumnType("jsonb");
        builder.Property(m => m.Error).HasMaxLength(2000);

        builder.HasIndex(m => new { m.ProcessedAt, m.RetryCount });
    }
}
