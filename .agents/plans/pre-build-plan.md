# Pre-Build Planning & Scaffolding Guide
## Multi-Tenant Ecommerce SaaS Platform
### .NET 9 Microservices + Next.js 15 App Router

---

## Executive Architecture Summary

This platform is a multi-tenant ecommerce SaaS where merchants onboard, manage stores (products, inventory, orders, customers), and buyers browse storefronts and checkout. The architecture follows Domain-Driven Design (DDD) bounded contexts, database-per-service isolation, and async event-driven communication via MassTransit + RabbitMQ. The frontend uses a Backend-for-Frontend (BFF) pattern with Next.js 15 App Router Server Components as the default, streaming data via Suspense boundaries.

**Core Principles:**
- **No shared databases** between services.
- **Eventual consistency** for cross-service operations (sagas).
- **Stubbed auth/payments** — no real identity provider or payment gateway integration.
- **Observability-first** — OpenTelemetry, structured logging, health checks on every boundary.
- **Async/await everywhere** with `CancellationToken` propagation.
- **AsNoTracking** on all read queries.

---

## Deliverable 1: Monorepo Structure

```
/  (repo root)
├── .agents/
│   └── plans/
│       └── pre-build-plan.md        <-- THIS DOCUMENT
├── docs/
│   ├── adr/                         <-- Architecture Decision Records
│   └── runbooks/
├── docker-compose.yml               <-- Local infrastructure (existing)
├── frontend/                        <-- Next.js 15 (existing)
│   ├── src/
│   │   ├── app/                     <-- App Router
│   │   ├── components/ui/           <-- shadcn/ui
│   │   └── lib/
│   ├── next.config.ts
│   └── package.json
├── backend/
│   ├── SaaSMicroservices.sln        <-- .NET Solution (existing)
│   ├── Directory.Build.props        <-- Shared MSBuild props (existing)
│   ├── Directory.Packages.props     <-- Centralized versioning (existing)
│   ├── src/
│   │   ├── Gateway/                 <-- YARP API Gateway
│   │   │   └── Gateway.csproj
│   │   ├── Services/
│   │   │   ├── IdentityService/
│   │   │   ├── CatalogService/
│   │   │   ├── InventoryService/
│   │   │   ├── OrderService/
│   │   │   ├── CustomerService/
│   │   │   ├── PaymentService/
│   │   │   └── NotificationService/
│   │   └── Shared/SaaSCommon/     <-- Shared technical library
│   │       ├── SaaSCommon.csproj
│   │       ├── Domain/
│   │       ├── Messaging/
│   │       ├── OpenTelemetry/
│   │       └── Web/
│   ├── tests/
│   │   ├── Unit/
│   │   ├── Integration/             <-- Testcontainers per service
│   │   └── Contract/
│   └── docker/
│       ├── prometheus.yml
│       └── grafana-provisioning/
└── k8s/
    └── helm/
        └── saas-ecommerce/
            ├── Chart.yaml
            ├── values.yaml
            └── templates/
```

**Key Decisions:**
- **Single solution file** with all service `.csproj` references for IDE navigation, but each service deploys independently.
- **Shared `SaaSCommon`** is a project reference (not a NuGet package) during active development to avoid versioning friction.
- **Frontend and backend at repo root** enables shared Docker Compose and CI/CD pipelines.

---

## Deliverable 2: Service Boundary Definitions

| Service | Bounded Context | Owned Entities | Published Events | Consumed Events | Exposed APIs |
|---|---|---|---|---|---|
| **Gateway** | Routing/Ingress | None (stateless) | None | None | Proxy only — no business API |
| **IdentityService** | Identity & Access | Tenant, User, Role, TenantMembership | `TenantCreated`, `TenantUpdated`, `UserRegistered` | None | `/api/identity/tenants`, `/api/identity/users`, `/api/identity/auth/token` |
| **CatalogService** | Product Catalog | Product, ProductVariant, Category, MediaAsset | `ProductCreated`, `ProductUpdated`, `ProductDeleted`, `CategoryCreated` | `TenantCreated` | `/api/catalog/products`, `/api/catalog/categories` |
| **InventoryService** | Stock Management | InventoryItem, Reservation, StockMovement | `InventoryReserved`, `InventoryReservationFailed`, `StockReleased` | `OrderPlaced`, `OrderCancelled`, `TenantCreated` | `/api/inventory/items`, `/api/inventory/reserve` |
| **OrderService** | Order Lifecycle | Order, OrderItem, OrderStatusHistory | `OrderPlaced`, `OrderPaid`, `OrderCancelled`, `OrderShipped` | `InventoryReserved`, `PaymentProcessed`, `PaymentFailed`, `InventoryReservationFailed` | `/api/orders`, `/api/orders/{id}/cancel` |
| **CustomerService** | Customer Profiles | Customer, Address, OrderSummary (denormalized) | `CustomerCreated`, `CustomerUpdated` | `OrderPlaced`, `OrderPaid`, `TenantCreated` | `/api/customers`, `/api/customers/{id}/addresses` |
| **PaymentService** | Payment Simulation | PaymentIntent, PaymentTransaction | `PaymentProcessed`, `PaymentFailed` | `OrderPlaced`, `OrderCancelled` | `/api/payments/intents`, `/api/payments/webhook` |
| **NotificationService** | Notifications | NotificationLog, Template | `NotificationSent` | `OrderPlaced`, `OrderPaid`, `OrderCancelled`, `CustomerCreated` | `/api/notifications`, `/api/notifications/templates` |

**Important:** No synchronous service-to-service HTTP calls for long-running operations. The OrderService saga orchestrates via events.

---

## Deliverable 3: Integration Events Catalog

### IdentityService Publishes:
- `TenantCreated { Guid TenantId, string Name, string Slug, DateTimeOffset CreatedAt }`
- `TenantUpdated { Guid TenantId, string Name, string Slug }`
- `UserRegistered { Guid UserId, Guid TenantId, string Email, DateTimeOffset RegisteredAt }`

### CatalogService Publishes:
- `ProductCreated { Guid ProductId, Guid TenantId, string Name, string Sku, decimal Price, DateTimeOffset CreatedAt }`
- `ProductUpdated { Guid ProductId, Guid TenantId, string Name, string Sku, decimal Price }`
- `ProductDeleted { Guid ProductId, Guid TenantId }`
- `CategoryCreated { Guid CategoryId, Guid TenantId, string Name, Guid? ParentCategoryId }`

### InventoryService Publishes:
- `InventoryReserved { Guid OrderId, Guid TenantId, Dictionary<Guid, int> ReservedQuantities }`
- `InventoryReservationFailed { Guid OrderId, Guid TenantId, string Reason }`
- `StockReleased { Guid OrderId, Guid TenantId, Dictionary<Guid, int> ReleasedQuantities }`

### OrderService Publishes:
- `OrderPlaced { Guid OrderId, Guid TenantId, Guid CustomerId, decimal TotalAmount, IReadOnlyList<OrderItemDto> Items }`
- `OrderPaid { Guid OrderId, Guid TenantId, Guid PaymentIntentId }`
- `OrderCancelled { Guid OrderId, Guid TenantId, string Reason }`
- `OrderShipped { Guid OrderId, Guid TenantId, string TrackingNumber }`

### CustomerService Publishes:
- `CustomerCreated { Guid CustomerId, Guid TenantId, string Email, string FullName }`
- `CustomerUpdated { Guid CustomerId, Guid TenantId, string Email, string FullName }`

### PaymentService Publishes:
- `PaymentProcessed { Guid PaymentIntentId, Guid OrderId, Guid TenantId, decimal Amount, DateTimeOffset ProcessedAt }`
- `PaymentFailed { Guid PaymentIntentId, Guid OrderId, Guid TenantId, string FailureReason }`

### NotificationService Publishes:
- `NotificationSent { Guid NotificationId, Guid TenantId, string Recipient, string Channel, string TemplateKey }`

---

## Deliverable 4: API Gateway Routing Table (YARP)

```json
{
  "ReverseProxy": {
    "Routes": {
      "identity": { "ClusterId": "identity", "Match": { "Path": "/api/identity/{**catch-all}" }, "Transforms": [ { "PathPattern": "/{**catch-all}" } ] },
      "catalog": { "ClusterId": "catalog", "Match": { "Path": "/api/catalog/{**catch-all}" }, "Transforms": [ { "PathPattern": "/{**catch-all}" } ] },
      "inventory": { "ClusterId": "inventory", "Match": { "Path": "/api/inventory/{**catch-all}" }, "Transforms": [ { "PathPattern": "/{**catch-all}" } ] },
      "orders": { "ClusterId": "orders", "Match": { "Path": "/api/orders/{**catch-all}" }, "Transforms": [ { "PathPattern": "/{**catch-all}" } ] },
      "customers": { "ClusterId": "customers", "Match": { "Path": "/api/customers/{**catch-all}" }, "Transforms": [ { "PathPattern": "/{**catch-all}" } ] },
      "payments": { "ClusterId": "payments", "Match": { "Path": "/api/payments/{**catch-all}" }, "Transforms": [ { "PathPattern": "/{**catch-all}" } ] },
      "notifications": { "ClusterId": "notifications", "Match": { "Path": "/api/notifications/{**catch-all}" }, "Transforms": [ { "PathPattern": "/{**catch-all}" } ] }
    },
    "Clusters": {
      "identity": { "Destinations": { "identity/destination1": { "Address": "http://identityservice:8080" } } },
      "catalog": { "Destinations": { "catalog/destination1": { "Address": "http://catalogservice:8080" } } },
      "inventory": { "Destinations": { "inventory/destination1": { "Address": "http://inventoryservice:8080" } } },
      "orders": { "Destinations": { "orders/destination1": { "Address": "http://orderservice:8080" } } },
      "customers": { "Destinations": { "customers/destination1": { "Address": "http://customerservice:8080" } } },
      "payments": { "Destinations": { "payments/destination1": { "Address": "http://paymentservice:8080" } } },
      "notifications": { "Destinations": { "notifications/destination1": { "Address": "http://notificationservice:8080" } } }
    }
  }
}
```

**Auth & Rate Limits:**
- JWT validation applied at Gateway level (global middleware).
- Rate limit policies:
  - `standard`: 100 requests/min per IP.
  - `authenticated`: 1000 requests/min per `sub` claim.
  - `webhook`: 50 requests/min per source IP (for `/api/payments/webhook`).
- `X-Tenant-ID` header injected from JWT `tenant_id` claim on every downstream request.

---

## Deliverable 5: Database Schema Per Service

**Tenant Isolation Strategy:** Row-level `TenantId` column on every entity. Single PostgreSQL instance with database-per-service. No schema-per-tenant (operational overhead too high for SaaS with thousands of tenants).

### IdentityService — `identity_db`
- `Tenants` — `Id (PK, Guid)`, `Name`, `Slug (UQ)`, `CreatedAt`, `UpdatedAt`
- `Users` — `Id (PK, Guid)`, `TenantId (FK, IX)`, `Email (UQ per tenant)`, `PasswordHash`, `Role`, `CreatedAt`
- `TenantMemberships` — composite PK (`UserId`, `TenantId`)

### CatalogService — `catalog_db`
- `Products` — `Id (PK)`, `TenantId (IX)`, `Name`, `Description`, `Sku (UQ per tenant)`, `BasePrice`, `CategoryId (FK)`, `CreatedAt`, `UpdatedAt`
- `ProductVariants` — `Id (PK)`, `ProductId (FK, IX)`, `Sku`, `PriceAdjustment`, `StockTrackingEnabled`
- `Categories` — `Id (PK)`, `TenantId (IX)`, `Name`, `ParentCategoryId (self-FK)`
- `MediaAssets` — `Id (PK)`, `TenantId (IX)`, `ProductId (FK)`, `Url`, `Type`, `SortOrder`

### InventoryService — `inventory_db`
- `InventoryItems` — `Id (PK)`, `TenantId (IX)`, `ProductId`, `Sku`, `QuantityOnHand`, `QuantityReserved`, `ReorderLevel`, `LastUpdated`
- `Reservations` — `Id (PK)`, `TenantId (IX)`, `OrderId (IX)`, `ProductId`, `Quantity`, `Status (enum)`, `CreatedAt`, `ExpiresAt`
- `StockMovements` — `Id (PK)`, `TenantId (IX)`, `InventoryItemId (FK)`, `Type`, `Quantity`, `Reference`, `CreatedAt`

### OrderService — `order_db`
- `Orders` — `Id (PK)`, `TenantId (IX)`, `CustomerId`, `Status (enum)`, `TotalAmount`, `Currency`, `ShippingAddressJson`, `CreatedAt`, `UpdatedAt`
- `OrderItems` — `Id (PK)`, `OrderId (FK, IX)`, `ProductId`, `ProductName (denormalized)`, `Sku`, `UnitPrice`, `Quantity`, `Discount`
- `OrderStatusHistories` — `Id (PK)`, `OrderId (FK, IX)`, `Status`, `ChangedAt`, `Reason`

### CustomerService — `customer_db`
- `Customers` — `Id (PK)`, `TenantId (IX)`, `Email (UQ per tenant)`, `FullName`, `Phone`, `CreatedAt`, `UpdatedAt`
- `Addresses` — `Id (PK)`, `CustomerId (FK, IX)`, `TenantId (IX)`, `Type (Billing/Shipping)`, `Line1`, `Line2`, `City`, `State`, `PostalCode`, `Country`, `IsDefault`
- `OrderSummaries` — `Id (PK)`, `TenantId (IX)`, `OrderId (UQ)`, `CustomerId (FK, IX)`, `TotalAmount`, `Status`, `CreatedAt`

### PaymentService — `payment_db`
- `PaymentIntents` — `Id (PK)`, `TenantId (IX)`, `OrderId (IX)`, `Amount`, `Currency`, `Status (enum)`, `Method`, `IdempotencyKey (UQ)`, `CreatedAt`, `ProcessedAt`
- `PaymentTransactions` — `Id (PK)`, `PaymentIntentId (FK, IX)`, `Type`, `Amount`, `Status`, `GatewayResponse`, `CreatedAt`

### NotificationService — `notification_db`
- `NotificationLogs` — `Id (PK)`, `TenantId (IX)`, `Recipient`, `Channel (Email/SMS)`, `TemplateKey`, `PayloadJson`, `Status`, `SentAt`, `Error`
- `Templates` — `Id (PK)`, `TenantId (IX)`, `Key`, `Subject`, `BodyHtml`, `BodyText`, `Channel`

**EF Core Migration Strategy:** Each service owns its `Migrations/` folder. Use `dotnet ef migrations add --project {Service}.csproj`.

---

## Deliverable 6: Saga & Workflow Designs

### (a) Order Placement Saga (MassTransit State Machine)

```csharp
public class OrderPlacementState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = null!;
    public Guid OrderId { get; set; }
    public Guid TenantId { get; set; }
    public decimal TotalAmount { get; set; }
}

public class OrderPlacementStateMachine : MassTransitStateMachine<OrderPlacementState>
{
    public State InventoryPending { get; private set; } = null!;
    public State PaymentPending { get; private set; } = null!;
    public State Completed { get; private set; } = null!;
    public State Failed { get; private set; } = null!;

    public Event<OrderPlaced> OrderPlaced { get; private set; } = null!;
    public Event<InventoryReserved> InventoryReserved { get; private set; } = null!;
    public Event<InventoryReservationFailed> InventoryReservationFailed { get; private set; } = null!;
    public Event<PaymentProcessed> PaymentProcessed { get; private set; } = null!;
    public Event<PaymentFailed> PaymentFailed { get; private set; } = null!;

    public OrderPlacementStateMachine()
    {
        InstanceState(x => x.CurrentState);

        Initially(
            When(OrderPlaced)
                .Then(ctx => ctx.Instance.OrderId = ctx.Data.OrderId)
                .Then(ctx => ctx.Instance.TenantId = ctx.Data.TenantId)
                .Then(ctx => ctx.Instance.TotalAmount = ctx.Data.TotalAmount)
                .TransitionTo(InventoryPending)
                .Publish(ctx => new ReserveInventory
                {
                    OrderId = ctx.Data.OrderId,
                    TenantId = ctx.Data.TenantId,
                    Items = ctx.Data.Items.ToDictionary(i => i.ProductId, i => i.Quantity)
                }));

        During(InventoryPending,
            When(InventoryReserved)
                .TransitionTo(PaymentPending)
                .Publish(ctx => new InitiatePayment
                {
                    OrderId = ctx.Instance.OrderId,
                    TenantId = ctx.Instance.TenantId,
                    Amount = ctx.Instance.TotalAmount
                }),
            When(InventoryReservationFailed)
                .TransitionTo(Failed)
                .Publish(ctx => new CancelOrder
                {
                    OrderId = ctx.Instance.OrderId,
                    TenantId = ctx.Instance.TenantId,
                    Reason = "Inventory unavailable"
                }));

        During(PaymentPending,
            When(PaymentProcessed)
                .TransitionTo(Completed)
                .Publish(ctx => new ConfirmOrder
                {
                    OrderId = ctx.Instance.OrderId,
                    TenantId = ctx.Instance.TenantId
                }),
            When(PaymentFailed)
                .TransitionTo(Failed)
                .Publish(ctx => new ReleaseStock
                {
                    OrderId = ctx.Instance.OrderId,
                    TenantId = ctx.Instance.TenantId
                })
                .Publish(ctx => new CancelOrder
                {
                    OrderId = ctx.Instance.OrderId,
                    TenantId = ctx.Instance.TenantId,
                    Reason = "Payment failed"
                }));
    }
}
```

### (b) Order Cancellation Saga

```csharp
public class OrderCancellationState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = null!;
    public Guid OrderId { get; set; }
    public Guid TenantId { get; set; }
}

public class OrderCancellationStateMachine : MassTransitStateMachine<OrderCancellationState>
{
    public State RefundPending { get; private set; } = null!;
    public State Completed { get; private set; } = null!;

    public Event<OrderCancelled> OrderCancelled { get; private set; } = null!;
    public Event<PaymentRefunded> PaymentRefunded { get; private set; } = null!;

    public OrderCancellationStateMachine()
    {
        InstanceState(x => x.CurrentState);

        Initially(
            When(OrderCancelled)
                .Then(ctx => {
                    ctx.Instance.OrderId = ctx.Data.OrderId;
                    ctx.Instance.TenantId = ctx.Data.TenantId;
                })
                .IfElse(
                    ctx => ctx.Data.RequiresRefund,
                    then => then
                        .TransitionTo(RefundPending)
                        .Publish(ctx => new RefundPayment
                        {
                            OrderId = ctx.Data.OrderId,
                            TenantId = ctx.Data.TenantId
                        }),
                    else => else
                        .Publish(ctx => new ReleaseStock
                        {
                            OrderId = ctx.Data.OrderId,
                            TenantId = ctx.Data.TenantId
                        })
                        .TransitionTo(Completed)));

        During(RefundPending,
            When(PaymentRefunded)
                .Publish(ctx => new ReleaseStock
                {
                    OrderId = ctx.Instance.OrderId,
                    TenantId = ctx.Instance.TenantId
                })
                .TransitionTo(Completed));
    }
}
```

---

## Deliverable 7: CQRS Command/Query Split

### IdentityService
- `RegisterUserCommand` → `RegisterUserResponse`
- `CreateTenantCommand` → `TenantDto`
- `GetTenantByIdQuery` → `TenantDto`
- `GetUserByEmailQuery` → `UserDto`

### CatalogService
- `CreateProductCommand` → `ProductDto`
- `UpdateProductCommand` → `ProductDto`
- `DeleteProductCommand` → `Unit`
- `GetProductByIdQuery` → `ProductDto`
- `ListProductsQuery` → `PagedResult<ProductSummaryDto>`

### InventoryService
- `ReserveInventoryCommand` → `ReservationResult`
- `ReleaseStockCommand` → `Unit`
- `AdjustStockCommand` → `StockAdjustmentDto`
- `GetInventoryItemBySkuQuery` → `InventoryItemDto`
- `ListLowStockQuery` → `IReadOnlyList<InventoryItemDto>`

### OrderService
- `PlaceOrderCommand` → `OrderDto`
- `CancelOrderCommand` → `OrderDto`
- `GetOrderByIdQuery` → `OrderDto`
- `ListOrdersByCustomerQuery` → `PagedResult<OrderSummaryDto>`

### CustomerService
- `CreateCustomerCommand` → `CustomerDto`
- `UpdateCustomerCommand` → `CustomerDto`
- `AddAddressCommand` → `AddressDto`
- `GetCustomerByIdQuery` → `CustomerDto`
- `GetCustomerOrderHistoryQuery` → `PagedResult<OrderSummaryDto>`

### PaymentService
- `CreatePaymentIntentCommand` → `PaymentIntentDto`
- `ProcessPaymentCommand` → `PaymentResult`
- `RefundPaymentCommand` → `RefundDto`
- `GetPaymentIntentByIdQuery` → `PaymentIntentDto`

### NotificationService
- `SendNotificationCommand` → `NotificationLogDto`
- `GetNotificationLogQuery` → `PagedResult<NotificationLogDto>`

---

## Deliverable 8: Next.js Architecture Plan

### App Router Folder Structure
```
frontend/src/app/
├── (storefront)/              <-- Buyer-facing routes
│   ├── [tenantSlug]/
│   │   ├── page.tsx           <-- Tenant storefront home
│   │   ├── products/
│   │   │   └── [id]/page.tsx
│   │   └── cart/
│   │       └── page.tsx
│   └── layout.tsx
├── (dashboard)/               <-- Merchant dashboard
│   ├── dashboard/
│   │   ├── products/
│   │   ├── orders/
│   │   └── customers/
│   └── layout.tsx
├── api/
│   ├── health/route.ts
│   ├── catalog/
│   │   └── products/route.ts  <-- BFF proxy
│   ├── orders/
│   │   └── route.ts
│   └── auth/
│       └── session/route.ts
├── layout.tsx                 <-- Root layout (Server Component)
└── globals.css
```

### Server vs Client Component Matrix

| Scenario | Component Type | Reason |
|---|---|---|
| Product listing page | Server | Data fetched at request time, SEO-critical |
| Cart UI interactions | Client | React state, localStorage, optimistic updates |
| Checkout form | Client | Browser APIs, Stripe.js (stubbed) |
| Dashboard analytics | Server | Sensitive data, auth gate, no SEO need |
| Modal dialogs | Client | `useState`, `useEffect`, portals |

### Middleware (`middleware.ts`)
- Tenant detection from subdomain or path (`[tenantSlug]`).
- JWT cookie validation (stubbed — checks presence, not signature in dev).
- Redirect unauthenticated dashboard requests to `/login`.
- Inject `x-tenant-id` header on BFF API calls.

### Data Fetching Patterns
- **Parallel fetching:** Use `Promise.all` in Server Components for independent queries (e.g., product + reviews).
- **Streaming:** Wrap slow queries in `<Suspense>` with fallback skeletons.
- **Caching:** TanStack Query for client state; `fetch` with `revalidate` for ISR on catalog pages.

---

## Deliverable 9: Shared BuildingBlocks Library (`SaaSCommon`)

```csharp
// SaaSCommon/Domain/Entity.cs
public abstract class Entity
{
    public Guid Id { get; protected set; }
    public Guid TenantId { get; protected set; }
}

// SaaSCommon/Domain/IntegrationEvent.cs
public abstract record IntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredOn { get; init; } = DateTimeOffset.UtcNow;
    public Guid TenantId { get; init; }
}

// SaaSCommon/Messaging/IOutboxStore.cs
public interface IOutboxStore
{
    Task AddAsync(OutboxMessage message, CancellationToken ct = default);
    Task<IReadOnlyList<OutboxMessage>> GetPendingAsync(int batchSize, CancellationToken ct = default);
    Task MarkProcessedAsync(Guid messageId, CancellationToken ct = default);
}

// SaaSCommon/Web/Pagination.cs
public record PaginationRequest(int Page = 1, int PageSize = 20);
public record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize);

// SaaSCommon/Domain/Result.cs
public readonly record struct Result<T>
{
    public T? Value { get; }
    public string? Error { get; }
    public bool IsSuccess => Error is null;
    public static Result<T> Success(T value) => new(value, null);
    public static Result<T> Failure(string error) => new(default, error);
}

// SaaSCommon/Web/CorrelationIdMiddleware.cs
public class CorrelationIdMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault() ?? Guid.NewGuid().ToString();
        context.Response.Headers["X-Correlation-ID"] = correlationId;
        // Propagate via Activity or Serilog context...
        await next(context);
    }
}
```

---

## Deliverable 10: Observability Plan

### OpenTelemetry Instrumentation Strategy
- **Traces:** ASP.NET Core + HttpClient + EF Core + MassTransit instrumented.
- **Propagation:** `traceparent` header via HTTP; message envelope headers via RabbitMQ.
- **Correlation:** `X-Correlation-ID` → `Activity.Current?.SetBaggageItem("correlation.id", id)`.
- **Metrics:** `http.server.request.duration`, `masstransit.consumer.duration`, `efcore.query.duration`.

### Health Check Contracts
Every service exposes:
- `GET /health/live` — Liveness (always returns 200 if process is up).
- `GET /health/ready` — Readiness (checks DB, RabbitMQ, Redis connectivity).

### Grafana Dashboard Suggestions
1. **Service Overview:** Request rate, error rate, latency p95/p99 per service.
2. **Saga Health:** Active sagas, failed saga transitions, average completion time.
3. **Tenant Activity:** Requests per tenant, event throughput per tenant.

---

## Deliverable 11: Testing Strategy

### Test Pyramid per Service
1. **Unit Tests (60%):** Domain logic, validators, policy classes. Run in-memory, no I/O.
2. **Integration Tests (30%):** Spin up real PostgreSQL + RabbitMQ via Testcontainers per test class. Test repository + MediatR handler interactions.
3. **Contract Tests (5%):** Verify API Gateway YARP routes resolve correctly; Pact-style consumer/provider for event schemas.
4. **E2E Tests (5%):** Critical checkout flow using Playwright against the full Docker Compose stack.

### Key Testing Libraries
- xUnit + FluentAssertions + NSubstitute
- Testcontainers.PostgreSQL + Testcontainers.RabbitMQ
- Microsoft.AspNetCore.TestHost for in-memory API testing

---

## Deliverable 12: CI/CD Pipeline Plan

### GitHub Actions Workflow per Service
```yaml
name: Service CI/CD
on:
  push:
    paths: ['backend/src/Services/{ServiceName}/**']
jobs:
  lint:
    steps:
      - dotnet format --verify-no-changes
  build:
    needs: lint
    steps:
      - dotnet restore
      - dotnet build --no-restore
  unit-test:
    needs: build
    steps:
      - dotnet test --filter Category=Unit --no-build
  integration-test:
    needs: build
    steps:
      - dotnet test --filter Category=Integration --no-build
  docker-build:
    needs: [unit-test, integration-test]
    steps:
      - docker build -t saas/{service}:sha .
      - docker push ghcr.io/saas/{service}:sha
  deploy-staging:
    needs: docker-build
    steps:
      - helm upgrade --install {service} ./k8s/helm/saas-ecommerce
  smoke-test:
    needs: deploy-staging
    steps:
      - curl -f http://staging.saas.local/health/ready
  promote-prod:
    needs: smoke-test
    steps:
      - helm upgrade --install {service} ./k8s/helm/saas-ecommerce --values values.prod.yaml
```

---

## Deliverable 13: Security Checklist

- [ ] JWT RS256 validation at Gateway (stubbed key for dev).
- [ ] `tenant_id` claim enforced in every service handler; reject if `TenantId` from route/body mismatches claim.
- [ ] EF Core only — no raw SQL. All queries parameterized.
- [ ] Secrets in `appsettings.Production.json` reference Kubernetes Secrets or Vault; never committed.
- [ ] CORS: `Access-Control-Allow-Origin` restricted to known storefront domains per tenant.
- [ ] Rate limiting per tenant (`X-Tenant-ID` bucket) to prevent noisy neighbor issues.
- [ ] OWASP mitigations: Input validation (FluentValidation), output encoding, CSRF tokens on mutations, security headers (HSTS, CSP).

---

## Deliverable 14: Decision Log (ADRs)

### ADR-001: Microservices over Modular Monolith
**Context:** Startup velocity vs. long-term scale. Multi-tenant SaaS demands independent deployability per bounded context.
**Decision:** Adopt microservices from day one with database-per-service.
**Consequences:** Higher operational overhead (8 DBs, messaging), but avoids future monolith decomposition pain.

### ADR-002: MassTransit over Raw RabbitMQ
**Context:** Need saga orchestration, outbox, consumer discovery, retry policies.
**Decision:** Use MassTransit 8 with RabbitMQ transport.
**Consequences:** Abstraction over AMQP; easier testing (in-memory bus), but adds dependency on MassTransit-specific patterns.

### ADR-003: YARP over Ocelot
**Context:** .NET 9 built-in reverse proxy vs. older Ocelot library.
**Decision:** YARP — actively maintained by Microsoft, better performance, aligns with .NET 9 minimal API philosophy.
**Consequences:** Less community plugin ecosystem than Ocelot, but sufficient for routing + transforms.

### ADR-004: Database-per-Service
**Context:** Tenant isolation and service autonomy requirements.
**Decision:** Each service owns a dedicated PostgreSQL database. Row-level `TenantId` for isolation within each DB.
**Consequences:** No cross-service joins; data sync via events. Operational complexity of managing 8+ databases.

### ADR-005: Next.js App Router BFF Pattern
**Context:** Need SEO, performance, and unified data fetching for storefront + dashboard.
**Decision:** Next.js 15 App Router with Server Components as default; BFF API routes proxy to backend microservices.
**Consequences:** Reduced client-side JS, better caching, but requires careful Server/Client boundary management.

---

## Deliverable 15: Phased Build Roadmap

| Phase | Deliverable | Acceptance Criteria | Complexity | Depends On |
|---|---|---|---|---|
| 1 | **Shared SaaSCommon + Infrastructure** | `SaaSCommon` builds, Docker Compose starts all infra, health checks pass. | M | — |
| 2 | **Identity Service Stub** | JWT token endpoint returns stubbed token with `tenant_id` claim. Tenant CRUD works. | M | Phase 1 |
| 3 | **Catalog Service** | Products, variants, categories CRUD. `ProductCreated` event published. | L | Phase 2 |
| 4 | **Inventory Service** | Stock tracking, reservation endpoint. Consumes `OrderPlaced`. | L | Phase 3 |
| 5 | **Order Service + Placement Saga** | `PlaceOrder` command initiates saga. Inventory reserved → payment initiated. | XL | Phase 4 |
| 6 | **Payment Service Stub** | Payment intent simulation. `PaymentProcessed` / `PaymentFailed` events. | M | Phase 5 |
| 7 | **Order Cancellation Saga** | Cancel command triggers refund + stock release flow. | L | Phase 6 |
| 8 | **Customer Service** | Profiles, addresses. Denormalized order summaries from events. | M | Phase 5 |
| 9 | **Notification Service** | Email/SMS log consumers. Templates CRUD. | S | Phase 5, 7 |
| 10 | **API Gateway + Routing** | YARP routes all `/api/*` paths. JWT validation, tenant header injection. | M | Phase 2–9 |
| 11 | **Frontend Storefront** | Next.js tenant storefront. Product listing, cart, checkout UI. | XL | Phase 10 |
| 12 | **Observability + Tests** | Jaeger traces full checkout flow. Grafana dashboards. E2E passes. | L | Phase 11 |

---

## Pre-Implementation Team Decisions Required

1. **Tenant Routing:** Subdomain (`tenant.saas.com`) vs path (`saas.com/t/tenant`)?
2. **Real Auth Provider:** Stick with stub or integrate Keycloak/Auth0 before production?
3. **Payment Stub Heuristics:** Which card numbers trigger success vs failure?
4. **Notification Channels:** SendGrid/Mailgun integration now, or log-only?
5. **Frontend State:** Zustand for cart + UI only, or also for cached server data?
