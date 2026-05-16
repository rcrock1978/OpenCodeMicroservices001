# SaaS Ecommerce Microservices Platform

## Project Overview

This is a fullstack multi-tenant ecommerce SaaS application built with a microservices architecture.

- **Frontend:** Next.js 16+ (App Router, TypeScript, Tailwind CSS, shadcn/ui)
- **Backend:** ASP.NET Core 9 microservices with minimal APIs
- **Infrastructure:** PostgreSQL, Redis, RabbitMQ, Jaeger, Prometheus, Grafana
- **Orchestration:** Docker Compose

## Architecture

### Service Boundaries (Bounded Contexts)

| Service | Responsibility | Database | Port |
|---------|---------------|----------|------|
| **Gateway** | YARP reverse proxy, routing, correlation ID propagation | N/A | 5000 |
| **IdentityService** | Stub JWT auth, tenants, users, multi-tenancy | identity_db | 5001 |
| **CatalogService** | Products, variants, categories, media | catalog_db | 5002 |
| **InventoryService** | Stock levels, reservations, releases | inventory_db | 5003 |
| **OrderService** | Order lifecycle, state machine, saga orchestration | order_db | 5004 |
| **CustomerService** | Customer profiles, addresses, order history | customer_db | 5005 |
| **PaymentService** | Payment intent simulation, idempotency, webhooks | payment_db | 5006 |
| **NotificationService** | Email/SMS consumers, webhook dispatchers | notification_db | 5007 |

### Communication Patterns

- **Sync:** REST APIs for request/response (reads, simple writes)
- **Async:** MassTransit 8 + RabbitMQ for cross-aggregate operations and event-driven updates
- **Gateway:** Routes `/api/identity/*`, `/api/catalog/*`, `/api/inventory/*`, `/api/orders/*`, `/api/customers/*`, `/api/payments/*`, `/api/notifications/*`

### Data Strategy

- **Database per Service:** Each service owns its PostgreSQL database exclusively
- **No shared databases** between services
- **Eventual consistency** via messaging for cross-service data synchronization
- **Redis** for caching and session storage
- **Row-level tenant isolation** via `TenantId` column on every entity

### Observability

- **Distributed Tracing:** OpenTelemetry + Jaeger (port 16686)
- **Metrics:** Prometheus (port 9090) + Grafana (port 3001)
- **Structured Logging:** Serilog with JSON console output
- **Health Checks:** Every service exposes `/health/live` and `/health/ready`
- **Correlation IDs:** Propagated across all requests via `X-Correlation-ID` header
- **API Documentation:** Scalar UI at `/scalar` (replaces Swagger)

## Tech Stack & Skills

### Loaded Skills
- `microservices-architect` - DDD, service decomposition, resilience patterns
- `next-best-practices` - Next.js 16 App Router, RSC, async APIs, self-hosting
- `aspnet-minimal-api-openapi` - Minimal APIs, OpenAPI/Scalar docs
- `dotnet-best-practices` - C# 12 features, DI, async/await, testing standards

### Key Libraries
- **Frontend:** next, react, tailwindcss, shadcn/ui, lucide-react, @tanstack/react-query, zustand
- **Backend:** ASP.NET Core 9, EF Core, Npgsql, YARP, OpenTelemetry, JWT Bearer, MassTransit, MediatR, Serilog, Scalar
- **Infrastructure:** PostgreSQL 17, Redis 7, RabbitMQ 4

## Project Structure

```
/
├── docker-compose.yml          # Infrastructure orchestration
├── frontend/                   # Next.js application
│   ├── src/app/                # App Router pages
│   ├── src/components/ui/      # shadcn/ui components
│   └── next.config.ts          # Standalone output for Docker
├── backend/
│   ├── SaaSMicroservices.sln   # .NET Solution
│   ├── Directory.Build.props   # Shared MSBuild properties
│   ├── Directory.Packages.props # Centralized package versions
│   ├── src/
│   │   ├── Gateway/            # YARP API Gateway
│   │   ├── Services/
│   │   │   ├── IdentityService/
│   │   │   ├── CatalogService/
│   │   │   ├── InventoryService/
│   │   │   ├── OrderService/
│   │   │   ├── CustomerService/
│   │   │   ├── PaymentService/
│   │   │   └── NotificationService/
│   │   └── Shared/SaaSCommon/  # Shared technical libraries
│   └── docker/                 # Docker configs (Prometheus, Grafana, Postgres init)
```

## Design System

The frontend follows the design system defined in `DESIGN.md`:

- **Colors:** Warm terracotta accent (`#C8956C`), ink neutrals, semantic colors
- **Typography:** Playfair Display (headings), DM Sans (body), JetBrains Mono (mono)
- **Spacing:** 4px base unit, 12-column grid on desktop
- **Components:** shadcn/ui base with DESIGN.md token overrides in `globals.css`

## Development Workflow

### Prerequisites
- Node.js 20+ and npm
- .NET 9 SDK
- Docker and Docker Compose

### Running Infrastructure
```bash
docker-compose up -d
```
This starts: PostgreSQL, Redis, RabbitMQ, Jaeger, Prometheus, Grafana

### Running Backend Services
```bash
cd backend
# Restore dependencies
dotnet restore
# Run individual services
dotnet run --project src/Services/CatalogService
# Or run all via Docker Compose (includes services)
docker-compose up -d gateway identityservice catalogservice inventoryservice orderservice customerservice paymentservice notificationservice
```

### Running Frontend
```bash
cd frontend
npm install
npm run dev
```

### API Documentation
Each service exposes Scalar UI at `/scalar` when running in Development mode.

## Conventions

### Backend
- Use **minimal APIs** with explicit request/response DTOs (records)
- Apply **XML documentation comments** to all public APIs
- Use **primary constructors** for DI
- Implement **health checks** on every service
- Add **correlation ID middleware** to all HTTP pipelines
- Use **async/await** for all I/O operations
- Apply **FluentValidation** for input validation (wired but not yet implemented per endpoint)
- Follow **database-per-service** strictly
- Use **Serilog** for structured logging
- Add **OpenTelemetry tracing and runtime metrics**
- Use **AsNoTracking** for all read queries

### Frontend
- Use **Server Components** by default
- Add `'use client'` only when browser APIs or hooks are needed
- Use **next/image** for all images
- Implement **health check** at `/api/health`
- Configure **standalone output** for Docker
- Use **shadcn/ui** components for UI consistency
- Use **TanStack Query** for server state
- Use **Zustand** for client state (cart, UI preferences)

### MUST DO
- Keep business logic inside service boundaries
- Use events for cross-service communication
- Implement idempotency for message handlers
- Add distributed tracing to all new endpoints
- Write tests for both success and failure scenarios
- Enforce tenant isolation on every query

### MUST NOT DO
- Share databases between services
- Make synchronous calls across services for long-running operations
- Skip health checks or observability
- Create distributed monoliths (tight coupling)
- Store secrets in code (use appsettings + environment variables)
- Use raw SQL without parameterization

## Known Architectural Decisions

1. **Scalar over Swashbuckle:** .NET 9 built-in OpenAPI + Scalar.AspNetCore replaces SwaggerUI
2. **MassTransit over raw RabbitMQ:** Provides sagas, outbox, consumer discovery, retry policies
3. **Stub JWT auth:** No real identity provider; hardcoded signing key for development only
4. **Payment simulation:** No real Stripe integration; test card heuristics determine success/failure
5. **Evolutionary refactor:** Existing services mutated in place rather than clean-slate rewrite
