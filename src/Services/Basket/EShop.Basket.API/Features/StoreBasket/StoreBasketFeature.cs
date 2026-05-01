using Carter;
using EShop.Basket.API.Data;
using EShop.Basket.API.Models;
using EShop.BuildingBlocks.Common.CQRS;
using FluentValidation;
using MediatR;

namespace EShop.Basket.API.Features.StoreBasket;

// Command
public record StoreBasketCommand(ShoppingCart Cart) : ICommand<StoreBasketResult>;

public record StoreBasketResult(string UserName);

// Validator
public class StoreBasketCommandValidator : AbstractValidator<StoreBasketCommand>
{
    public StoreBasketCommandValidator()
    {
        RuleFor(x => x.Cart).NotNull();
        RuleFor(x => x.Cart.UserName).NotEmpty().When(x => x.Cart is not null);
    }
}

// Handler
internal class StoreBasketHandler(IBasketRepository repository)
    : ICommandHandler<StoreBasketCommand, StoreBasketResult>
{
    public async Task<StoreBasketResult> Handle(StoreBasketCommand command, CancellationToken cancellationToken)
    {
        var basket = await repository.StoreBasketAsync(command.Cart, cancellationToken);
        return new StoreBasketResult(basket.UserName);
    }
}

// Endpoint
public class StoreBasketEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/basket",
            async (ShoppingCart cart, ISender sender) =>
            {
                var command = new StoreBasketCommand(cart);
                var result = await sender.Send(command);
                return Results.Ok(result);
            })
            .WithName("StoreBasket")
            .Produces<StoreBasketResult>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Store Basket")
            .WithDescription("Store or update shopping cart for a user");
    }
}
