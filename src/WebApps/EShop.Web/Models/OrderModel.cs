namespace EShop.Web.Models;

public class OrderModel
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string OrderName { get; set; } = default!;
    public AddressModel ShippingAddress { get; set; } = default!;
    public PaymentModel Payment { get; set; } = default!;
    public int Status { get; set; }
    public List<OrderItemModel> OrderItems { get; set; } = [];
    public decimal TotalPrice { get; set; }
}

public class AddressModel
{
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string EmailAddress { get; set; } = default!;
    public string AddressLine { get; set; } = default!;
    public string Country { get; set; } = default!;
    public string State { get; set; } = default!;
    public string ZipCode { get; set; } = default!;
}

public class PaymentModel
{
    public string CardName { get; set; } = default!;
    public string CardNumber { get; set; } = default!;
    public string Expiration { get; set; } = default!;
    public string Cvv { get; set; } = default!;
    public int PaymentMethod { get; set; }
}

public class OrderItemModel
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = default!;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}

public class GetOrdersResponse
{
    public List<OrderModel> Orders { get; set; } = [];
}

public class CreateOrderResponse
{
    public Guid Id { get; set; }
}

public static class OrderStatusHelper
{
    public static string GetStatusText(int status) => status switch
    {
        0 => "Draft",
        1 => "Pending",
        2 => "Completed",
        3 => "Cancelled",
        _ => "Unknown"
    };

    public static string GetStatusBadgeClass(int status) => status switch
    {
        0 => "bg-secondary",
        1 => "bg-warning text-dark",
        2 => "bg-success",
        3 => "bg-danger",
        _ => "bg-secondary"
    };
}
