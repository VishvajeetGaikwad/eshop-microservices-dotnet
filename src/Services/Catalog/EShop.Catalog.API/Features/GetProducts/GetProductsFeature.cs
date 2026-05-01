using Carter;
using EShop.BuildingBlocks.Common.CQRS;
using EShop.BuildingBlocks.Common.Pagination;
using EShop.Catalog.API.Data;
using EShop.Catalog.API.Models;
using MediatR;
using MongoDB.Driver;

namespace EShop.Catalog.API.Features.GetProducts;

// Query
public record GetProductsQuery(int PageIndex = 0, int PageSize = 10) : IQuery<GetProductsResult>;

public record GetProductsResult(PaginatedResult<Product> Products);

// Handler
internal class GetProductsHandler(ICatalogContext context)
    : IQueryHandler<GetProductsQuery, GetProductsResult>
{
    public async Task<GetProductsResult> Handle(GetProductsQuery query, CancellationToken cancellationToken)
    {
        var totalCount = await context.Products.CountDocumentsAsync(
            Builders<Product>.Filter.Empty, cancellationToken: cancellationToken);

        var products = await context.Products
            .Find(Builders<Product>.Filter.Empty)
            .Skip(query.PageIndex * query.PageSize)
            .Limit(query.PageSize)
            .ToListAsync(cancellationToken);

        var result = new PaginatedResult<Product>(
            query.PageIndex, query.PageSize, totalCount, products);

        return new GetProductsResult(result);
    }
}

// Endpoint
public class GetProductsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/catalog/products",
            async ([AsParameters] PaginationRequest request, ISender sender) =>
            {
                var query = new GetProductsQuery(request.PageIndex, request.PageSize);
                var result = await sender.Send(query);
                return Results.Ok(result);
            })
            .WithName("GetProducts")
            .Produces<GetProductsResult>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Get Products")
            .WithDescription("Get paginated list of products from catalog");
    }
}
