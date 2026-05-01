using EShop.Ordering.Application.Abstractions;
using EShop.Ordering.Domain.Entities;
using EShop.Ordering.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EShop.Ordering.Infrastructure.Repositories;

public class OrderRepository(OrderDbContext context) : IOrderRepository
{
    public async Task<IEnumerable<Order>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await context.Orders
            .Include(o => o.OrderItems)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Order>> GetByCustomerAsync(Guid customerId, CancellationToken cancellationToken)
    {
        return await context.Orders
            .Include(o => o.OrderItems)
            .Where(o => o.CustomerId == customerId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<Order> AddAsync(Order order, CancellationToken cancellationToken)
    {
        await context.Orders.AddAsync(order, cancellationToken);
        return order;
    }

    public void Update(Order order)
    {
        context.Orders.Update(order);
    }

    public void Delete(Order order)
    {
        context.Orders.Remove(order);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await context.SaveChangesAsync(cancellationToken);
    }
}
