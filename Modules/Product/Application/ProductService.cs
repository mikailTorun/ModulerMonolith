using Microsoft.EntityFrameworkCore;
using Module.Order;
using ModulerMonolith.Core.Results;
using ModulerMonolith.Infrastructure.Persistence;
using ProductEntity = Module.Product.Domain.Product;

namespace Module.Product.Application;

internal sealed class ProductService(AppDbContext context, IOrderModuleApi orderApi) : IProductService
{
    public async Task<IEnumerable<ProductEntity>> GetAllAsync() =>
        await context.Set<ProductEntity>().ToListAsync();

    public Task<ProductEntity?> GetByIdAsync(Guid id) =>
        context.Set<ProductEntity>().FindAsync(id).AsTask();

    public async Task<ProductEntity> CreateAsync(ProductEntity product)
    {
        context.Set<ProductEntity>().Add(product);
        await context.SaveChangesAsync();
        return product;
    }

    public async Task UpdateAsync(ProductEntity product)
    {
        context.Set<ProductEntity>().Update(product);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        if (await orderApi.HasActiveOrdersForProductAsync(id, ct))
            throw new ValidationException([new ResultError("Id", "Aktif siparişi olan ürün silinemez.")]);

        var entity = await context.Set<ProductEntity>().FindAsync([id], ct);
        if (entity is not null)
        {
            context.Set<ProductEntity>().Remove(entity);
            await context.SaveChangesAsync(ct);
        }
    }
}
