# Muvit

A platform connecting customers who need moving/hauling services with independent drivers who offer them. Customers search for drivers, negotiate a price based on distance, and book — all payments go through the platform, never directly to the driver. The platform collects payment, keeps a commission, and pays out the remainder to the driver.

## Status: actively in development

| Layer | Status |
|---|---|
| Domain (`Muvit.Domain`) | ✅ Complete — entities, value objects, enums, 16 passing unit tests |
| Application (`Muvit.Application`) | 🔧 In progress |
| Infrastructure (`Muvit.Infrastructure`) | ⏳ Not started |
| API (`Muvit.Api`) | ⏳ Not started |
| Frontend (Customer / Driver / Admin) | ⏳ Not started |

Full task breakdown and progress: see the [GitHub Project board](https://github.com/users/Kgotso-hub/projects/1).

## Architecture

Clean Architecture / Dependency Inversion across 4 projects, plus a test project:

```
Muvit.Domain/            Entities, value objects, enums, domain interfaces — no dependencies
Muvit.Application/       Application services + their interfaces, DTOs — depends on Domain only
Muvit.Infrastructure/    EF Core, payment gateway, notifications — implements Application's interfaces
Muvit.Api/               ASP.NET Core endpoints, DI wiring — references everything
Muvit.Domain.Tests/      xUnit tests for the Domain layer
```

**Built so far:** a rich domain model with enforced invariants — e.g. `Booking`'s status transitions are validated against an explicit allowed-transition table before any state changes, so an illegal jump (like `Completed → Requested`) is structurally impossible rather than just discouraged by convention.

**Planned, in progress:** Strategy pattern for commission calculation (`ICommissionStrategy`), Repository pattern for persistence, and constructor-injected Dependency Inversion between Application and Infrastructure — these land in the Application/Infrastructure layers currently underway.

## Tech stack

- .NET 10 / C# 13
- ASP.NET Core Web API
- Entity Framework Core + Azure SQL Database
- xUnit

## Running locally

```
dotnet build
dotnet test
```

(API run instructions will be added once `Muvit.Api` has real endpoints.)