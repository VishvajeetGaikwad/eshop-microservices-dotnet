using EShop.Ordering.Application.Abstractions;
using FluentValidation;
using MediatR;

namespace EShop.Ordering.Application.Orders.Commands.DeleteOrder;

// Command
public record DeleteOrderCommand(Guid OrderId) : IRequest<DeleteOrderResult>;

public record DeleteOrderResult(bool IsSuccess);

// Validator
public class DeleteOrderCommandValidator : AbstractValidator<DeleteOrderCommand>
{
    public DeleteOrderCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
    }
}

// Handler
internal class DeleteOrderHandler(IOrderRepository repository) : IRequestHandler<DeleteOrderCommand, DeleteOrderResult>
{
    public async Task<DeleteOrderResult> Handle(DeleteOrderCommand command, CancellationToken cancellationToken)
    {
        var order = await repository.GetByIdAsync(command.OrderId, cancellationToken);
        if (order is null)
        {
            throw new InvalidOperationException($"Order {command.OrderId} not found");
        }

        repository.Delete(order);
        await repository.SaveChangesAsync(cancellationToken);

        return new DeleteOrderResult(true);
    }
}
