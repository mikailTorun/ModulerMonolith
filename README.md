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
├── ModulerMonolith.Core/             # Shared kernel — no framework dependencies
├── ModulerMonolith.Infrastructure/   # EF Core DbContext, persistence setup
└── Modules/
    ├── Auth/                         # Keycloak JWT authentication module
    │   ├── AuthModule.cs             # Service registration + endpoint mapping
    │   └── Options/KeycloakOptions.cs
    └── Product/                      # Product CRUD module
        ├── ProductModule.cs          # Service registration + endpoint mapping
        ├── Domain/                   # Entities + EF configurations
        ├── Application/              # Service interfaces + implementations
        └── Endpoints/                # Minimal API endpoint definitions
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
| ORM | Entity Framework Core 10 |
| Database (dev) | In-Memory |
| Authentication | Keycloak + JWT Bearer |
| API Documentation | Scalar (OpenAPI) |
| Architecture | Modular Monolith |

---

## Best Practices Applied

- **Vertical Slice per Module** — each module owns its domain, application logic, and HTTP endpoints
- **Explicit Module Registration** — no runtime assembly scanning or convention-based magic in `Program.cs`
- **EF Core per Module** — each module registers its own `IEntityTypeConfiguration<T>` via `AppDbContext.RegisterModuleAssembly()`
- **FrameworkReference over package spam** — ASP.NET Core types accessed via shared framework, keeping class library projects lean
- **Scalar over Swagger** — modern, clean API reference UI out of the box
- **No placeholder code** — zero `WeatherForecast`, zero `Class1.cs`, zero dead routes
- **Nullable reference types** — enabled across all projects
- **Sealed classes by default** — domain entities and configurations marked `sealed`

---

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Keycloak](https://www.keycloak.org/) (optional — only needed for `/api/auth/me`)

### Run

```bash
git clone https://github.com/mikailTorun/ModulerMonolith.git
cd ModulerMonolith
dotnet run --project ModulerMonolith.Api
```

### API Reference

Once running, open your browser:

```
http://localhost:5020/scalar/v1
```

---

## Endpoints

### Products

| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/api/products` | List all products |
| `GET` | `/api/products/{id}` | Get product by ID |
| `POST` | `/api/products` | Create a new product |
| `PUT` | `/api/products/{id}` | Update a product |
| `DELETE` | `/api/products/{id}` | Delete a product |

### Auth

| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/api/auth/me` | Get current user claims (requires Bearer token) |

---

## Configuration

Update `appsettings.json` for your Keycloak instance:

```json
{
  "Keycloak": {
    "Authority": "http://localhost:8080",
    "Realm": "master",
    "ClientId": "modulermonolith",
    "ClientSecret": ""
  }
}
```

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
