using Carter;
using EShop.Basket.API.Data;
using EShop.Basket.API.Models;
using EShop.BuildingBlocks.Common.CQRS;
using EShop.BuildingBlocks.Common.Exceptions;
using MediatR;

namespace EShop.Basket.API.Features.GetBasket;

// Query
public record GetBasketQuery(string UserName) : IQuery<GetBasketResult>;

public record GetBasketResult(ShoppingCart Cart);

// Handler
internal class GetBasketHandler(IBasketRepository repository)
    : IQueryHandler<GetBasketQuery, GetBasketResult>
{
    public async Task<GetBasketResult> Handle(GetBasketQuery query, CancellationToken cancellationToken)
    {
        var basket = await repository.GetBasketAsync(query.UserName, cancellationToken);

        if (basket is null)
        {
            // Return empty cart for new users
            basket = new ShoppingCart(query.UserName);
        }

        return new GetBasketResult(basket);
    }
}

// Endpoint
public class GetBasketEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/basket/{userName}",
            async (string userName, ISender sender) =>
            {
                var result = await sender.Send(new GetBasketQuery(userName));
                return Results.Ok(result);
            })
            .WithName("GetBasket")
            .Produces<GetBasketResult>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Get Basket")
            .WithDescription("Get shopping cart for a user");
    }
}
