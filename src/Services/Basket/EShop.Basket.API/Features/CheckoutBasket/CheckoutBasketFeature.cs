using Carter;
using EShop.Basket.API.Data;
using EShop.BuildingBlocks.Common.CQRS;
using EShop.BuildingBlocks.Common.Exceptions;
using EShop.BuildingBlocks.Messaging.Events;
using FluentValidation;
using MassTransit;
using MediatR;

namespace EShop.Basket.API.Features.CheckoutBasket;

// Command
public record CheckoutBasketCommand(BasketCheckoutDto BasketCheckout) : ICommand<CheckoutBasketResult>;

public record CheckoutBasketResult(bool IsSuccess);

public record BasketCheckoutDto(
    string UserName,
    Guid CustomerId,
    string FirstName,
    string LastName,
    string EmailAddress,
    string AddressLine,
    string Country,
    string State,
    string ZipCode,
    string CardName,
    string CardNumber,
    string Expiration,
    string Cvv,
    int PaymentMethod);

// Validator
public class CheckoutBasketCommandValidator : AbstractValidator<CheckoutBasketCommand>
{
    public CheckoutBasketCommandValidator()
    {
        RuleFor(x => x.BasketCheckout).NotNull();
        RuleFor(x => x.BasketCheckout.UserName).NotEmpty().When(x => x.BasketCheckout is not null);
        RuleFor(x => x.BasketCheckout.FirstName).NotEmpty().When(x => x.BasketCheckout is not null);
        RuleFor(x => x.BasketCheckout.LastName).NotEmpty().When(x => x.BasketCheckout is not null);
        RuleFor(x => x.BasketCheckout.EmailAddress).NotEmpty().EmailAddress().When(x => x.BasketCheckout is not null);
    }
}

// Handler
internal class CheckoutBasketHandler(
    IBasketRepository repository,
    IPublishEndpoint publishEndpoint)
    : ICommandHandler<CheckoutBasketCommand, CheckoutBasketResult>
{
    public async Task<CheckoutBasketResult> Handle(CheckoutBasketCommand command, CancellationToken cancellationToken)
    {
        var basket = await repository.GetBasketAsync(command.BasketCheckout.UserName, cancellationToken);

        if (basket is null || basket.Items.Count == 0)
        {
            throw new BadRequestException("Basket is empty or does not exist for user: " + command.BasketCheckout.UserName);
        }

        // Publish BasketCheckoutEvent to RabbitMQ
        var eventMessage = new BasketCheckoutEvent
        {
            UserName = command.BasketCheckout.UserName,
            CustomerId = command.BasketCheckout.CustomerId,
            TotalPrice = basket.TotalPrice,
            FirstName = command.BasketCheckout.FirstName,
            LastName = command.BasketCheckout.LastName,
            EmailAddress = command.BasketCheckout.EmailAddress,
            AddressLine = command.BasketCheckout.AddressLine,
            Country = command.BasketCheckout.Country,
            State = command.BasketCheckout.State,
            ZipCode = command.BasketCheckout.ZipCode,
            CardName = command.BasketCheckout.CardName,
            CardNumber = command.BasketCheckout.CardNumber,
            Expiration = command.BasketCheckout.Expiration,
            Cvv = command.BasketCheckout.Cvv,
            PaymentMethod = command.BasketCheckout.PaymentMethod
        };

        await publishEndpoint.Publish(eventMessage, cancellationToken);

        // Clear the basket after checkout
        await repository.DeleteBasketAsync(command.BasketCheckout.UserName, cancellationToken);

        return new CheckoutBasketResult(true);
    }
}

// Endpoint
public class CheckoutBasketEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/basket/checkout",
            async (BasketCheckoutDto dto, ISender sender) =>
            {
                var command = new CheckoutBasketCommand(dto);
                var result = await sender.Send(command);
                return Results.Ok(result);
            })
            .WithName("CheckoutBasket")
            .Produces<CheckoutBasketResult>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Checkout Basket")
            .WithDescription("Checkout the basket — publishes event and clears cart");
    }
}
