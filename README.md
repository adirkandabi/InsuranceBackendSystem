# AiGent - Insurance Policy Management System

An enterprise-grade Insurance Backend System built with **.NET** following **Clean Architecture** principles. The system manages customers and their insurance policies, enforces domain validation rules, and exposes business intelligence metrics for insurance agents.

---

## Setup

### Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) (8.0 or later)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (for SQL Server)

### 1. Start the database

The database runs in a Docker container. From the repository root:

```bash
docker compose up -d
```

This starts SQL Server on `localhost:1433` with the credentials configured in `docker-compose.yml` and `appsettings.json`.

### 2. Apply database migrations

Generate the schema in your local SQL Server instance:

```bash
dotnet ef database update --project AiGent.Infrastructure --startup-project AiGent.API
```

### 3. Run the API

```bash
dotnet run --project AiGent.API
```

The API listens on **http://localhost:5237** (see `launchSettings.json`).

### 4. Open Swagger

Navigate to the interactive API documentation:

**http://localhost:5237/swagger**

### Recommended testing workflow

When testing with Swagger or Postman, follow this order:

1. **Create customer** — `POST /api/Customers`. Copy the returned `id` (GUID).
2. **Issue policy** — `POST /api/Policies` with the customer GUID in `customerId`. Set `endDate` at least 6 months after `startDate`.
3. **Check KPIs** — `GET /api/Dashboard/stats` to verify ARR and policy counts by type.
4. **Test constraints** — `DELETE /api/Customers/{id}` on a customer with an active policy returns `400 BadRequest`.

---

## Architecture

The solution (`AiGentInsurance.slnx`) is structured according to **Clean Architecture**, keeping domain logic decoupled from infrastructure and presentation concerns.

```text
       [ AiGent.API ]          Controllers, DTOs, Swagger
             │
             ▼ (references)
       [ AiGent.Core ]         Entities, interfaces, domain services
             ▲
             │ (implements)
 [ AiGent.Infrastructure ]     EF Core, DbContext, repositories
```

### Project layers

**AiGent.Core (Domain / Application)**

- Domain entities: `Customer`, `Policy`
- Enums: `PolicyType`, `PolicyStatus`
- Repository and service interfaces (`ICustomerService`, `IPolicyRepository`, etc.)
- Business logic and validation, with no external framework dependencies

**AiGent.Infrastructure (Data access)**

- `InsuranceDbContext` and EF Core migrations
- Repository implementations with `.AsNoTracking()` on read-only queries
- Unique indexes and relationship configuration in `OnModelCreating`

**AiGent.API (Presentation)**

- RESTful endpoints (`POST`, `GET`, `PUT`, `PATCH`, `DELETE`) with appropriate HTTP status codes (`201 Created`, `204 NoContent`, `400 BadRequest`, `404 NotFound`)
- DTOs separate API contracts from internal entity graphs, avoiding serialization cycles

### Data modeling

```text
Customer (1) ─── (N) Policy
```

| Concept | Implementation |
|---------|----------------|
| Relationship | One customer owns many policies (`CustomerId` FK with `DeleteBehavior.Restrict`) |
| Policy cancellation | Soft delete — `Status = Cancelled`, `CancelledAt` timestamp; records are retained |
| Customer deletion | Hard delete allowed only when the customer has **no active policies** |

---

## Assumptions

### Uniqueness constraints

- A customer's **email** and **phone number** are each globally unique (separate unique indexes).
- On create or update, the system rejects email or phone values already assigned to another customer.

### Policy lifecycle and financial integrity

- A policy **start date cannot be in the past** (with a one-day buffer for timezone offsets).
- A policy must run for **at least 6 months** (`endDate` must be more than 6 months after `startDate`).
- **Premium** and **coverage amount** must be strictly greater than zero.
- Policy numbers are auto-generated: `POL-{TYPE}-{YEAR}-{4-CHAR-HEX}` (e.g. `POL-CAR-2026-F8A2`).

### Agent intelligence KPIs

- **Annual Recurring Revenue (ARR)** is the sum of premiums from **active** policies only.
- Dashboard stats include total active policies, active customers (distinct customers with at least one active policy), and policy counts grouped by type.
