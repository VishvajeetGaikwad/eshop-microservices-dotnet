using EShop.Ordering.Domain.Entities;

namespace EShop.Ordering.Application.Abstractions;

public interface IOrderRepository
{
    Task<IEnumerable<Order>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Order>> GetByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Order> AddAsync(Order order, CancellationToken cancellationToken = default);
    void Update(Order order);
    void Delete(Order order);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
