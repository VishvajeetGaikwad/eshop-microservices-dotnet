using System.Net.Http.Json;
using EShop.Web.Models;

namespace EShop.Web.Services;

public interface IOrderService
{
    Task<List<OrderModel>> GetOrders();
    Task<List<OrderModel>> GetOrdersByCustomer(Guid customerId);
    Task<Guid?> CreateOrder(OrderModel order);
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

    public async Task<Guid?> CreateOrder(OrderModel order)
    {
        var response = await httpClient.PostAsJsonAsync("/api/v1/ordering/orders", order);
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<CreateOrderResponse>();
            return result?.Id;
        }
        return null;
    }
}
