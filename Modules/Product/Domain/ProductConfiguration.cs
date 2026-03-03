using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Module.Product.Domain;

internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Price).HasPrecision(18, 2);

        builder.HasData(
            new Product { Id = Guid.Parse("a1b2c3d4-0001-0000-0000-000000000001"), Name = "Laptop", Price = 24999.99m, CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Product { Id = Guid.Parse("a1b2c3d4-0001-0000-0000-000000000002"), Name = "Mechanical Keyboard", Price = 1499.99m, CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Product { Id = Guid.Parse("a1b2c3d4-0001-0000-0000-000000000003"), Name = "4K Monitor", Price = 8999.99m, CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );
    }
}
