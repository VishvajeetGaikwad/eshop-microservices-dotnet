using EShop.Ordering.Domain.Abstractions;

namespace EShop.Ordering.Domain.Entities;

public class OrderItem : Entity<Guid>
{
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = default!;
    public decimal Price { get; private set; }
    public int Quantity { get; private set; }

    // EF Core constructor
    internal OrderItem() { }

    internal OrderItem(Guid orderId, Guid productId, string productName, decimal price, int quantity)
    {
        Id = Guid.NewGuid();
        OrderId = orderId;
        ProductId = productId;
        ProductName = productName;
        Price = price;
        Quantity = quantity;
    }

    public void UpdateQuantity(int quantity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);
        Quantity = quantity;
    }
}
