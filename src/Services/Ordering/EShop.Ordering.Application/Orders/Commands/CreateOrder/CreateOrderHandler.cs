using EShop.Ordering.Application.Abstractions;
using EShop.Ordering.Application.DTOs;
using EShop.Ordering.Domain.Entities;
using EShop.Ordering.Domain.ValueObjects;
using FluentValidation;
using MediatR;

namespace EShop.Ordering.Application.Orders.Commands.CreateOrder;

// Command
public record CreateOrderCommand(OrderDto Order) : IRequest<CreateOrderResult>;

public record CreateOrderResult(Guid Id);

// Validator
public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.Order).NotNull();
        RuleFor(x => x.Order.OrderName).NotEmpty().MaximumLength(100).When(x => x.Order is not null);
        RuleFor(x => x.Order.CustomerId).NotEmpty().When(x => x.Order is not null);
        RuleFor(x => x.Order.OrderItems).NotEmpty().When(x => x.Order is not null);
    }
}

// Handler
internal class CreateOrderHandler(IOrderRepository repository) : IRequestHandler<CreateOrderCommand, CreateOrderResult>
{
    public async Task<CreateOrderResult> Handle(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        var dto = command.Order;

        var shippingAddress = Address.Of(
            dto.ShippingAddress.FirstName,
            dto.ShippingAddress.LastName,
            dto.ShippingAddress.EmailAddress,
            dto.ShippingAddress.AddressLine,
            dto.ShippingAddress.Country,
            dto.ShippingAddress.State,
            dto.ShippingAddress.ZipCode);

        var payment = Payment.Of(
            dto.Payment.CardName,
            dto.Payment.CardNumber,
            dto.Payment.Expiration,
            dto.Payment.Cvv,
            dto.Payment.PaymentMethod);

        var order = Order.Create(
            Guid.NewGuid(),
            dto.CustomerId,
            dto.OrderName,
            shippingAddress,
            payment);

        foreach (var item in dto.OrderItems)
        {
            order.AddItem(item.ProductId, item.ProductName, item.Price, item.Quantity);
        }

        await repository.AddAsync(order, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return new CreateOrderResult(order.Id);
    }
}
