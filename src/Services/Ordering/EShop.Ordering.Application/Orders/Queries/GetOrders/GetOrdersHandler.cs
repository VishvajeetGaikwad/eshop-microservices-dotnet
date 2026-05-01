using EShop.Ordering.Application.Abstractions;
using EShop.Ordering.Application.DTOs;
using EShop.Ordering.Domain.Entities;
using MediatR;

namespace EShop.Ordering.Application.Orders.Queries.GetOrders;

// Query
public record GetOrdersQuery : IRequest<GetOrdersResult>;

public record GetOrdersResult(IEnumerable<OrderDto> Orders);

// Handler
internal class GetOrdersHandler(IOrderRepository repository) : IRequestHandler<GetOrdersQuery, GetOrdersResult>
{
    public async Task<GetOrdersResult> Handle(GetOrdersQuery query, CancellationToken cancellationToken)
    {
        var orders = await repository.GetAllAsync(cancellationToken);
        var orderDtos = orders.Select(MapToDto).ToList();
        return new GetOrdersResult(orderDtos);
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
