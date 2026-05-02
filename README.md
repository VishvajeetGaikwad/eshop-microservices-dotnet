# EShop Microservices

A production-grade **Microservices E-Commerce System** built with .NET 8, demonstrating real-world distributed systems patterns including Saga Orchestration, Idempotency, Circuit Breaker, Domain-Driven Design, and CQRS.

## Problem Statement

> **"How do you handle distributed transactions, data consistency, and cascading failures when a monolithic e-commerce system is decomposed into microservices?"**

In a monolith, placing an order is a single database transaction — create order, deduct stock, clear cart, done. But in microservices, each service owns its own database. There's **no distributed transaction** that spans MongoDB, a cache, and a SQL database atomically.

This project solves three real problems that every microservices system faces:

| Problem | What Goes Wrong | Solution Implemented |
|---------|----------------|---------------------|
| **No distributed transactions** | Order is created but basket isn't cleared (or vice versa) — user sees inconsistent state | **Saga Orchestration** with compensating transactions (automatic rollback) |
| **Network retries cause duplicates** | Timeout on order creation → client retries → two identical orders charged to customer | **Idempotency Keys** — server deduplicates requests using a unique key per operation |
| **Cascading failures** | Catalog service goes down → every page hangs for 30s → entire app feels broken | **Circuit Breaker** — fail fast after 5 errors, stop hammering the dead service, recover gracefully |

These aren't theoretical patterns — they're the exact problems you hit the moment you move from `localhost` monolith to independently deployed services communicating over unreliable networks.

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                    Blazor WebAssembly (SPA)                      │
│              Products │ Cart │ Checkout │ Orders                 │
│         ┌──────────────────────────────────────────┐            │
│         │  Polly: Retry + Circuit Breaker + Jitter │            │
│         │  Saga Orchestrator (Checkout Flow)        │            │
│         │  Idempotency Keys (Duplicate Prevention)  │            │
│         └──────────┬───────────┬───────────┬───────┘            │
└────────────────────┼───────────┼───────────┼────────────────────┘
                     │           │           │
            ┌────────▼──┐  ┌────▼────┐  ┌───▼────────┐
            │  Catalog   │  │ Basket  │  │  Ordering  │
            │  Service   │  │ Service │  │  Service   │
            │  :5050     │  │ :6060   │  │  :5070     │
            ├────────────┤  ├─────────┤  ├────────────┤
            │ Vertical   │  │Vertical │  │   Clean    │
            │ Slice +    │  │ Slice + │  │Architecture│
            │ CQRS       │  │ CQRS    │  │  + DDD     │
            ├────────────┤  ├─────────┤  ├────────────┤
            │ MongoDB    │  │Distrib. │  │ EF Core    │
            │ (Mongo2Go) │  │ Cache   │  │ InMemory   │
            └────────────┘  └─────────┘  └────────────┘
                     │           │           │
            ┌────────▼───────────▼───────────▼────────┐
            │          MassTransit Event Bus           │
            │        (InMemory / RabbitMQ ready)       │
            └─────────────────────────────────────────┘
```

## Tech Stack

| Layer | Technology |
|-------|-----------|
| **Frontend** | Blazor WebAssembly, Bootstrap 5.3, Bootstrap Icons |
| **API Services** | .NET 8 Minimal APIs, Carter, MediatR (CQRS) |
| **Validation** | FluentValidation with MediatR Pipeline Behaviors |
| **Messaging** | MassTransit (InMemory / RabbitMQ) |
| **Databases** | MongoDB (Mongo2Go), EF Core InMemory, Distributed Memory Cache |
| **API Gateway** | YARP (Yet Another Reverse Proxy) |
| **Resilience** | Polly v8 (Retry, Circuit Breaker, Exponential Backoff + Jitter) |
| **Patterns** | Saga Orchestration, Idempotency Keys, CQRS, DDD, Vertical Slice, Clean Architecture |
| **DevOps** | Docker Compose, Health Checks, Swagger/OpenAPI |

## Project Structure

```
EShopMicroservices/
├── src/
│   ├── ApiGateways/
│   │   └── EShop.Gateway/                    # YARP API Gateway
│   ├── BuildingBlocks/
│   │   ├── EShop.BuildingBlocks.Common/       # CQRS abstractions, Behaviors, Exception Handling
│   │   └── EShop.BuildingBlocks.Messaging/    # MassTransit integration events
│   ├── Services/
│   │   ├── Catalog/
│   │   │   └── EShop.Catalog.API/             # Vertical Slice Architecture + MongoDB
│   │   ├── Basket/
│   │   │   └── EShop.Basket.API/              # Vertical Slice + Distributed Cache
│   │   └── Ordering/
│   │       ├── EShop.Ordering.API/            # Endpoints, Idempotency Filter
│   │       ├── EShop.Ordering.Application/    # CQRS Handlers, DTOs, Validators
│   │       ├── EShop.Ordering.Domain/         # DDD: Aggregates, Value Objects, Domain Events
│   │       └── EShop.Ordering.Infrastructure/ # EF Core DbContext, Repositories
│   └── WebApps/
│       └── EShop.Web/                         # Blazor WASM Frontend
│           ├── Pages/                         # Products, Cart, Checkout, Orders
│           ├── Services/                      # Typed HttpClients, Saga Orchestrator
│           └── Resilience/                    # Polly policies configuration
├── docs/
│   ├── ARCHITECTURE.md                        # Detailed architecture decisions
│   └── RESILIENCE-PATTERNS.md                 # Saga, Idempotency, Circuit Breaker
├── docker-compose.yml
└── EShopMicroservices.sln
```

## Quick Start

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- (Optional) [Docker Desktop](https://www.docker.com/products/docker-desktop) for containerized deployment

### Run Locally

```bash
# 1. Build the solution
dotnet build EShopMicroservices.sln

# 2. Start each service in a separate terminal
dotnet run --project src/Services/Catalog/EShop.Catalog.API --urls http://localhost:5050
dotnet run --project src/Services/Basket/EShop.Basket.API --urls http://localhost:6060
dotnet run --project src/Services/Ordering/EShop.Ordering.API --urls http://localhost:5070

# 3. Start the Blazor frontend
dotnet run --project src/WebApps/EShop.Web --urls http://localhost:5200
```

Open http://localhost:5200 to use the application.

### Service URLs

| Service | URL | Swagger |
|---------|-----|---------|
| Catalog API | http://localhost:5050 | http://localhost:5050/swagger |
| Basket API | http://localhost:6060 | http://localhost:6060/swagger |
| Ordering API | http://localhost:5070 | http://localhost:5070/swagger |
| Blazor Frontend | http://localhost:5200 | — |

### Run with Docker Compose

```bash
docker-compose up -d
```

## Key Features

### Microservice Architecture Patterns
- **CQRS** — Command/Query separation with MediatR pipeline
- **Vertical Slice Architecture** — Feature folders in Catalog & Basket services
- **Clean Architecture + DDD** — Layered with Dependency Inversion in Ordering service
- **Event-Driven Communication** — MassTransit integration events between services
- **API Gateway** — YARP reverse proxy for unified frontend access
- **Polyglot Persistence** — Right database per service bounded context

### Production Resilience Patterns
- **Saga Orchestration** — Multi-step checkout with compensation (rollback) on failure
- **Idempotency Keys** — Prevent duplicate orders from network retries
- **Circuit Breaker** — Polly-based fault tolerance with exponential backoff + jitter

> See [docs/RESILIENCE-PATTERNS.md](docs/RESILIENCE-PATTERNS.md) for implementation details and interview talking points.

### Full E-Commerce Flow
1. Browse products with category filtering
2. Add items to shopping cart
3. Checkout via Saga orchestration (CreateOrder → ClearBasket → Confirm)
4. View order history with status tracking

## API Endpoints

### Catalog Service (`/api/v1/catalog`)
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/products` | Get paginated products |
| GET | `/products/{id}` | Get product by ID |
| GET | `/products/category/{category}` | Filter by category |
| POST | `/products` | Create product |
| PUT | `/products` | Update product |
| DELETE | `/products/{id}` | Delete product |

### Basket Service (`/api/v1/basket`)
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/{userName}` | Get user's basket |
| POST | `/` | Store/update basket |
| DELETE | `/{userName}` | Delete basket |
| POST | `/checkout` | Checkout basket |

### Ordering Service (`/api/v1/ordering`)
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/orders` | Get all orders |
| GET | `/orders/customer/{customerId}` | Get orders by customer |
| POST | `/orders` | Create order (idempotent) |
| PUT | `/orders` | Update order |
| DELETE | `/orders/{id}` | Delete order |

## Architecture Decisions

| Decision | Choice | Rationale |
|----------|--------|-----------|
| Service communication | Direct HTTP + Event Bus | Sync for queries, async for state changes |
| Ordering architecture | Clean Architecture + DDD | Complex domain logic warrants full DDD |
| Catalog/Basket architecture | Vertical Slice | Simple CRUD, feature cohesion over layer separation |
| Database per service | MongoDB / Cache / EF Core | Polyglot persistence — right tool per use case |
| Idempotency | Endpoint Filter + ConcurrentDictionary | Prevents duplicate creates from network retries |
| Saga | Orchestration (not Choreography) | Explicit control flow, easier to reason about failures |
| Resilience | Polly v8 Pipelines | Industry standard, composable policies |

## Completed Phases

- [x] **Phase 1** — Foundation: Solution structure, Catalog service, API Gateway, Docker Compose
- [x] **Phase 2** — Core Commerce: Basket service, Ordering service (DDD/Clean Arch), Event Bus
- [x] **Phase 3** — Frontend: Blazor WebAssembly SPA with typed HTTP clients
- [x] **Phase 4** — Production Resilience: Saga pattern, Idempotency keys, Circuit Breaker

## Documentation

- [Architecture Deep Dive](docs/ARCHITECTURE.md)
- [Resilience Patterns](docs/RESILIENCE-PATTERNS.md)

## License

This project is for educational and portfolio purposes.
