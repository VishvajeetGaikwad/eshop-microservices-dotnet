using Carter;
using EShop.BuildingBlocks.Common.CQRS;
using EShop.Catalog.API.Data;
using EShop.Catalog.API.Models;
using FluentValidation;
using MediatR;

namespace EShop.Catalog.API.Features.CreateProduct;

// Command
public record CreateProductCommand(
    string Name,
    List<string> Category,
    string Description,
    string ImageFile,
    decimal Price) : ICommand<CreateProductResult>;

public record CreateProductResult(string Id);

// Validator
public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Category).NotEmpty();
        RuleFor(x => x.Description).NotEmpty();
        RuleFor(x => x.ImageFile).NotEmpty();
        RuleFor(x => x.Price).GreaterThan(0);
    }
}

// Handler
internal class CreateProductHandler(ICatalogContext context)
    : ICommandHandler<CreateProductCommand, CreateProductResult>
{
    public async Task<CreateProductResult> Handle(CreateProductCommand command, CancellationToken cancellationToken)
    {
        var product = new Product
        {
            Name = command.Name,
            Category = command.Category,
            Description = command.Description,
            ImageFile = command.ImageFile,
            Price = command.Price
        };

        await context.Products.InsertOneAsync(product, cancellationToken: cancellationToken);

        return new CreateProductResult(product.Id);
    }
}

// Endpoint
public class CreateProductEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/catalog/products",
            async (CreateProductCommand command, ISender sender) =>
            {
                var result = await sender.Send(command);
                return Results.Created($"/api/v1/catalog/products/{result.Id}", result);
            })
            .WithName("CreateProduct")
            .Produces<CreateProductResult>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Create Product")
            .WithDescription("Creates a new product in the catalog");
    }
}
