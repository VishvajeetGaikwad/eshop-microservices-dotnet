using System.Net.Http.Json;
using EShop.Web.Models;

namespace EShop.Web.Services;

public interface IOrderService
{
    Task<List<OrderModel>> GetOrders();
    Task<List<OrderModel>> GetOrdersByCustomer(Guid customerId);
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
}
