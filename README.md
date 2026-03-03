# ModulerMonolith

> A production-ready **Modular Monolith** architecture built with **.NET 10**, demonstrating clean module boundaries, event-driven patterns, and microservice-readiness without microservice complexity.

---

## Overview

This project showcases how to build a **Modular Monolith** — an architectural style that combines the deployment simplicity of a monolith with the internal structure and boundaries of a microservices system.

Each module is independently organized with its own domain, application, and endpoint layers. Modules **never reference each other** — cross-module communication happens exclusively through events via the Outbox Pattern.

The goal is to prove that **you don't need to start with microservices** to write clean, maintainable, and scalable code. When the time comes to extract a module into its own service, the boundary is already there.

---

## Architecture

```
ModulerMonolith/
├── ModulerMonolith.Api/              # Entry point — Minimal API host
├── ModulerMonolith.Core/             # Shared kernel — AuthPolicies, ICurrentUser
├── ModulerMonolith.Infrastructure/   # EF Core DbContext, Outbox Pattern, PostgreSQL
│   └── Outbox/
│       ├── OutboxMessage.cs          # Entity persisted to outbox_messages table
│       ├── OutboxConfiguration.cs    # EF config — jsonb payload column
│       ├── IOutboxService.cs         # Public interface used by domain modules
│       ├── OutboxService.cs          # Adds messages to DbContext change tracker
│       └── OutboxProcessor.cs        # BackgroundService — polls DB, sends to n8n
├── Modules/
│   ├── Auth/                         # Authentication & authorization module
│   │   ├── AuthModule.cs
│   │   ├── Authorization/            # Keycloak claims transformation
│   │   ├── Dtos/                     # LoginRequest, TokenResponse, etc.
│   │   └── Services/                 # ITokenService, TokenService, CurrentUser
│   ├── Product/                      # Product CRUD module
│   │   ├── ProductModule.cs
│   │   ├── Domain/                   # Product entity + EF configuration
│   │   ├── Application/              # IProductService, ProductService
│   │   └── Endpoints/
│   └── Order/                        # Order module (event source)
│       ├── OrderModule.cs
│       ├── Domain/                   # Order, OrderItem entities + EF configuration
│       ├── Application/              # IOrderService, OrderService
│       └── Endpoints/
├── docker/
│   ├── postgres/init.sql             # Creates the keycloak database
│   ├── keycloak/realm-export.json   # Auto-imported realm + audience mapper
│   └── n8n/workflow.json            # Importable Saga Orchestrator workflow
├── Dockerfile
└── docker-compose.yml
```

### Module Contract

Every module exposes exactly two static extension methods — nothing more:

```csharp
// Register services
services.AddOrderModule(configuration);

// Map endpoints
app.MapOrderEndpoints();
```

No reflection, no scanning, no magic. Explicit and traceable.

### Module Isolation Rule

Modules **do not reference each other's assemblies**. Cross-module data needs are solved by:

- **Snapshot pattern**: `OrderItem` stores `ProductId` (Guid) without a foreign key. Product name and price are copied at order time, not resolved at query time.
- **Event-driven**: When a business event needs to cross module boundaries, it goes through the Outbox → n8n, not a direct method call.

---

## Tech Stack

| Layer | Technology |
|---|---|
| Runtime | .NET 10 |
| API Style | Minimal APIs |
| ORM | Entity Framework Core 9.0.4 |
| Database | PostgreSQL 17 |
| Authentication | Keycloak 26 + JWT Bearer |
| API Documentation | Scalar (OpenAPI) |
| Logging | Serilog + Seq |
| Event Delivery | Outbox Pattern + BackgroundService |
| Workflow Orchestration | n8n |
| Containerization | Docker Compose |
| Architecture | Modular Monolith |

> **Note:** EF Core is pinned to 9.0.4 because Npgsql has no stable release for EF Core 10.x yet.

---

## Getting Started

### Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [.NET 10 SDK](https://dotnet.microsoft.com/download) (only for local development outside Docker)

### Run with Docker (recommended)

```bash
git clone https://github.com/mikailTorun/ModulerMonolith.git
cd ModulerMonolith
docker compose up -d
```

Wait ~60 seconds for Keycloak to initialize, then open the API reference:

```
http://localhost:5020/scalar/v1
```

### Run locally (without Docker API)

Make sure PostgreSQL, Keycloak, Seq, and n8n are running (via Docker), then:

```bash
dotnet run --project ModulerMonolith.Api
```

The local API uses `appsettings.Development.json` which points to `http://localhost:8080` for Keycloak and `http://localhost:5678` for n8n.

---

## Docker Services

| Service | URL | Credentials |
|---------|-----|-------------|
| API | http://localhost:5020 | — |
| API Reference (Scalar) | http://localhost:5020/scalar/v1 | — |
| Keycloak Admin Console | http://localhost:8080 | admin / admin |
| Seq (Log UI) | http://localhost:8081 | — |
| n8n (Workflow UI) | http://localhost:5678 | — |
| PostgreSQL | localhost:5432 | postgres / postgres |

---

## Authentication

Authentication is handled by **Keycloak** using the ROPC (Resource Owner Password Credentials) grant. The realm `modulermonolith` is auto-imported on first start.

### Test Users

| Username | Password | Roles |
|----------|----------|-------|
| `admin` | `Admin123!` | admin, product-manager |
| `manager` | `Manager123!` | product-manager |
| `user` | `User123!` | — |

### Get a Token

```bash
curl -s -X POST http://localhost:5020/api/auth/token \
  -H "Content-Type: application/json" \
  -d '{"username": "admin", "password": "Admin123!"}'
```

Use the returned `access_token` as a Bearer token in subsequent requests.

---

## Endpoints

### Auth

| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| `POST` | `/api/auth/token` | Public | Login — returns access + refresh token |
| `POST` | `/api/auth/token/refresh` | Public | Refresh access token |
| `POST` | `/api/auth/logout` | Public | Revoke refresh token |
| `GET` | `/api/auth/me` | Required | Returns current user claims |

### Products

| Method | Route | Required Role | Description |
|--------|-------|---------------|-------------|
| `GET` | `/api/products` | Any authenticated | List all products |
| `GET` | `/api/products/{id}` | Any authenticated | Get product by ID |
| `POST` | `/api/products` | admin, product-manager | Create a new product |
| `PUT` | `/api/products/{id}` | admin, product-manager | Update a product |
| `DELETE` | `/api/products/{id}` | admin, product-manager | Delete a product |

### Orders

| Method | Route | Required Role | Description |
|--------|-------|---------------|-------------|
| `GET` | `/api/orders` | Any authenticated | List all orders |
| `GET` | `/api/orders/{id}` | Any authenticated | Get order by ID |
| `POST` | `/api/orders` | Any authenticated | Create a new order |
| `PUT` | `/api/orders/{id}/confirm` | admin, product-manager | Confirm an order |
| `PUT` | `/api/orders/{id}/cancel` | admin, product-manager | Cancel an order |

---

## Authorization Policies

Policies are defined in `ModulerMonolith.Core/AuthPolicies.cs` and enforced via `RequireAuthorization()` on each endpoint.

| Policy | Requirement |
|--------|-------------|
| `ProductRead` | Authenticated user (any role) |
| `ProductWrite` | Authenticated + role `admin` or `product-manager` |
| `OrderRead` | Authenticated user (any role) |
| `OrderWrite` | Authenticated + role `admin` or `product-manager` |
| `Admin` | Authenticated + role `admin` |

Roles are extracted from the Keycloak `realm_access.roles` claim via `KeycloakClaimsTransformation`.

---

## Outbox Pattern

The Outbox Pattern guarantees that **business state changes and domain events are always committed together** — or not at all.

### The Problem It Solves

Without Outbox:
```
1. Order saved to DB          ✓
2. Send event to n8n          ✗  ← app crashes here
```
Result: Order exists but the downstream workflow never runs. Inconsistency.

### How It Works

```
OrderService.CreateAsync()
    │
    ├── _context.Set<Order>().Add(order)
    ├── _outbox.Add("OrderCreated", payload)   ← no SaveChanges yet
    │
    └── await _context.SaveChangesAsync()
              │
              └── BEGIN TRANSACTION
                    INSERT INTO orders (...)
                    INSERT INTO outbox_messages (...)   ← same commit
                  COMMIT
```

`IOutboxService.Add()` only stages the entity in the DbContext change tracker. The caller controls the transaction via `SaveChangesAsync()`. Either both records land in the DB, or neither does.

### OutboxProcessor (BackgroundService)

Every 5 seconds, a background loop runs:

```
SELECT * FROM outbox_messages
WHERE processed_at IS NULL AND retry_count < 5
ORDER BY created_at LIMIT 20

  → POST {N8n:WebhookUrl}
       body: { eventType, payload, createdAt }

  → Success: processed_at = NOW()
  → Failure: retry_count++, error = message
```

Max 5 retries per message. Failed messages are visible in Seq logs.

### Database Schema

```sql
CREATE TABLE outbox_messages (
    id            UUID PRIMARY KEY,
    event_type    VARCHAR(100) NOT NULL,
    payload       JSONB NOT NULL,        -- PostgreSQL native JSON, indexable
    created_at    TIMESTAMPTZ NOT NULL,
    processed_at  TIMESTAMPTZ,           -- NULL = pending
    retry_count   INT NOT NULL DEFAULT 0,
    error         VARCHAR(2000)
);

CREATE INDEX ON outbox_messages (processed_at, retry_count);
```

---

## n8n & Saga Pattern

### What is a Saga?

A **Saga** coordinates a multi-step business process where each step can fail. If any step fails, previously completed steps are **compensated** (rolled back via business logic, not DB transactions).

| Step | Forward Action | Compensating Action |
|------|---------------|---------------------|
| 1 | Create order | Cancel order |
| 2 | Reserve stock | Restore stock |
| 3 | Charge payment | Refund payment |

If step 3 fails, the Saga runs compensation for step 2 and step 1 in reverse order.

### Two Saga Styles

**Choreography** — services emit events, others react. No central coordinator. Hard to debug as complexity grows.

**Orchestration** — a central coordinator (the Saga Orchestrator) drives each step and handles compensation. Easier to reason about. **This project uses this style via n8n.**

### n8n as Saga Orchestrator

n8n acts as the Saga Orchestrator. It receives events from the API via webhook and decides what to do next.

```
OrderCreated event
        ↓
  n8n Webhook
        ↓
  Is OrderCreated? (IF node)
        │
        ├── YES
        │     ↓
        │   totalAmount > 50.000? (Saga: Stock Check)
        │     │
        │     ├── YES → "Out of stock" scenario (compensation)
        │     │           ↓
        │     │         Get Auth Token  POST /api/auth/token
        │     │           ↓
        │     │         Cancel Order    PUT /api/orders/{id}/cancel
        │     │
        │     └── NO  → Order OK, no action
        │
        └── NO → Ignore other event types
```

The threshold `totalAmount > 50.000` simulates a stock check failure. In a real system this node would call an Inventory service.

### Import the Workflow

1. Start the stack: `docker compose up -d`
2. Open n8n: http://localhost:5678
3. Click **Add workflow** → **Import from file**
4. Select `docker/n8n/workflow.json`
5. Click **Publish**

The webhook is now active at `http://localhost:5678/webhook/modulermonolith-events`.

---

## Testing the Full Flow

### 1. Start everything

```bash
docker compose up -d
```

Wait ~60 seconds for Keycloak to be healthy.

### 2. Import and publish the n8n workflow

See [Import the Workflow](#import-the-workflow) above.

### 3. Get an auth token

```bash
TOKEN=$(curl -s -X POST http://localhost:5020/api/auth/token \
  -H "Content-Type: application/json" \
  -d '{"username": "admin", "password": "Admin123!"}' \
  | jq -r '.access_token')
```

### 4. Create a product (for reference)

```bash
curl -s -X POST http://localhost:5020/api/products \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Gaming PC",
    "description": "High-end gaming computer",
    "price": 75000,
    "stock": 10
  }'
```

Note the returned `id`.

### 5a. Normal order (totalAmount ≤ 50.000) — Saga does NOT fire

```bash
curl -s -X POST http://localhost:5020/api/orders \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "items": [
      {
        "productId": "<product-id>",
        "productName": "Gaming PC",
        "unitPrice": 30000,
        "quantity": 1
      }
    ]
  }'
```

Wait 5 seconds. Check the order status — it should remain `Pending`.

### 5b. High-value order (totalAmount > 50.000) — Saga fires, order gets cancelled

```bash
curl -s -X POST http://localhost:5020/api/orders \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "items": [
      {
        "productId": "<product-id>",
        "productName": "Gaming PC",
        "unitPrice": 75000,
        "quantity": 1
      }
    ]
  }'
```

Note the returned `id`, then wait ~5 seconds and fetch the order:

```bash
curl -s http://localhost:5020/api/orders/<order-id> \
  -H "Authorization: Bearer $TOKEN"
```

The `status` field should be `Cancelled` — the n8n Saga compensated automatically.

### 6. Verify in Seq

Open http://localhost:8081 and filter logs:

```
OutboxProcessor
```

You will see entries like:
```
Outbox: OrderCreated [<id>] sent to n8n successfully
```

### 7. Verify in n8n

Open http://localhost:5678 → click **Executions** (left sidebar) to see the full workflow execution history, including each node's input/output data.

### 8. Inspect the outbox_messages table directly

Connect to PostgreSQL (`localhost:5432`, database `modulermonolith`) and run:

```sql
SELECT id, event_type, processed_at, retry_count, error
FROM outbox_messages
ORDER BY created_at DESC;
```

- `processed_at IS NOT NULL` → successfully delivered to n8n
- `retry_count > 0` → had failures (check `error` column)
- `processed_at IS NULL AND retry_count >= 5` → dead letter, manual investigation needed

---

## Configuration

### `appsettings.json`

```json
{
  "AllowedHosts": "*",
  "N8n": {
    "WebhookUrl": "http://localhost:5678/webhook/modulermonolith-events"
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft.AspNetCore": "Warning",
        "Microsoft.EntityFrameworkCore": "Warning"
      }
    }
  }
}
```

### `appsettings.Development.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=modulermonolith;Username=postgres;Password=postgres"
  },
  "Keycloak": {
    "Authority": "http://localhost:8080",
    "Realm": "modulermonolith",
    "ClientId": "modulermonolith",
    "ClientSecret": ""
  },
  "Seq": {
    "ServerUrl": "http://localhost:5341"
  }
}
```

When running inside Docker, environment variables in `docker-compose.yml` override these values automatically (e.g. `N8n__WebhookUrl` becomes `http://n8n:5678/webhook/modulermonolith-events`).

---

## Best Practices Applied

- **Vertical Slice per Module** — each module owns its domain, application logic, and HTTP endpoints
- **Explicit Module Registration** — no runtime assembly scanning in `Program.cs`
- **EF Core per Module** — each module registers its own `IEntityTypeConfiguration<T>` via `AppDbContext.RegisterModuleAssembly()`
- **Outbox Pattern** — events and business state committed in one transaction, delivered reliably by a background processor
- **Saga Orchestration** — compensating transactions coordinated externally (n8n), not via direct service-to-service calls
- **Module Isolation** — modules share no assembly references; cross-module data resolved via snapshot or events
- **FrameworkReference over package spam** — ASP.NET Core types accessed via shared framework
- **Scalar over Swagger** — modern, clean API reference UI
- **Structured logging** — Serilog with Seq sink, queryable logs in development
- **Nullable reference types** — enabled across all projects

---

## Why Modular Monolith?

| | Monolith | Modular Monolith | Microservices |
|---|---|---|---|
| Deployment complexity | Low | Low | High |
| Internal boundaries | None | Strong | Strong |
| Network overhead | None | None | High |
| Team scalability | Low | Medium | High |
| Operational overhead | Low | Low | Very High |
| Microservice migration | Hard | Easy | — |

Start modular, extract services only when you have a proven reason to. The Outbox + n8n setup means event-driven communication is already in place — extracting a module into its own service requires changing only the HTTP endpoint URL in n8n, not the module's internal code.

---

## License

MIT
