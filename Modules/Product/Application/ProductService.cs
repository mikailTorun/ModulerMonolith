using Microsoft.EntityFrameworkCore;
using ModulerMonolith.Infrastructure.Persistence;
using ProductEntity = Module.Product.Domain.Product;

namespace Module.Product.Application;

internal sealed class ProductService : IProductService
{
    private readonly AppDbContext _context;

    public ProductService(AppDbContext context) => _context = context;

    public async Task<IEnumerable<ProductEntity>> GetAllAsync() =>
        await _context.Set<ProductEntity>().ToListAsync();

    public Task<ProductEntity?> GetByIdAsync(Guid id) =>
        _context.Set<ProductEntity>().FindAsync(id).AsTask();

    public async Task<ProductEntity> CreateAsync(ProductEntity product)
    {
        _context.Set<ProductEntity>().Add(product);
        await _context.SaveChangesAsync();
        return product;
    }

    public async Task UpdateAsync(ProductEntity product)
    {
        _context.Set<ProductEntity>().Update(product);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _context.Set<ProductEntity>().FindAsync(id);
        if (entity is not null)
        {
            _context.Set<ProductEntity>().Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
