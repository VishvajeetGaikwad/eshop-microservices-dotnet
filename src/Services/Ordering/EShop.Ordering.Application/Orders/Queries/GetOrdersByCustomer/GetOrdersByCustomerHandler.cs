using EShop.Ordering.Application.Abstractions;
using EShop.Ordering.Application.DTOs;
using EShop.Ordering.Domain.Entities;
using MediatR;

namespace EShop.Ordering.Application.Orders.Queries.GetOrdersByCustomer;

// Query
public record GetOrdersByCustomerQuery(Guid CustomerId) : IRequest<GetOrdersByCustomerResult>;

public record GetOrdersByCustomerResult(IEnumerable<OrderDto> Orders);

// Handler
internal class GetOrdersByCustomerHandler(IOrderRepository repository)
    : IRequestHandler<GetOrdersByCustomerQuery, GetOrdersByCustomerResult>
{
    public async Task<GetOrdersByCustomerResult> Handle(GetOrdersByCustomerQuery query, CancellationToken cancellationToken)
    {
        var orders = await repository.GetByCustomerAsync(query.CustomerId, cancellationToken);
        var orderDtos = orders.Select(MapToDto).ToList();
        return new GetOrdersByCustomerResult(orderDtos);
    }

    private static OrderDto MapToDto(Order order)
    {
        return new OrderDto(
            Id: order.Id,
            CustomerId: order.CustomerId,
            OrderName: order.OrderName,
            ShippingAddress: new AddressDto(
                order.ShippingAddress.FirstName,
                order.ShippingAddress.LastName,
                order.ShippingAddress.EmailAddress,
                order.ShippingAddress.AddressLine,
                order.ShippingAddress.Country,
                order.ShippingAddress.State,
                order.ShippingAddress.ZipCode),
            Payment: new PaymentDto(
                order.Payment.CardName,
                order.Payment.CardNumber,
                order.Payment.Expiration,
                order.Payment.Cvv,
                order.Payment.PaymentMethod),
            Status: order.Status,
            OrderItems: order.OrderItems.Select(i => new OrderItemDto(
                i.ProductId, i.ProductName, i.Price, i.Quantity)).ToList(),
            TotalPrice: order.TotalPrice);
    }
}
