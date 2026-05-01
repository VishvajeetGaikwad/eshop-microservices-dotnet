using EShop.BuildingBlocks.Messaging.Events;
using EShop.Ordering.Application.Abstractions;
using EShop.Ordering.Domain.Entities;
using EShop.Ordering.Domain.ValueObjects;
using MassTransit;

namespace EShop.Ordering.API.EventHandlers;

public class BasketCheckoutEventConsumer(
    IOrderRepository orderRepository,
    ILogger<BasketCheckoutEventConsumer> logger) : IConsumer<BasketCheckoutEvent>
{
    public async Task Consume(ConsumeContext<BasketCheckoutEvent> context)
    {
        var message = context.Message;

        logger.LogInformation("Consuming BasketCheckoutEvent for user: {UserName}", message.UserName);

        var shippingAddress = Address.Of(
            message.FirstName,
            message.LastName,
            message.EmailAddress,
            message.AddressLine,
            message.Country,
            message.State,
            message.ZipCode);

        var payment = Payment.Of(
            message.CardName,
            message.CardNumber,
            message.Expiration,
            message.Cvv,
            message.PaymentMethod);

        var order = Order.Create(
            Guid.NewGuid(),
            message.CustomerId,
            $"Order-{message.UserName}-{DateTime.UtcNow:yyyyMMdd}",
            shippingAddress,
            payment);

        // Add a generic item representing the basket total
        order.AddItem(
            Guid.NewGuid(),
            "Basket Checkout",
            message.TotalPrice,
            1);

        await orderRepository.AddAsync(order);
        await orderRepository.SaveChangesAsync();

        logger.LogInformation("Order {OrderId} created successfully from BasketCheckoutEvent", order.Id);
    }
}
