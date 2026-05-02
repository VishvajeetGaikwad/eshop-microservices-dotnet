using System.Net.Http.Json;
using EShop.Web.Models;

namespace EShop.Web.Services;

public interface IOrderService
{
    Task<List<OrderModel>> GetOrders();
    Task<List<OrderModel>> GetOrdersByCustomer(Guid customerId);
    Task<Guid?> CreateOrder(OrderModel order, string? idempotencyKey = null);
    Task<bool> DeleteOrder(Guid orderId);
}

public class OrderService(HttpClient httpClient) : IOrderService
{
    public async Task<List<OrderModel>> GetOrders()
    {
        var response = await httpClient.GetFromJsonAsync<GetOrdersResponse>("/api/v1/ordering/orders");
        return response?.Orders ?? [];
    }

    public async Task<List<OrderModel>> GetOrdersByCustomer(Guid customerId)
    {
        var response = await httpClient.GetFromJsonAsync<GetOrdersResponse>($"/api/v1/ordering/orders/customer/{customerId}");
        return response?.Orders ?? [];
    }

    /// <summary>
    /// Creates an order with idempotency key support.
    /// The idempotency key ensures that retrying the same request (e.g., due to
    /// network timeout or user double-click) won't create duplicate orders.
    /// </summary>
    public async Task<Guid?> CreateOrder(OrderModel order, string? idempotencyKey = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/ordering/orders")
        {
            Content = JsonContent.Create(order)
        };

        // Attach idempotency key header if provided
        if (!string.IsNullOrEmpty(idempotencyKey))
        {
            request.Headers.Add("Idempotency-Key", idempotencyKey);
        }

        var response = await httpClient.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<CreateOrderResponse>();
            return result?.Id;
        }
        return null;
    }

    /// <summary>
    /// Deletes an order - used as compensation in the Checkout Saga
    /// when a downstream step fails after order creation.
    /// </summary>
    public async Task<bool> DeleteOrder(Guid orderId)
    {
        var response = await httpClient.DeleteAsync($"/api/v1/ordering/orders/{orderId}");
        return response.IsSuccessStatusCode;
    }
}
