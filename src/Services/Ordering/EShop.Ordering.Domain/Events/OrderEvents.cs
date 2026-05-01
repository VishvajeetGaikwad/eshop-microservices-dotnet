using EShop.Ordering.Domain.Abstractions;
using EShop.Ordering.Domain.Entities;

namespace EShop.Ordering.Domain.Events;

public record OrderCreatedEvent(Order Order) : IDomainEvent;

public record OrderUpdatedEvent(Order Order) : IDomainEvent;
