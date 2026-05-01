using Carter;
using EShop.BuildingBlocks.Common.CQRS;
using EShop.BuildingBlocks.Common.Exceptions;
using EShop.Catalog.API.Data;
using EShop.Catalog.API.Models;
using MediatR;
using MongoDB.Driver;

namespace EShop.Catalog.API.Features.GetProductById;

// Query
public record GetProductByIdQuery(string Id) : IQuery<GetProductByIdResult>;

public record GetProductByIdResult(Product Product);

// Handler
internal class GetProductByIdHandler(ICatalogContext context)
    : IQueryHandler<GetProductByIdQuery, GetProductByIdResult>
{
    public async Task<GetProductByIdResult> Handle(GetProductByIdQuery query, CancellationToken cancellationToken)
    {
        var filter = Builders<Product>.Filter.Eq(p => p.Id, query.Id);
        var product = await context.Products
            .Find(filter)
            .FirstOrDefaultAsync(cancellationToken);

        if (product is null)
        {
            throw new NotFoundException(nameof(Product), query.Id);
        }

        return new GetProductByIdResult(product);
    }
}

// Endpoint
public class GetProductByIdEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/catalog/products/{id}",
            async (string id, ISender sender) =>
            {
                var result = await sender.Send(new GetProductByIdQuery(id));
                return Results.Ok(result);
            })
            .WithName("GetProductById")
            .Produces<GetProductByIdResult>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Get Product By Id")
            .WithDescription("Get a specific product by its id");
    }
}
