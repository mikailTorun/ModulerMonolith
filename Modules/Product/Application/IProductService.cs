using ProductEntity = Module.Product.Domain.Product;

namespace Module.Product.Application;

public interface IProductService
{
    Task<IEnumerable<ProductEntity>> GetAllAsync();
    Task<ProductEntity?> GetByIdAsync(Guid id);
    Task<ProductEntity> CreateAsync(ProductEntity product);
    Task UpdateAsync(ProductEntity product);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
