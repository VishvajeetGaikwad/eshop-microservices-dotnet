using EShop.Ordering.Application.Abstractions;
using EShop.Ordering.Application.DTOs;
using EShop.Ordering.Domain.ValueObjects;
using FluentValidation;
using MediatR;

namespace EShop.Ordering.Application.Orders.Commands.UpdateOrder;

// Command
public record UpdateOrderCommand(OrderDto Order) : IRequest<UpdateOrderResult>;

public record UpdateOrderResult(bool IsSuccess);

// Validator
public class UpdateOrderCommandValidator : AbstractValidator<UpdateOrderCommand>
{
    public UpdateOrderCommandValidator()
    {
        RuleFor(x => x.Order.Id).NotEmpty();
        RuleFor(x => x.Order.OrderName).NotEmpty();
    }
}

// Handler
internal class UpdateOrderHandler(IOrderRepository repository) : IRequestHandler<UpdateOrderCommand, UpdateOrderResult>
{
    public async Task<UpdateOrderResult> Handle(UpdateOrderCommand command, CancellationToken cancellationToken)
    {
        var order = await repository.GetByIdAsync(command.Order.Id, cancellationToken);
        if (order is null)
        {
            throw new InvalidOperationException($"Order {command.Order.Id} not found");
        }

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

        order.Update(dto.OrderName, shippingAddress, payment, dto.Status);

        repository.Update(order);
        await repository.SaveChangesAsync(cancellationToken);

        return new UpdateOrderResult(true);
    }
}
