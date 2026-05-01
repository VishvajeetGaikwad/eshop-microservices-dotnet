using Carter;
using EShop.BuildingBlocks.Common.CQRS;
using EShop.Catalog.API.Data;
using EShop.Catalog.API.Models;
using MediatR;
using MongoDB.Driver;

namespace EShop.Catalog.API.Features.GetProductByCategory;

// Query
public record GetProductByCategoryQuery(string Category) : IQuery<GetProductByCategoryResult>;

public record GetProductByCategoryResult(IEnumerable<Product> Products);

// Handler
internal class GetProductByCategoryHandler(ICatalogContext context)
    : IQueryHandler<GetProductByCategoryQuery, GetProductByCategoryResult>
{
    public async Task<GetProductByCategoryResult> Handle(GetProductByCategoryQuery query, CancellationToken cancellationToken)
    {
        var filter = Builders<Product>.Filter
            .AnyEq(p => p.Category, query.Category);

        var products = await context.Products
            .Find(filter)
            .ToListAsync(cancellationToken);

        return new GetProductByCategoryResult(products);
    }
}

// Endpoint
public class GetProductByCategoryEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/catalog/products/category/{category}",
            async (string category, ISender sender) =>
            {
                var result = await sender.Send(new GetProductByCategoryQuery(category));
                return Results.Ok(result);
            })
            .WithName("GetProductByCategory")
            .Produces<GetProductByCategoryResult>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Get Products By Category")
            .WithDescription("Get products filtered by category");
    }
}
