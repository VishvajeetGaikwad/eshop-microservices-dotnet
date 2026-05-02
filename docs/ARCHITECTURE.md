# Architecture Deep Dive

This document details the architectural decisions, patterns, and service design of the EShop Microservices system.

## System Architecture

The system follows a **microservices architecture** with bounded contexts aligned to business capabilities:

| Service | Responsibility | Architecture Style | Database |
|---------|---------------|-------------------|----------|
| Catalog | Product management | Vertical Slice + CQRS | MongoDB (Mongo2Go) |
| Basket | Shopping cart management | Vertical Slice + CQRS | Distributed Memory Cache |
| Ordering | Order lifecycle management | Clean Architecture + DDD | EF Core InMemory |
| Gateway | Request routing & aggregation | Reverse Proxy | — |
| Web | User interface | Blazor WASM SPA | — |

## Architecture Patterns by Service

### Catalog Service — Vertical Slice Architecture

Each feature is self-contained in a single folder with its own request/response/handler:

```
Catalog.API/
├── Products/
│   ├── CreateProduct/
│   │   ├── CreateProductCommand.cs      # Command + Handler in one file
│   │   └── CreateProductEndpoint.cs     # Carter endpoint
│   ├── GetProducts/
│   │   ├── GetProductsQuery.cs          # Query + Handler
│   │   └── GetProductsEndpoint.cs
│   ├── GetProductsByCategory/
│   ├── UpdateProduct/
│   └── DeleteProduct/
├── Data/
│   └── CatalogDbContext.cs              # MongoDB context with seed data
├── Models/
│   └── Product.cs
└── Program.cs
```

**Why Vertical Slice?** Catalog is a straightforward CRUD service. Vertical slices keep related code together, making it easy to understand and modify a feature without jumping across layers.

### Ordering Service — Clean Architecture + DDD

The most complex service uses full Domain-Driven Design with Clean Architecture layers:

```
Ordering/
├── EShop.Ordering.Domain/               # Innermost layer - no dependencies
│   ├── Models/
│   │   ├── Order.cs                     # Aggregate Root
│   │   └── OrderItem.cs                 # Entity
│   ├── ValueObjects/
│   │   ├── OrderName.cs                 # Self-validating value object
│   │   ├── Address.cs
│   │   └── Payment.cs
│   ├── Enums/
│   │   └── OrderStatus.cs
│   ├── Events/
│   │   └── OrderCreatedEvent.cs         # Domain event
│   └── Abstractions/
│       └── IDomainEvent.cs
│
├── EShop.Ordering.Application/          # Use cases - depends only on Domain
│   ├── Orders/
│   │   ├── Commands/
│   │   │   ├── CreateOrder/
│   │   │   ├── UpdateOrder/
│   │   │   └── DeleteOrder/
│   │   └── Queries/
│   │       ├── GetOrders/
│   │       └── GetOrdersByCustomer/
│   ├── DTOs/
│   │   └── OrderDto.cs
│   └── DependencyInjection.cs
│
├── EShop.Ordering.Infrastructure/       # External concerns
│   ├── Data/
│   │   ├── OrderingDbContext.cs         # EF Core configuration
│   │   └── Extensions/                  # Seed data
│   └── DependencyInjection.cs
│
└── EShop.Ordering.API/                  # Outer layer - composition root
    ├── Endpoints/
    │   ├── CreateOrderEndpoint.cs
    │   ├── GetOrdersEndpoint.cs
    │   └── ...
    ├── Idempotency/
    │   ├── IdempotencyService.cs        # Deduplication logic
    │   └── IdempotencyFilter.cs         # Endpoint filter
    └── Program.cs                       # DI composition
```

**Why DDD here?** Orders have complex business rules (status transitions, payment validation, address verification), making them a natural fit for rich domain models with encapsulated behavior.

### Basket Service — Vertical Slice + Distributed Cache

```
Basket.API/
├── Basket/
│   ├── StoreBasket/
│   ├── GetBasket/
│   ├── DeleteBasket/
│   └── CheckoutBasket/
├── Models/
│   ├── ShoppingCart.cs
│   └── ShoppingCartItem.cs
└── Program.cs
```

Uses `IDistributedCache` for storage — easily swappable between MemoryCache (dev) and Redis (production).

## Cross-Cutting Concerns

### Building Blocks (Shared Library)

```
BuildingBlocks.Common/
├── CQRS/
│   ├── ICommand.cs / ICommandHandler.cs
│   └── IQuery.cs / IQueryHandler.cs
├── Behaviors/
│   ├── ValidationBehavior.cs            # Auto-validates commands via FluentValidation
│   └── LoggingBehavior.cs               # Request/response logging
└── Exceptions/
    ├── NotFoundException.cs
    ├── BadRequestException.cs
    └── InternalServerException.cs
```

### MediatR Pipeline

```
Request → LoggingBehavior → ValidationBehavior → Handler → Response
                                    ↓
                        FluentValidation rules
                        (throws ValidationException)
```

Every command passes through:
1. **Logging** — Structured logging of request type and timing
2. **Validation** — Automatic FluentValidation rule execution
3. **Handler** — Business logic execution

### Exception Handling

Global exception handler middleware maps domain exceptions to HTTP status codes:

| Exception | HTTP Status |
|-----------|-------------|
| `ValidationException` | 400 Bad Request |
| `NotFoundException` | 404 Not Found |
| `BadRequestException` | 400 Bad Request |
| `InternalServerException` | 500 Internal Server Error |

## Communication Patterns

### Synchronous (HTTP)
- Frontend → Services: Direct HTTP calls with typed clients
- Used for: Queries, commands where immediate response needed

### Asynchronous (Events)
- Service → Service: MassTransit integration events
- Used for: State propagation across bounded contexts (e.g., BasketCheckoutEvent)

### Integration Events

```csharp
// Published by Basket service after checkout
public record BasketCheckoutEvent(
    string UserName,
    Guid CustomerId,
    decimal TotalPrice,
    // ... shipping/payment details
) : IntegrationEvent;

// Consumed by Ordering service to create order
public class BasketCheckoutConsumer : IConsumer<BasketCheckoutEvent>
```

## Database Strategy

### Polyglot Persistence

Each service owns its data store, chosen for its specific access patterns:

| Service | Store | Rationale |
|---------|-------|-----------|
| Catalog | MongoDB | Document model fits product catalogs with varying attributes |
| Basket | Distributed Cache | Session-like data, high read/write, short-lived |
| Ordering | Relational (EF Core) | Transactional consistency, complex queries, relationships |

### Data Isolation

- No shared databases between services
- Each service has its own schema/collection
- Cross-service data access only via APIs or events

## API Gateway (YARP)

Routes all frontend requests through a single entry point:

```json
{
  "ReverseProxy": {
    "Routes": {
      "catalog-route": { "Match": "/api/v1/catalog/{**catch-all}" },
      "basket-route": { "Match": "/api/v1/basket/{**catch-all}" },
      "ordering-route": { "Match": "/api/v1/ordering/{**catch-all}" }
    }
  }
}
```

Benefits:
- Single origin for CORS
- Centralized rate limiting / auth (future)
- Service discovery abstraction

## Frontend Architecture (Blazor WASM)

```
EShop.Web/
├── Pages/
│   ├── Products.razor         # Product listing + category filter
│   ├── Cart.razor             # Cart management
│   ├── Checkout.razor         # Saga-orchestrated checkout
│   └── Orders.razor           # Order history
├── Services/
│   ├── CatalogService.cs     # Typed HttpClient for Catalog API
│   ├── BasketService.cs      # Typed HttpClient for Basket API
│   ├── OrderService.cs       # Typed HttpClient for Ordering API
│   └── CheckoutSagaOrchestrator.cs  # Saga coordination
├── Resilience/
│   └── PollyPolicies.cs      # Retry + Circuit Breaker configuration
├── Models/                    # Client-side DTOs
└── Program.cs                 # Service registration + Polly setup
```

### Service Registration Pattern

```csharp
// Each HTTP client gets Polly resilience policies
builder.Services.AddHttpClient<CatalogService>(client =>
    client.BaseAddress = new Uri("http://localhost:5050"))
    .AddPolicyHandler(PollyPolicies.GetRetryPolicy())
    .AddPolicyHandler(PollyPolicies.GetCircuitBreakerPolicy());
```

## Health Checks

Every service exposes `/health` endpoint:

```csharp
builder.Services.AddHealthChecks();
app.MapHealthChecks("/health");
```

Used for:
- Docker Compose readiness probes
- Load balancer health monitoring
- Service mesh integration

## Docker Compose Topology

```yaml
services:
  catalog.api:    # Port 5050
  basket.api:     # Port 6060
  ordering.api:   # Port 5070
  gateway:        # Port 5000
  web:            # Port 5200
  catalogdb:      # MongoDB :27017
  messagebroker:  # RabbitMQ :5672 / :15672
```

## Design Principles

1. **Single Responsibility** — Each service owns one business capability
2. **Autonomy** — Services can be deployed, scaled, and evolved independently
3. **Resilience** — Failures are expected and handled gracefully (see RESILIENCE-PATTERNS.md)
4. **Eventual Consistency** — Prefer availability over strict consistency across services
5. **Smart Endpoints, Dumb Pipes** — Business logic in services, not in the message bus
