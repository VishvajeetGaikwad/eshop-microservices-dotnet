# EShop Microservices

A production-ready microservices-based e-commerce application built with .NET 8, demonstrating enterprise architecture patterns, CQRS, Event-Driven Architecture, and containerized deployment.

## 🏗️ Architecture

```
┌─────────────────────────────────────────────────────────┐
│              Blazor WebAssembly (Frontend)               │
└─────────────────────┬───────────────────────────────────┘
                      │
┌─────────────────────▼───────────────────────────────────┐
│               YARP API Gateway (:5000)                  │
└──┬──────────┬──────────┬──────────┬─────────────────────┘
   │          │          │          │
   ▼          ▼          ▼          ▼
┌──────┐ ┌────────┐ ┌────────┐ ┌──────────────┐
│Catalog│ │Basket  │ │Ordering│ │   Identity   │
│ :5050 │ │ :5060  │ │ :5070  │ │    :5080     │
└──┬────┘ └──┬─────┘ └──┬─────┘ └──────┬───────┘
   │          │          │              │
   ▼          ▼          ▼              ▼
 MongoDB    Redis    SQL Server    SQL Server

         ← RabbitMQ + MassTransit (Event Bus) →
```

## 🛠️ Tech Stack

| Layer | Technology |
|-------|-----------|
| **Frontend** | Blazor WebAssembly |
| **API Gateway** | YARP Reverse Proxy |
| **Services** | .NET 8 Minimal APIs |
| **CQRS** | MediatR |
| **Validation** | FluentValidation |
| **Messaging** | RabbitMQ + MassTransit |
| **Databases** | MongoDB, Redis, SQL Server |
| **ORM** | EF Core, MongoDB Driver |
| **Containers** | Docker, Docker Compose |
| **Logging** | Serilog + Seq |
| **Health Checks** | ASP.NET Core Health Checks |
| **API Docs** | Swagger / OpenAPI |

## 📂 Project Structure

```
EShopMicroservices/
├── src/
│   ├── ApiGateways/
│   │   └── EShop.Gateway/                 # YARP API Gateway
│   ├── BuildingBlocks/
│   │   ├── EShop.BuildingBlocks.Common/   # CQRS, Behaviors, Exceptions
│   │   └── EShop.BuildingBlocks.Messaging/# MassTransit integration events
│   ├── Services/
│   │   ├── Catalog/EShop.Catalog.API/     # Product catalog (MongoDB)
│   │   ├── Basket/                        # Shopping cart (Redis)
│   │   ├── Ordering/                      # Order management (SQL Server + DDD)
│   │   ├── Identity/                      # Authentication & Authorization
│   │   ├── Payment/                       # Payment processing (Saga)
│   │   └── Notification/                  # Email/Push notifications
│   └── WebApps/                           # Blazor WASM frontend
├── tests/
├── docker-compose.yml
└── EShopMicroservices.sln
```

## 🚀 Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)

### Run with Docker Compose

```bash
# Clone the repository
git clone https://github.com/yourusername/EShopMicroservices.git
cd EShopMicroservices

# Start all services
docker-compose up -d

# Check services are running
docker-compose ps
```

### Run Locally (Development)

```bash
# Start infrastructure only
docker-compose up -d catalogdb basketdb orderdb messagebroker seq

# Run Catalog service
cd src/Services/Catalog/EShop.Catalog.API
dotnet run

# Run Gateway (separate terminal)
cd src/ApiGateways/EShop.Gateway
dotnet run
```

### Access Points

| Service | URL |
|---------|-----|
| **API Gateway** | http://localhost:5000 |
| **Catalog API** | http://localhost:5050/swagger |
| **RabbitMQ Dashboard** | http://localhost:15672 (guest/guest) |
| **Seq Log Dashboard** | http://localhost:8090 |

## 🧪 API Examples

### Create a Product
```bash
POST http://localhost:5000/api/v1/catalog/products
Content-Type: application/json

{
  "name": "Gaming Keyboard",
  "category": ["Peripherals", "Gaming"],
  "description": "Mechanical RGB keyboard",
  "imageFile": "keyboard.png",
  "price": 129.99
}
```

### Get All Products (Paginated)
```bash
GET http://localhost:5000/api/v1/catalog/products?pageIndex=0&pageSize=10
```

### Get Products by Category
```bash
GET http://localhost:5000/api/v1/catalog/products/category/Electronics
```

## 🏛️ Architecture Patterns

- **CQRS** (Command Query Responsibility Segregation) via MediatR
- **Vertical Slice Architecture** — features organized by business capability
- **Repository Pattern** with MongoDB collections
- **API Gateway Pattern** via YARP reverse proxy
- **Event-Driven Architecture** with RabbitMQ + MassTransit
- **Pipeline Behaviors** for cross-cutting concerns (validation, logging)
- **Health Check Pattern** for service monitoring
- **Polyglot Persistence** — right database for each service

## 📋 Roadmap

- [x] Phase 1: Foundation (Solution structure, Catalog, Gateway, Docker)
- [ ] Phase 2: Core Commerce (Basket, Ordering, Event Bus integration)
- [ ] Phase 3: Cross-Cutting (Identity, Payment Saga, Notifications)
- [ ] Phase 4: Frontend & Polish (Blazor WASM, Logging, Integration Tests)

## 📝 License

This project is licensed under the MIT License.
