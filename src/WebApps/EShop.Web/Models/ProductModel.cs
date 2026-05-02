namespace EShop.Web.Models;

public class ProductModel
{
    public string Id { get; set; } = default!;
    public string Name { get; set; } = default!;
    public List<string> Category { get; set; } = [];
    public string Description { get; set; } = default!;
    public string ImageFile { get; set; } = default!;
    public decimal Price { get; set; }
}

public class GetProductsResponse
{
    public ProductPaginatedResult Products { get; set; } = default!;
}

public class ProductPaginatedResult
{
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
    public int Count { get; set; }
    public List<ProductModel> Data { get; set; } = [];
}

public class GetProductsByCategoryResponse
{
    public List<ProductModel> Products { get; set; } = [];
}
