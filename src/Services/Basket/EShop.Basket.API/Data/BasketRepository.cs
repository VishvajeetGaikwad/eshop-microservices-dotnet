using EShop.Basket.API.Models;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace EShop.Basket.API.Data;

public interface IBasketRepository
{
    Task<ShoppingCart?> GetBasketAsync(string userName, CancellationToken cancellationToken = default);
    Task<ShoppingCart> StoreBasketAsync(ShoppingCart basket, CancellationToken cancellationToken = default);
    Task<bool> DeleteBasketAsync(string userName, CancellationToken cancellationToken = default);
}

public class BasketRepository(IDistributedCache cache) : IBasketRepository
{
    public async Task<ShoppingCart?> GetBasketAsync(string userName, CancellationToken cancellationToken)
    {
        var basket = await cache.GetStringAsync(userName, cancellationToken);
        return string.IsNullOrEmpty(basket) ? null : JsonSerializer.Deserialize<ShoppingCart>(basket);
    }

    public async Task<ShoppingCart> StoreBasketAsync(ShoppingCart basket, CancellationToken cancellationToken)
    {
        await cache.SetStringAsync(
            basket.UserName,
            JsonSerializer.Serialize(basket),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(30)
            },
            cancellationToken);

        return basket;
    }

    public async Task<bool> DeleteBasketAsync(string userName, CancellationToken cancellationToken)
    {
        await cache.RemoveAsync(userName, cancellationToken);
        return true;
    }
}
