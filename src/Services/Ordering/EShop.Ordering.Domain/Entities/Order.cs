using EShop.Ordering.Domain.Abstractions;
using EShop.Ordering.Domain.Enums;
using EShop.Ordering.Domain.Events;
using EShop.Ordering.Domain.ValueObjects;

namespace EShop.Ordering.Domain.Entities;

public class Order : Aggregate<Guid>
{
    private readonly List<OrderItem> _orderItems = [];
    public IReadOnlyList<OrderItem> OrderItems => _orderItems.AsReadOnly();

    public Guid CustomerId { get; private set; }
    public string OrderName { get; private set; } = default!;
    public Address ShippingAddress { get; private set; } = default!;
    public Payment Payment { get; private set; } = default!;
    public OrderStatus Status { get; private set; } = OrderStatus.Pending;
    public decimal TotalPrice => _orderItems.Sum(x => x.Price * x.Quantity);

    // EF Core constructor
    private Order() { }

    public static Order Create(Guid id, Guid customerId, string orderName,
        Address shippingAddress, Payment payment)
    {
        var order = new Order
        {
            Id = id,
            CustomerId = customerId,
            OrderName = orderName,
            ShippingAddress = shippingAddress,
            Payment = payment,
            Status = OrderStatus.Pending
        };

        order.AddDomainEvent(new OrderCreatedEvent(order));
        return order;
    }

    public void Update(string orderName, Address shippingAddress, Payment payment, OrderStatus status)
    {
        OrderName = orderName;
        ShippingAddress = shippingAddress;
        Payment = payment;
        Status = status;

        AddDomainEvent(new OrderUpdatedEvent(this));
    }

    public void AddItem(Guid productId, string productName, decimal price, int quantity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(price);

        var existingItem = _orderItems.FirstOrDefault(x => x.ProductId == productId);
        if (existingItem is not null)
        {
            existingItem.UpdateQuantity(existingItem.Quantity + quantity);
        }
        else
        {
            var orderItem = new OrderItem(Id, productId, productName, price, quantity);
            _orderItems.Add(orderItem);
        }
    }

    public void RemoveItem(Guid productId)
    {
        var item = _orderItems.FirstOrDefault(x => x.ProductId == productId);
        if (item is not null)
        {
            _orderItems.Remove(item);
        }
    }
}
