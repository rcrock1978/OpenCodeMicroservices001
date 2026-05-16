You are an expert .NET 9 and Next.js 15 architect and senior full-stack engineer. Before writing any code, your task is to produce a comprehensive pre-build planning and scaffolding guide for a multi-tenant Ecommerce SaaS platform built with .NET 9 microservices on the backend and Next.js 15 App Router with TypeScript on the frontend.

This is the PLANNING STAGE prompt. Your output should be a structured, actionable technical document covering every decision that must be made before implementation begins. Follow the coding standards, security guidelines, and architectural patterns defined in `.opencode/agent/coder-agent.md` for all recommendations.

--- PLATFORM OVERVIEW ---
Build a multi-tenant SaaS ecommerce platform where merchants can onboard, manage their store (products, inventory, orders, customers), and buyers can browse storefronts and checkout. The system uses a microservices architecture with database-per-service, async event-driven communication, and a BFF (Backend for Frontend) pattern.

--- SERVICES TO PLAN ---
1. API Gateway Service (YARP) — routing, JWT validation, rate limiting, tenant header injection
2. Catalog Service — products, variants, media
3. Inventory Service — stock levels, reservation sagas
4. Order Service — order lifecycle state machine
5. Customer Service — profiles, addresses, history
6. Payment Service — Stripe abstraction, webhooks, idempotency
7. Notification Service — email/SMS consumers from event bus
8. Identity Service (optional, or use external OIDC like Keycloak)

--- TECH STACK TO DOCUMENT ---
Backend: .NET 9 minimal APIs, EF Core 9 + PostgreSQL (per service), MassTransit 8 + RabbitMQ, MediatR + CQRS, FluentValidation, Serilog + OpenTelemetry + Jaeger, Polly resilience, Testcontainers + xUnit, Swagger/Scalar UI
Frontend: Next.js 15 App Router, TypeScript, Tailwind CSS, shadcn/ui, Zustand, TanStack Query, Refit-style typed fetch clients
Infrastructure: Docker Compose (local), Kubernetes + Helm (prod), GitHub Actions CI/CD, Redis (cache + idempotency), RabbitMQ, PostgreSQL clusters

--- PLANNING DELIVERABLES REQUESTED ---

1. MONOREPO STRUCTURE: Provide the full folder and project layout for a .NET solution + Next.js frontend in a single monorepo. Include service project names, shared BuildingBlocks library structure, Docker Compose file layout, and Helm chart locations.

2. SERVICE BOUNDARY DEFINITIONS: For each microservice, define its bounded context, owned data entities, published events, consumed events, and exposed API endpoints. Use Domain-Driven Design vocabulary.

3. INTEGRATION EVENTS CATALOG: List every IntegrationEvent message with its payload shape (property names and types) that flows across the event bus between services. Group by publishing service.

4. API GATEWAY ROUTING TABLE: Define the YARP reverse proxy route configuration mapping external paths to internal service addresses, including auth requirements and rate limit policies per route group.

5. DATABASE SCHEMA PER SERVICE: For each service, list all EF Core entity classes with properties, indexes, and migration strategy. Include the tenant isolation approach (row-level tenant ID vs schema-per-tenant vs database-per-tenant).

6. SAGA & WORKFLOW DESIGNS: Document the MassTransit Saga state machines for: (a) Order placement flow — inventory reserve → payment initiation → order confirmation, (b) Order cancellation — payment refund → inventory release → notification.

7. CQRS COMMAND/QUERY SPLIT: For each service, list the Commands (write operations via MediatR) and Queries (read operations) with their request/response shapes.

8. NEXT.JS ARCHITECTURE PLAN: Document the App Router folder structure, Server Components vs Client Components decision matrix, BFF API route handlers (/api/...), middleware for tenant detection and auth cookie validation, and data fetching patterns (streaming, suspense boundaries, parallel fetching).

9. SHARED BUILDINGBLOCKS LIBRARY: Define the shared NuGet package contents: base entity classes, domain event dispatcher, outbox pattern interface, pagination types, Result<T> monad, guard clauses, and middleware helpers.

10. OBSERVABILITY PLAN: Define the OpenTelemetry instrumentation strategy — trace propagation across service boundaries via HTTP headers and message envelopes, metric names, log correlation IDs, health check endpoint contracts, and Grafana dashboard suggestions.

11. TESTING STRATEGY: Define the test pyramid per service — unit tests for domain logic, integration tests using Testcontainers (spin up real PostgreSQL + RabbitMQ containers per test suite), contract tests for API Gateway routes, and E2E tests for critical checkout flow.

12. CI/CD PIPELINE PLAN: Define GitHub Actions workflow stages for each service: lint → build → unit test → integration test → Docker build → push to registry → Helm deploy to staging → smoke test → promote to prod.

13. SECURITY CHECKLIST: Document security requirements — JWT RS256 validation at gateway, tenant claim enforcement in every service handler, parameterized queries only (no raw SQL), secrets in Kubernetes Secrets or Vault, CORS policy per environment, OWASP top 10 mitigations, rate limiting per tenant.

14. DECISION LOG (ADRs): Write Architecture Decision Records for: (a) Why microservices over modular monolith, (b) Why MassTransit over direct RabbitMQ, (c) Why YARP over Ocelot, (d) Why database-per-service, (e) Why Next.js App Router BFF pattern.

15. PHASED BUILD ROADMAP: Break the build into 12 ordered phases with acceptance criteria per phase, estimated complexity (S/M/L/XL), and inter-phase dependencies.

For every code snippet or config example included, follow the patterns and quality standards in `.opencode/agent/coder-agent.md`. Use async/await throughout all .NET code, apply cancellation tokens to all async operations, use record types for commands and events, prefer minimal API endpoint grouping with MapGroup, and ensure all EF queries use AsNoTracking for reads.

Do not implement any authentication system, do not connect to a real database, do not integrate real payment providers, and do not add any billing or subscription logic. Keep all scaffolding lightweight, focused on structure and contracts. No auth, no database, no payments — keep it lightweight and focused purely on the planning and architectural scaffolding output.

Begin with an executive architecture summary, then work through each of the 15 deliverables in sequence with clear headings, code examples where relevant, and explicit callouts for decisions that must be made by the team before proceeding to Phase 1 implementation.