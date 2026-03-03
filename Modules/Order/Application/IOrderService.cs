using Module.Order.Domain;

namespace Module.Order.Application;

public interface IOrderService
{
    Task<IEnumerable<Domain.Order>> GetAllAsync();
    Task<Domain.Order?> GetByIdAsync(Guid id);
    Task<Domain.Order> CreateAsync(Domain.Order order);
    Task ConfirmAsync(Guid id);
    Task CancelAsync(Guid id);
}
