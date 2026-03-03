# ModulerMonolith

> A production-ready **Modular Monolith** architecture built with **.NET 10**, demonstrating clean separation of concerns, modern API patterns, and best practices for scalable backend systems.

---

## Overview

This project showcases how to build a **Modular Monolith** — an architectural style that combines the deployment simplicity of a monolith with the internal structure and boundaries of a microservices system. Each module is independently organized with its own domain, application, and endpoint layers, yet shares a single runtime and database.

The goal is to prove that **you don't need to start with microservices** to write clean, maintainable, and scalable code.

---

## Architecture

```
ModulerMonolith/
├── ModulerMonolith.Api/              # Entry point — Minimal API host
├── ModulerMonolith.Core/             # Shared kernel — AuthPolicies, ICurrentUser
├── ModulerMonolith.Infrastructure/   # EF Core DbContext, PostgreSQL migrations
├── Modules/
│   ├── Auth/                         # Authentication & authorization module
│   │   ├── AuthModule.cs             # Service registration + endpoint mapping
│   │   ├── Authorization/            # Keycloak claims transformation
│   │   ├── Dtos/                     # LoginRequest, TokenResponse, etc.
│   │   ├── Options/                  # KeycloakOptions
│   │   └── Services/                 # ITokenService, TokenService, CurrentUser
│   └── Product/                      # Product CRUD module
│       ├── ProductModule.cs          # Service registration + endpoint mapping
│       ├── Domain/                   # Entities + EF configurations
│       ├── Application/              # Service interfaces + implementations
│       └── Endpoints/                # Minimal API endpoint definitions
├── docker/
│   ├── postgres/init.sql             # Creates the keycloak database
│   └── keycloak/realm-export.json   # Auto-imported realm config
├── Dockerfile
└── docker-compose.yml
```

### Module Contract

Every module exposes two static extension methods — nothing more:

```csharp
// Register services
services.AddProductModule(configuration);

// Map endpoints
app.MapProductEndpoints();
```

No reflection, no scanning, no magic. Explicit and traceable.

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
| Containerization | Docker Compose |
| Architecture | Modular Monolith |

> **Note:** EF Core is pinned to 9.0.4 because Npgsql has no stable release for EF Core 10.x yet.

---

## Getting Started

### Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [.NET 10 SDK](https://dotnet.microsoft.com/download) (only for local development)

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

Make sure PostgreSQL and Keycloak are running (via Docker or natively), then:

```bash
dotnet run --project ModulerMonolith.Api
```

The local API uses `appsettings.Development.json` which points to `http://localhost:8080` for Keycloak.

---

## Docker Services

| Service | URL | Credentials |
|---------|-----|-------------|
| API | http://localhost:5020 | — |
| API Reference (Scalar) | http://localhost:5020/scalar/v1 | — |
| Keycloak Admin Console | http://localhost:8080 | admin / admin |
| Seq (Log UI) | http://localhost:8081 | — |
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

---

## Authorization Policies

Policies are defined in `ModulerMonolith.Core/AuthPolicies.cs` and enforced via `RequireAuthorization()` on each endpoint.

| Policy | Requirement |
|--------|-------------|
| `ProductRead` | Authenticated user (any role) |
| `ProductWrite` | Authenticated + role `admin` or `product-manager` |
| `Admin` | Authenticated + role `admin` |

Roles are extracted from the Keycloak `realm_access.roles` claim via `KeycloakClaimsTransformation`.

---

## Configuration

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

When running inside Docker, environment variables in `docker-compose.yml` override these values (e.g. `Keycloak__Authority` becomes `http://keycloak:8080`).

---

## Best Practices Applied

- **Vertical Slice per Module** — each module owns its domain, application logic, and HTTP endpoints
- **Explicit Module Registration** — no runtime assembly scanning or convention-based magic in `Program.cs`
- **EF Core per Module** — each module registers its own `IEntityTypeConfiguration<T>` via `AppDbContext.RegisterModuleAssembly()`
- **FrameworkReference over package spam** — ASP.NET Core types accessed via shared framework, keeping class library projects lean
- **Scalar over Swagger** — modern, clean API reference UI out of the box
- **Structured logging** — Serilog with Seq sink, queryable logs in development
- **No placeholder code** — zero `WeatherForecast`, zero `Class1.cs`, zero dead routes
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

Start modular, extract services only when you have a proven reason to.

---

## License

MIT
