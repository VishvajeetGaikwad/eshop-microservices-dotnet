using EShop.Catalog.API.Models;
using MongoDB.Driver;

namespace EShop.Catalog.API.Data;

public interface ICatalogContext
{
    IMongoCollection<Product> Products { get; }
}

public class CatalogContext : ICatalogContext
{
    public CatalogContext(IConfiguration configuration)
    {
        var client = new MongoClient(configuration.GetValue<string>("DatabaseSettings:ConnectionString"));
        var database = client.GetDatabase(configuration.GetValue<string>("DatabaseSettings:DatabaseName"));

        Products = database.GetCollection<Product>(
            configuration.GetValue<string>("DatabaseSettings:CollectionName"));

        SeedData(Products);
    }

    public IMongoCollection<Product> Products { get; }

    private static void SeedData(IMongoCollection<Product> products)
    {
        var exists = products.Find(Builders<Product>.Filter.Empty).Any();
        if (!exists)
        {
            products.InsertMany(GetPreconfiguredProducts());
        }
    }

    private static IEnumerable<Product> GetPreconfiguredProducts()
    {
        return new List<Product>
        {
            new()
            {
                Name = "iPhone 15 Pro",
                Description = "Apple's flagship smartphone with A17 Pro chip",
                ImageFile = "product-1.png",
                Price = 999.99m,
                Category = ["Smartphones", "Electronics"]
            },
            new()
            {
                Name = "Samsung Galaxy S24 Ultra",
                Description = "Samsung's premium smartphone with AI features",
                ImageFile = "product-2.png",
                Price = 1199.99m,
                Category = ["Smartphones", "Electronics"]
            },
            new()
            {
                Name = "Sony WH-1000XM5",
                Description = "Industry-leading noise cancelling headphones",
                ImageFile = "product-3.png",
                Price = 349.99m,
                Category = ["Audio", "Electronics"]
            },
            new()
            {
                Name = "MacBook Pro 16\" M3 Max",
                Description = "Apple's most powerful laptop for professionals",
                ImageFile = "product-4.png",
                Price = 3499.99m,
                Category = ["Laptops", "Electronics"]
            },
            new()
            {
                Name = "Logitech MX Master 3S",
                Description = "Advanced wireless mouse for productivity",
                ImageFile = "product-5.png",
                Price = 99.99m,
                Category = ["Accessories", "Peripherals"]
            },
            new()
            {
                Name = "Dell UltraSharp U2723QE",
                Description = "27-inch 4K USB-C Hub Monitor",
                ImageFile = "product-6.png",
                Price = 619.99m,
                Category = ["Monitors", "Electronics"]
            }
        };
    }
}
