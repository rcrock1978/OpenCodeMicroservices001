# SaaS Ecommerce Microservices Platform

A fullstack multi-tenant ecommerce application built with Next.js and ASP.NET Core microservices, following Domain-Driven Design (DDD) principles.

## Overview

This platform provides a modern ecommerce experience with a microservices architecture. Each service owns its own database and communicates via REST APIs and asynchronous messaging. The system supports multi-tenancy, distributed tracing, and event-driven updates.

- **Frontend:** Next.js 16+ (App Router, TypeScript, Tailwind CSS, shadcn/ui)
- **Backend:** ASP.NET Core 10 microservices with minimal APIs
- **Infrastructure:** PostgreSQL 17, Redis 7, RabbitMQ 4, Jaeger, Prometheus, Grafana
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

- **Sync:** REST APIs via the Gateway for request/response (reads, simple writes)
- **Async:** MassTransit + RabbitMQ for cross-aggregate operations and event-driven updates
- **Gateway Routes:** `/api/identity/*`, `/api/catalog/*`, `/api/inventory/*`, `/api/orders/*`, `/api/customers/*`, `/api/payments/*`, `/api/notifications/*`

### Data Strategy

- **Database per Service:** Each service owns its PostgreSQL database exclusively
- **No shared databases** between services
- **Eventual consistency** via messaging for cross-service data synchronization
- **Redis** for caching and session storage
- **Row-level tenant isolation** via `TenantId` column on every entity

## Quick Start

### Prerequisites
- Node.js 20+ and npm
- .NET 10 SDK
- Docker & Docker Compose

### 1. Start Infrastructure
```bash
docker-compose up -d
```
This starts: PostgreSQL, Redis, RabbitMQ, Jaeger, Prometheus, Grafana

### 2. Start Backend Services
```bash
cd backend
dotnet restore

# Run individual services
dotnet run --project src/Services/IdentityService
dotnet run --project src/Services/CatalogService
dotnet run --project src/Services/InventoryService
dotnet run --project src/Services/OrderService
dotnet run --project src/Services/CustomerService
dotnet run --project src/Services/PaymentService
dotnet run --project src/Services/NotificationService
dotnet run --project src/Gateway
```

Or use Docker Compose for everything:
```bash
docker-compose up -d gateway identityservice catalogservice inventoryservice orderservice customerservice paymentservice notificationservice
```

### 3. Start Frontend
```bash
cd frontend
npm install
npm run dev
```

Open [http://localhost:3000](http://localhost:3000) to view the application.

## Service Endpoints

| Service | Local URL | API Docs |
|---------|-----------|----------|
| Gateway | http://localhost:5050 | — |
| Identity | http://localhost:5001 | http://localhost:5001/scalar |
| Catalog | http://localhost:5002 | http://localhost:5002/scalar |
| Inventory | http://localhost:5003 | http://localhost:5003/scalar |
| Orders | http://localhost:5004 | http://localhost:5004/scalar |
| Customers | http://localhost:5005 | http://localhost:5005/scalar |
| Payments | http://localhost:5006 | http://localhost:5006/scalar |
| Notifications | http://localhost:5007 | http://localhost:5007/scalar |

## Observability

- **Jaeger UI:** http://localhost:16686 (Distributed Tracing)
- **Prometheus:** http://localhost:9090 (Metrics)
- **Grafana:** http://localhost:3001 (Dashboards, login: admin/admin)
- **RabbitMQ Management:** http://localhost:15672 (Messaging)

Every service exposes health checks at:
- `/health/live` — Liveness probe
- `/health/ready` — Readiness probe

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
└── DESIGN.md                   # Frontend design system specification
```

## Documentation

- **[AGENTS.md](./AGENTS.md)** — Detailed architecture decisions, coding conventions, and development guidelines
- **[DESIGN.md](./DESIGN.md)** — Frontend design system (colors, typography, components, page layouts)

## Key Libraries

- **Frontend:** next, react, tailwindcss, shadcn/ui, lucide-react, @tanstack/react-query, zustand
- **Backend:** ASP.NET Core 10, EF Core, Npgsql, YARP, OpenTelemetry, JWT Bearer, MassTransit, MediatR, Serilog, Scalar
- **Infrastructure:** PostgreSQL 17, Redis 7, RabbitMQ 4
