using MediatR;

namespace EShop.Ordering.Domain.Abstractions;

public interface IDomainEvent : INotification
{
    DateTime OccurredOn => DateTime.UtcNow;
}
