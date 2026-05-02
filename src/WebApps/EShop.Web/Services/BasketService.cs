using System.Net.Http.Json;
using EShop.Web.Models;

namespace EShop.Web.Services;

public interface IBasketService
{
    Task<ShoppingCart> GetBasket(string userName);
    Task StoreBasket(ShoppingCart cart);
    Task DeleteBasket(string userName);
    Task<bool> Checkout(BasketCheckoutRequest request);
}

public class BasketService(HttpClient httpClient) : IBasketService
{
    public async Task<ShoppingCart> GetBasket(string userName)
    {
        var response = await httpClient.GetFromJsonAsync<GetBasketResponse>($"/api/v1/basket/{userName}");
        return response?.Cart ?? new ShoppingCart { UserName = userName };
    }

    public async Task StoreBasket(ShoppingCart cart)
    {
        var request = new StoreBasketRequest
        {
            UserName = cart.UserName,
            Items = cart.Items
        };
        await httpClient.PostAsJsonAsync("/api/v1/basket", request);
    }

    public async Task DeleteBasket(string userName)
    {
        await httpClient.DeleteAsync($"/api/v1/basket/{userName}");
    }

    public async Task<bool> Checkout(BasketCheckoutRequest request)
    {
        var response = await httpClient.PostAsJsonAsync("/api/v1/basket/checkout", request);
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<CheckoutResponse>();
            return result?.IsSuccess ?? false;
        }
        return false;
    }
}
