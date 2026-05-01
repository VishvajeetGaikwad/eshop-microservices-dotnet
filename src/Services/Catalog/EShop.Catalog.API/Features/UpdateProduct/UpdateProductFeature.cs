using Carter;
using EShop.BuildingBlocks.Common.CQRS;
using EShop.BuildingBlocks.Common.Exceptions;
using EShop.Catalog.API.Data;
using EShop.Catalog.API.Models;
using FluentValidation;
using MediatR;
using MongoDB.Driver;

namespace EShop.Catalog.API.Features.UpdateProduct;

// Command
public record UpdateProductCommand(
    string Id,
    string Name,
    List<string> Category,
    string Description,
    string ImageFile,
    decimal Price) : ICommand<UpdateProductResult>;

public record UpdateProductResult(bool IsSuccess);

// Validator
public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Price).GreaterThan(0);
    }
}

// Handler
internal class UpdateProductHandler(ICatalogContext context)
    : ICommandHandler<UpdateProductCommand, UpdateProductResult>
{
    public async Task<UpdateProductResult> Handle(UpdateProductCommand command, CancellationToken cancellationToken)
    {
        var filter = Builders<Product>.Filter.Eq(p => p.Id, command.Id);

        var update = Builders<Product>.Update
            .Set(p => p.Name, command.Name)
            .Set(p => p.Category, command.Category)
            .Set(p => p.Description, command.Description)
            .Set(p => p.ImageFile, command.ImageFile)
            .Set(p => p.Price, command.Price);

        var result = await context.Products.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);

        if (result.MatchedCount == 0)
        {
            throw new NotFoundException(nameof(Product), command.Id);
        }

        return new UpdateProductResult(result.ModifiedCount > 0);
    }
}

// Endpoint
public class UpdateProductEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/v1/catalog/products",
            async (UpdateProductCommand command, ISender sender) =>
            {
                var result = await sender.Send(command);
                return Results.Ok(result);
            })
            .WithName("UpdateProduct")
            .Produces<UpdateProductResult>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Update Product")
            .WithDescription("Update an existing product in the catalog");
    }
}
