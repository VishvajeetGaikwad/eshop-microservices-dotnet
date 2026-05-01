using Carter;
using EShop.BuildingBlocks.Common.CQRS;
using EShop.BuildingBlocks.Common.Exceptions;
using EShop.Catalog.API.Data;
using EShop.Catalog.API.Models;
using FluentValidation;
using MediatR;
using MongoDB.Driver;

namespace EShop.Catalog.API.Features.DeleteProduct;

// Command
public record DeleteProductCommand(string Id) : ICommand<DeleteProductResult>;

public record DeleteProductResult(bool IsSuccess);

// Validator
public class DeleteProductCommandValidator : AbstractValidator<DeleteProductCommand>
{
    public DeleteProductCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

// Handler
internal class DeleteProductHandler(ICatalogContext context)
    : ICommandHandler<DeleteProductCommand, DeleteProductResult>
{
    public async Task<DeleteProductResult> Handle(DeleteProductCommand command, CancellationToken cancellationToken)
    {
        var filter = Builders<Product>.Filter.Eq(p => p.Id, command.Id);
        var result = await context.Products.DeleteOneAsync(filter, cancellationToken);

        if (result.DeletedCount == 0)
        {
            throw new NotFoundException(nameof(Product), command.Id);
        }

        return new DeleteProductResult(true);
    }
}

// Endpoint
public class DeleteProductEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/v1/catalog/products/{id}",
            async (string id, ISender sender) =>
            {
                var result = await sender.Send(new DeleteProductCommand(id));
                return Results.Ok(result);
            })
            .WithName("DeleteProduct")
            .Produces<DeleteProductResult>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Delete Product")
            .WithDescription("Delete a product from the catalog");
    }
}
