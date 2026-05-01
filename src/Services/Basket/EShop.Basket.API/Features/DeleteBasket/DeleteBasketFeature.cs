using Carter;
using EShop.Basket.API.Data;
using EShop.BuildingBlocks.Common.CQRS;
using FluentValidation;
using MediatR;

namespace EShop.Basket.API.Features.DeleteBasket;

// Command
public record DeleteBasketCommand(string UserName) : ICommand<DeleteBasketResult>;

public record DeleteBasketResult(bool IsSuccess);

// Validator
public class DeleteBasketCommandValidator : AbstractValidator<DeleteBasketCommand>
{
    public DeleteBasketCommandValidator()
    {
        RuleFor(x => x.UserName).NotEmpty();
    }
}

// Handler
internal class DeleteBasketHandler(IBasketRepository repository)
    : ICommandHandler<DeleteBasketCommand, DeleteBasketResult>
{
    public async Task<DeleteBasketResult> Handle(DeleteBasketCommand command, CancellationToken cancellationToken)
    {
        var result = await repository.DeleteBasketAsync(command.UserName, cancellationToken);
        return new DeleteBasketResult(result);
    }
}

// Endpoint
public class DeleteBasketEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/v1/basket/{userName}",
            async (string userName, ISender sender) =>
            {
                var result = await sender.Send(new DeleteBasketCommand(userName));
                return Results.Ok(result);
            })
            .WithName("DeleteBasket")
            .Produces<DeleteBasketResult>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Delete Basket")
            .WithDescription("Delete shopping cart for a user");
    }
}
