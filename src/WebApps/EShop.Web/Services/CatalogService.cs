using System.Net.Http.Json;
using EShop.Web.Models;

namespace EShop.Web.Services;

public interface ICatalogService
{
    Task<List<ProductModel>> GetProducts();
    Task<List<ProductModel>> GetProductsByCategory(string category);
    Task<ProductModel?> GetProductById(string id);
}

public class CatalogService(HttpClient httpClient) : ICatalogService
{
    public async Task<List<ProductModel>> GetProducts()
    {
        var response = await httpClient.GetFromJsonAsync<GetProductsResponse>("/api/v1/catalog/products");
        return response?.Products?.Data ?? [];
    }

    public async Task<List<ProductModel>> GetProductsByCategory(string category)
    {
        var response = await httpClient.GetFromJsonAsync<GetProductsByCategoryResponse>($"/api/v1/catalog/products/category/{category}");
        return response?.Products ?? [];
    }

    public async Task<ProductModel?> GetProductById(string id)
    {
        return await httpClient.GetFromJsonAsync<ProductModel>($"/api/v1/catalog/products/{id}");
    }
}
