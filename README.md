# SaaS Microservices Platform

A production-ready fullstack SaaS application built with Next.js and ASP.NET Core microservices.

## Architecture

This platform follows Domain-Driven Design (DDD) principles with clearly defined bounded contexts:

- **Identity Service** - Multi-tenant authentication, user management, JWT-based auth
- **Billing Service** - Subscription plans, billing cycles, invoicing
- **Notification Service** - Email, SMS, push notifications, webhooks
- **Core Service** - Extendable domain logic for your specific SaaS product
- **API Gateway** - YARP-based reverse proxy with correlation ID propagation

## Quick Start

### Prerequisites
- Node.js 20+ and npm
- .NET 9 SDK
- Docker & Docker Compose

### 1. Start Infrastructure
```bash
docker-compose up -d postgres redis rabbitmq jaeger prometheus grafana
```

### 2. Start Backend Services
```bash
cd backend
dotnet restore
dotnet run --project src/Services/IdentityService
dotnet run --project src/Services/BillingService
dotnet run --project src/Services/NotificationService
dotnet run --project src/Services/CoreService
dotnet run --project src/Gateway
```

Or use Docker Compose for everything:
```bash
docker-compose up -d
```

### 3. Start Frontend
```bash
cd frontend
npm install
npm run dev
```

Open [http://localhost:3000](http://localhost:3000) to view the dashboard.

## Service Endpoints

| Service | Local URL | Docker URL |
|---------|-----------|------------|
| Gateway | http://localhost:5000 | http://localhost:5000 |
| Identity | http://localhost:5001 | http://localhost:5001 |
| Billing | http://localhost:5002 | http://localhost:5002 |
| Notifications | http://localhost:5003 | http://localhost:5003 |
| Core | http://localhost:5004 | http://localhost:5004 |

## Observability

- **Jaeger UI:** http://localhost:16686 (Distributed Tracing)
- **Prometheus:** http://localhost:9090 (Metrics)
- **Grafana:** http://localhost:3001 (Dashboards, login: admin/admin)

## Documentation

See [AGENTS.md](./AGENTS.md) for detailed architecture decisions, coding conventions, and agent guidelines.
