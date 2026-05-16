# SaaS Microservices Platform

## Project Overview

This is a fullstack SaaS (Software as a Service) application built with a microservices architecture.

- **Frontend:** Next.js 16+ (App Router, TypeScript, Tailwind CSS, shadcn/ui)
- **Backend:** ASP.NET Core 9 microservices with minimal APIs
- **Infrastructure:** PostgreSQL, Redis, RabbitMQ, Jaeger, Prometheus, Grafana
- **Orchestration:** Docker Compose

## Architecture

### Service Boundaries (Bounded Contexts)

| Service | Responsibility | Database | Port |
|---------|---------------|----------|------|
| **Gateway** | YARP reverse proxy, routing, correlation ID propagation | N/A | 5000 |
| **IdentityService** | Authentication, authorization, users, multi-tenancy | identity_db | 5001 |
| **BillingService** | Subscription plans, billing, invoicing | billing_db | 5002 |
| **NotificationService** | Email, SMS, push notifications, webhooks | notification_db | 5003 |
| **CoreService** | Main business domain logic (extendable) | core_db | 5004 |

### Communication Patterns

- **Sync:** REST APIs for request/response (reads, simple writes)
- **Async:** RabbitMQ for cross-aggregate operations and event-driven updates
- **Gateway:** Routes `/api/identity/*`, `/api/billing/*`, `/api/notifications/*`, `/api/core/*`

### Data Strategy

- **Database per Service:** Each service owns its PostgreSQL database exclusively
- **No shared databases** between services
- **Eventual consistency** via messaging for cross-service data synchronization
- **Redis** for caching and session storage

### Observability

- **Distributed Tracing:** OpenTelemetry + Jaeger (port 16686)
- **Metrics:** Prometheus (port 9090) + Grafana (port 3001)
- **Health Checks:** Every service exposes `/health/live` and `/health/ready`
- **Correlation IDs:** Propagated across all requests via `X-Correlation-ID` header

## Tech Stack & Skills

### Loaded Skills
- `microservices-architect` - DDD, service decomposition, resilience patterns
- `next-best-practices` - Next.js 16 App Router, RSC, async APIs, self-hosting
- `aspnet-minimal-api-openapi` - Minimal APIs, OpenAPI/Swagger docs
- `dotnet-best-practices` - C# 12 features, DI, async/await, testing standards

### Key Libraries
- **Frontend:** next, react, tailwindcss, shadcn/ui, lucide-react
- **Backend:** ASP.NET Core 9, EF Core, Npgsql, YARP, OpenTelemetry, JWT Bearer
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
│   │   │   ├── BillingService/
│   │   │   ├── NotificationService/
│   │   │   └── CoreService/
│   │   └── Shared/SaaSCommon/  # Shared technical libraries
│   └── docker/                 # Docker configs (Prometheus, Grafana, Postgres init)
```

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
dotnet run --project src/Services/IdentityService
# Or run all via Docker Compose (includes services)
docker-compose up -d gateway identityservice billingservice notificationservice coreservice
```

### Running Frontend
```bash
cd frontend
npm install
npm run dev
```

### API Documentation
Each service exposes OpenAPI/Swagger UI at `/swagger` when running in Development mode.

## Conventions

### Backend
- Use **minimal APIs** with explicit request/response DTOs (records)
- Apply **XML documentation comments** to all public APIs
- Use **primary constructors** for DI
- Implement **health checks** on every service
- Add **correlation ID middleware** to all HTTP pipelines
- Use **async/await** for all I/O operations
- Apply **FluentValidation** for input validation
- Follow **database-per-service** strictly

### Frontend
- Use **Server Components** by default
- Add `'use client'` only when browser APIs or hooks are needed
- Use **next/image** for all images
- Implement **health check** at `/api/health`
- Configure **standalone output** for Docker
- Use **shadcn/ui** components for UI consistency

### MUST DO
- Keep business logic inside service boundaries
- Use events for cross-service communication
- Implement idempotency for message handlers
- Add distributed tracing to all new endpoints
- Write tests for both success and failure scenarios

### MUST NOT DO
- Share databases between services
- Make synchronous calls across services for long-running operations
- Skip health checks or observability
- Create distributed monoliths (tight coupling)
- Store secrets in code (use appsettings + environment variables)
