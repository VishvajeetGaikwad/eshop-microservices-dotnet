using EShop.Ordering.Domain.Enums;

namespace EShop.Ordering.Application.DTOs;

public record OrderDto(
    Guid Id,
    Guid CustomerId,
    string OrderName,
    AddressDto ShippingAddress,
    PaymentDto Payment,
    OrderStatus Status,
    List<OrderItemDto> OrderItems,
    decimal TotalPrice);

public record AddressDto(
    string FirstName,
    string LastName,
    string EmailAddress,
    string AddressLine,
    string Country,
    string State,
    string ZipCode);

public record PaymentDto(
    string CardName,
    string CardNumber,
    string Expiration,
    string Cvv,
    int PaymentMethod);

public record OrderItemDto(
    Guid ProductId,
    string ProductName,
    decimal Price,
    int Quantity);
